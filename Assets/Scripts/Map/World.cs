using Prism.Map.Configuration;
using SimplexNoise;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using Prism.NoiseFunc;
using Prism.NoiseConfigurations;
using Prism.Services;

namespace Prism.Map
{
    /// <summary>
    /// This component is in charge of handling the world actions like
    /// creating and rendering chunks, and also to provide informations about
    /// the elements of the world.
    /// </summary>
    public class World : MonoBehaviour
    {
        [Tooltip("Horizontal Render Distance in Chunks")]
        public int RenderDistance = 4;

        public GameObject Player;

        Dictionary<Vector2Int, Chunk> LoadedChunks = new Dictionary<Vector2Int, Chunk>();

        Dictionary<Vector3Int, Chunk> _LoadedChunks = new Dictionary<Vector3Int, Chunk>();
        List<Vector3Int> RenderedChunks = new List<Vector3Int>();

        public WorldConfiguration WorldConfiguration;
        public NoiseMethod NoiseMethod;
        public List<Bloc> BlocsDefinition = new List<Bloc>();

        public GameObject ChunkPrefab;

        public List<object> DebugList = new List<object>();

        // Bloc Service Singleton
        private static BlocService _blocService;
        public static BlocService BlocService
        {
            get
            {
                if (_blocService == null)
                    _blocService = new BlocService();

                return _blocService;
            }
        }
        /// <summary>
        /// Get the chunk's normalized position from the player's world position.
        /// </summary>
        public Vector2Int GetPlayerCurrentChunk => new Vector2Int(
                Mathf.FloorToInt(Player.transform.position.x / WorldConfiguration.ChunkWorldSize),
                Mathf.FloorToInt(Player.transform.position.z / WorldConfiguration.ChunkWorldSize)
        );

        // NEW
        public Vector3Int GetChunkPositionFromWorldPosition(Vector3 worldPosition)
        {
            return new Vector3Int(
                Mathf.FloorToInt(Player.transform.position.x / WorldConfiguration.ChunkWorldSize),
                Mathf.FloorToInt(Player.transform.position.y / WorldConfiguration.ChunkWorldSize),
                Mathf.FloorToInt(Player.transform.position.z / WorldConfiguration.ChunkWorldSize)
            );
        }

        private Vector2Int LastPlayerChunk;

        private IEnumerator CreateWorldFunction;
        private void Start()
        {
            if (!BlocService.Loaded) return;

            foreach (var bloc in BlocService.Blocs)
            {
                Debug.Log($"Bloc : {bloc.Key} | isSolid = {bloc.Value.IsSolid} | isVisible = {bloc.Value.IsVisible}");
            }

            Debug.Log("Generating & remap...");
            CreateWorldFromPosition(Player.transform.position);
            Debug.Log("Done generating the map !");
            //CreateWorldFromPlayerPosition();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                CreateWorldFromPosition(Player.transform.position);
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                using (var sw = new StreamWriter("C:\\Users\\Kaddle\\Desktop\\prims-world-debug.json"))
                {
                    string jsonData = JsonConvert.SerializeObject(DebugList, Formatting.Indented);

                    sw.Write(jsonData);
                    sw.Close();
                }
            }
        }

        public void CreateWorldFromPlayerPosition()
        {
            Vector2Int CurrentChunk = GetPlayerCurrentChunk;

            List<Vector2Int> newVisibleChunks = new List<Vector2Int>();

            for (int x = CurrentChunk.x - RenderDistance; x < CurrentChunk.x + RenderDistance; x++)
                for (int z = CurrentChunk.y - RenderDistance; z < CurrentChunk.y + RenderDistance; z++)
                {
                    var chunkNormalPosition = new Vector2Int(x, z);
                    newVisibleChunks.Add(chunkNormalPosition);

                    LoadChunk(chunkNormalPosition);
                }

            DeleteOldChunk(newVisibleChunks);
        }

        // NEW
        public void CreateWorldFromPosition(Vector3 position)
        {
            var currentChunk = GetChunkPositionFromWorldPosition(position);

            List<Chunk> chunksToRender = new List<Chunk>();
            List<string> newChunks = new List<string>();

            // Load or generate visible chunks
            for (int x = currentChunk.x - WorldConfiguration.HorizontalRenderDistance; x < currentChunk.x + WorldConfiguration.HorizontalRenderDistance; x++)
                for (int y = currentChunk.y - WorldConfiguration.VerticalRenderDistance; y < currentChunk.y + WorldConfiguration.VerticalRenderDistance; y++)
                    for (int z = currentChunk.z - WorldConfiguration.HorizontalRenderDistance; z < currentChunk.z + WorldConfiguration.HorizontalRenderDistance; z++)
                    {
                        var chunkPosition = new Vector3Int(x, y, z);
                        var chunkName = GetChunkNameFromPosition(chunkPosition);

                        newChunks.Add(chunkName);

                        if (!transform.Find(chunkName))
                        {
                            // if File.exists(chunkFile) => LoadChunkFromFile
                            // else

                            var chunk = CreateChunk(chunkPosition);
                            chunksToRender.Add(chunk);
                        }
                    }

            // Destroy current rendered chunks that are not in the render distance range.
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                if (!newChunks.Contains(transform.GetChild(i).name))
                {
                    Destroy(transform.GetChild(i).gameObject);
                }
            }

            // Render new chunks
            foreach (var chunk in chunksToRender)
            {
                RenderChunk(chunk);
            }
        }

        // NEW
        Chunk CreateChunk(Vector3Int position)
        {
            var alreadyExistingChunk = transform.Find(GetChunkNameFromPosition(position));

            if (alreadyExistingChunk)
                return alreadyExistingChunk.GetComponent<Chunk>();

            var chunkWorldPosition = new Vector3(
                position.x * WorldConfiguration.ChunkWorldSize,
                position.y * WorldConfiguration.ChunkWorldSize,
                position.z * WorldConfiguration.ChunkWorldSize
            );

            var chunkGameObject = Instantiate(ChunkPrefab, chunkWorldPosition, Quaternion.Euler(Vector3.zero), transform) as GameObject;
            var chunk = chunkGameObject.GetComponent<Chunk>();

            chunk.Initialize(position, WorldConfiguration.ChunkSideLength);
            
            chunk.Generate(NoiseMethod);

            return chunk;
        }

        // NEW
        public string GetChunkNameFromPosition(Vector3Int position)
        {
            return $"{position.x}_{position.y}_{position.z}";
        }

        void DeleteOldChunk(List<Vector2Int> visibleChunks)
        {
            List<Vector2Int> toDelete = new List<Vector2Int>();
            foreach (var c in LoadedChunks)
            {
                if (!visibleChunks.Contains(c.Key))
                    toDelete.Add(c.Key);
            }

            foreach (var c in toDelete)
            {
                Destroy(LoadedChunks[c].gameObject);
                LoadedChunks.Remove(c);
            }
        }

        public void LoadChunk(Vector2Int chunkNormalPosition)
        {
            if (LoadedChunks.ContainsKey(chunkNormalPosition))
                return;

            // else create
            var chunkWorldPosition = new Vector3(
                chunkNormalPosition.x * WorldConfiguration.ChunkWorldSize,
                0,
                chunkNormalPosition.y * WorldConfiguration.ChunkWorldSize
            );

            var chunkGameObject = Instantiate(ChunkPrefab, chunkWorldPosition, Quaternion.Euler(Vector3.zero), transform) as GameObject;
            var chunk = chunkGameObject.GetComponent<Chunk>();

            //chunk.Initialize(this,, chunkNormalPosition);

            // try load chunk data
            // else generate
            GenerateChunk7(ref chunk, false);
            chunk.shouldUpdate = true;
            // Save it
            LoadedChunks.Add(chunk.NormalPosition, chunk);
        }

        //public void GenerateChunk2(ref Chunk chunk, bool debug = false)
        //{
        //    for (int x = 0; x < WorldConfiguration.ChunkSideLength; x++)
        //        for (int z = 0; z < WorldConfiguration.ChunkSideLength; z++)
        //        {
        //            float xWorldPos = chunk.transform.position.x + (x * WorldConfiguration.BlocScale);
        //            float zWorldPos = chunk.transform.position.z + (z * WorldConfiguration.BlocScale);

        //            float frequency = NoiseMethod.Frequency;
        //            float amplitude = NoiseMethod.Amplitude;

        //            var nHeight = Noise.Generate(xWorldPos, zWorldPos);
        //            int yMax = Mathf.RoundToInt(nHeight * WorldConfiguration.ChunkHeight);

        //            for (int y = 0; y < yMax; y++)
        //            {
        //                float tDensity = 0f;
        //                float maxValue = 0;

        //                for (int i = 0; i < NoiseMethod.Octaves; i++)
        //                {
        //                    tDensity += Noise.Generate(xWorldPos * frequency, (y * WorldConfiguration.BlocScale) * frequency, zWorldPos * frequency) * amplitude;
        //                    maxValue += amplitude;
        //                    amplitude *= NoiseMethod.Persistence;
        //                    frequency *= NoiseMethod.FrequencyMultiplier;
        //                }

        //                var density = (tDensity / maxValue);

        //                if (density > NoiseMethod.DensityThreshold)
        //                    chunk.SetBloc(x, y, z, BlocsDefinition[2].id);
        //                else
        //                    chunk.SetBloc(x, y, z, BlocsDefinition[0].id);
        //            }
        //        }
        //}

        //public void GenerateChunk(ref Chunk chunk, bool debug = false)
        //{
        //    for (int x = 0; x < WorldConfiguration.ChunkSideLength; x++)
        //        for (int z = 0; z < WorldConfiguration.ChunkSideLength; z++)
        //            for (int y = 0; y < WorldConfiguration.ChunkHeight; y++)
        //            {
        //                Vector3 blocWorldPosition = new Vector3(
        //                    chunk.transform.position.x + (x * WorldConfiguration.BlocScale),
        //                    (y * WorldConfiguration.BlocScale),
        //                    chunk.transform.position.z + (z * WorldConfiguration.BlocScale)
        //                );

        //                float tDensity = 0f;
        //                float frequency = NoiseMethod.Frequency;
        //                float amplitude = NoiseMethod.Amplitude;
        //                float maxValue = 0;

        //                for (int i = 0; i < NoiseMethod.Octaves; i++)
        //                {
        //                    tDensity += Noise.Generate(blocWorldPosition.x * frequency, blocWorldPosition.y * frequency, blocWorldPosition.z * frequency) * amplitude;
        //                    maxValue += amplitude;
        //                    amplitude *= NoiseMethod.Persistence;
        //                    frequency *= NoiseMethod.FrequencyMultiplier;
        //                }

        //                var density = (tDensity / maxValue);
        //                density += (y / WorldConfiguration.ChunkHeight);

        //                if (density >= NoiseMethod.DensityThreshold)
        //                    chunk.SetBloc(x, y, z, BlocsDefinition[2].id);
        //                else
        //                    chunk.SetBloc(x, y, z, BlocsDefinition[0].id);

        //                if (debug)
        //                {
        //                    DebugList.Add(new { x = density.ToString(), y = y.ToString() });
        //                }
        //            }


        //}

        public void GenerateChunk6(ref Chunk chunk, bool debug = false)
        {
            var perlinNoise = new Perlin();

            for (int x = 0; x < WorldConfiguration.ChunkSideLength; x++)
                for (int z = 0; z < WorldConfiguration.ChunkSideLength; z++)
                    for (int y = 0; y < WorldConfiguration.MaxWorldHeight; y++)
                    {
                        Vector3 blocWorldPosition = new Vector3(
                            chunk.transform.position.x + (x * WorldConfiguration.BlocScale),
                            (y * WorldConfiguration.BlocScale),
                            chunk.transform.position.z + (z * WorldConfiguration.BlocScale)
                        );

                        double noiseValue = perlinNoise.OctavePerlin(blocWorldPosition.x, blocWorldPosition.y, blocWorldPosition.z, 4, 1f);

                        if (noiseValue >= 0.5)
                            chunk.SetBloc(x, y, z, BlocsDefinition[2].id);
                        else
                            chunk.SetBloc(x, y, z, BlocsDefinition[0].id);
                    }
        }

        public void GenerateChunk7(ref Chunk chunk, bool debug = false)
        {
            var perlinNoise = new Perlin();

            int baseHeight = 18;

            float[,] maxHeights = new float[WorldConfiguration.ChunkSideLength, WorldConfiguration.ChunkSideLength];

            for (int x = 0; x < WorldConfiguration.ChunkSideLength; x++)
                for (int z = 0; z < WorldConfiguration.ChunkSideLength; z++)
                {
                    Vector2 blocWorldPosition = new Vector2(
                            chunk.transform.position.x + (x * WorldConfiguration.BlocScale),
                            chunk.transform.position.z + (z * WorldConfiguration.BlocScale)
                        );

                    maxHeights[x, z] = Get2DNoise(blocWorldPosition.x, blocWorldPosition.y, 4, 1.2f);
                }

            for (int x = 0; x < WorldConfiguration.ChunkSideLength; x++)
                for (int z = 0; z < WorldConfiguration.ChunkSideLength; z++)
                    for (int y = 0; y < WorldConfiguration.MaxWorldHeight; y++)
                    {
                        if (y < baseHeight)
                            chunk.SetBloc(x, y, z, BlocsDefinition[1].id);
                        else if (y <= Mathf.RoundToInt(maxHeights[x, z] * WorldConfiguration.MaxWorldHeight))
                            chunk.SetBloc(x, y, z, BlocsDefinition[2].id);
                        else
                            chunk.SetBloc(x, y, z, BlocsDefinition[0].id);
                    }

            for (int x = 0; x < WorldConfiguration.ChunkSideLength; x++)
                for (int z = 0; z < WorldConfiguration.ChunkSideLength; z++)
                    for (int y = 0; y < WorldConfiguration.MaxWorldHeight; y++)
                    {
                        Vector3 blocWorldPosition = new Vector3(
                            chunk.transform.position.x + (x * WorldConfiguration.BlocScale),
                            (y * WorldConfiguration.BlocScale),
                            chunk.transform.position.z + (z * WorldConfiguration.BlocScale)
                        );
                        var value = perlinNoise.OctavePerlin(blocWorldPosition.x, blocWorldPosition.y, blocWorldPosition.z, 3, .5f);
                        bool isAir = value <= 0.45f;

                        if (isAir)
                            chunk.SetBloc(x, y, z, BlocsDefinition[0].id);
                    }
        }


        float Get2DNoise(float x, float z, int octaves, float persistence)
        {
            float total = 0;
            float frequency = 0.008f;
            float amplitude = 1;
            float maxValue = 0;            // Used for normalizing result to 0.0 - 1.0
            for (int i = 0; i < octaves; i++)
            {
                total += Mathf.PerlinNoise(x * frequency, z * frequency) * amplitude;

                maxValue += amplitude;

                amplitude *= persistence;
                frequency *= 2;
            }

            return total / maxValue;
        }

        //public void GenerateChunk5(ref Chunk chunk, bool debug = false)
        //{
        //    float[,] heightMap = new float[WorldConfiguration.ChunkSideLength, WorldConfiguration.ChunkSideLength];

        //    float maxNoiseDensity = float.MinValue;
        //    float minNoiseDensity = float.MaxValue;

        //    for (int x = 0; x < WorldConfiguration.ChunkSideLength; x++)
        //        for (int z = 0; z < WorldConfiguration.ChunkSideLength; z++)
        //        {
        //            Vector3 blocWorldPosition = new Vector3(
        //                    chunk.transform.position.x + (x * WorldConfiguration.BlocScale),
        //                    0,
        //                    chunk.transform.position.z + (z * WorldConfiguration.BlocScale)
        //                );

        //            float amplitude = NoiseMethod.Amplitude;
        //            float frequency = NoiseMethod.Frequency;
        //            float noiseHeight = 0;

        //            for (int i = 0; i < NoiseMethod.Octaves; i++)
        //            {
        //                float sampleX = blocWorldPosition.x / NoiseMethod.Scale * frequency;
        //                float sampleZ = blocWorldPosition.z / NoiseMethod.Scale * frequency;

        //                float noiseValue = Mathf.PerlinNoise(sampleX, sampleZ);
        //                noiseHeight += noiseValue * amplitude;

        //                amplitude *= NoiseMethod.Persistence;
        //                frequency *= NoiseMethod.Lacunarity;
        //            }

        //            if (noiseHeight > maxNoiseDensity)
        //                maxNoiseDensity = noiseHeight;
        //            else if (noiseHeight < minNoiseDensity)
        //                minNoiseDensity = noiseHeight;

        //            int yMax = Mathf.FloorToInt(noiseHeight * WorldConfiguration.ChunkHeight);


        //            for (int y = 0; y < WorldConfiguration.ChunkHeight; y++)
        //            {
        //                amplitude = NoiseMethod.Amplitude;
        //                frequency = NoiseMethod.Frequency;
        //                float totalDensity = 0;
        //                if (y >= yMax)
        //                {
        //                    chunk.SetBloc(x, y, z, BlocsDefinition[0].id);
        //                }
        //                else
        //                {
        //                    for (int i = 0; i < 3; i++)
        //                    {
        //                        float sampleX = blocWorldPosition.x / NoiseMethod.Scale * frequency;
        //                        float sampleZ = blocWorldPosition.z / NoiseMethod.Scale * frequency;
        //                        float sampleY = y / NoiseMethod.Scale * frequency;

        //                        float densityZ = Mathf.PerlinNoise(sampleZ, sampleY) + (y / WorldConfiguration.ChunkHeight);
        //                        float densityX = Mathf.PerlinNoise(sampleY, sampleX) + (y / WorldConfiguration.ChunkHeight);
        //                        totalDensity += ((densityZ + densityX) * 0.4f) * amplitude;

        //                        amplitude *= NoiseMethod.Persistence;
        //                        frequency *= NoiseMethod.Lacunarity;
        //                    }

        //                    if (totalDensity >= NoiseMethod.DensityThreshold)
        //                        chunk.SetBloc(x, y, z, BlocsDefinition[0].id);
        //                    else
        //                        chunk.SetBloc(x, y, z, BlocsDefinition[2].id);
        //                }
        //            }
        //        }
        //}

        //public void GenerateChunk4(ref Chunk chunk, bool debug = false)
        //{
        //    float maxNoiseDensity = float.MinValue;
        //    float minNoiseDensity = float.MaxValue;

        //    for (int x = 0; x < WorldConfiguration.ChunkSideLength; x++)
        //        for (int z = 0; z < WorldConfiguration.ChunkSideLength; z++)
        //            for (int y = 0; y < WorldConfiguration.ChunkHeight; y++)
        //            {
        //                Vector3 blocWorldPosition = new Vector3(
        //                    chunk.transform.position.x + (x * WorldConfiguration.BlocScale),
        //                    (y * WorldConfiguration.BlocScale),
        //                    chunk.transform.position.z + (z * WorldConfiguration.BlocScale)
        //                );

        //                float amplitude = NoiseMethod.Amplitude;
        //                float frequency = NoiseMethod.Frequency;
        //                float noiseDensity = 0;

        //                for (int i = 0; i < NoiseMethod.Octaves; i++)
        //                {
        //                    float sampleX = blocWorldPosition.x / NoiseMethod.Scale * frequency;
        //                    float sampleY = blocWorldPosition.y / NoiseMethod.Scale * frequency;
        //                    float sampleZ = blocWorldPosition.z / NoiseMethod.Scale * frequency;

        //                    float noiseValue = Noise.Generate(sampleX, sampleY, sampleZ);
        //                    noiseDensity += noiseValue * amplitude;

        //                    amplitude *= NoiseMethod.Persistence;
        //                    frequency *= NoiseMethod.Lacunarity;
        //                }

        //                if (noiseDensity > maxNoiseDensity)
        //                    maxNoiseDensity = noiseDensity;
        //                else if (noiseDensity < minNoiseDensity)
        //                    minNoiseDensity = noiseDensity;

        //                noiseDensity -= ((float)y / (float)WorldConfiguration.ChunkHeight);

        //                if (noiseDensity <= NoiseMethod.DensityThreshold)
        //                    chunk.SetBloc(x, y, z, BlocsDefinition[0].id);
        //                else
        //                    chunk.SetBloc(x, y, z, BlocsDefinition[2].id);
        //            }
        //}

        //public void GenerateChunk3(ref Chunk chunk, bool debug = false)
        //{
        //    for (int x = 0; x < WorldConfiguration.ChunkSideLength; x++)
        //        for (int z = 0; z < WorldConfiguration.ChunkSideLength; z++)
        //        {
        //            Vector2 horizontalWorldPos = new Vector2(chunk.transform.position.x + (x * WorldConfiguration.BlocScale), chunk.transform.position.z + (z * WorldConfiguration.BlocScale));

        //            float totalDensity2D = 0;
        //            float scale = 0.025f;

        //            for (int j = 0; j < NoiseMethod.Octaves; j++)
        //            {
        //                totalDensity2D += Mathf.PerlinNoise(horizontalWorldPos.x * scale, horizontalWorldPos.y * scale);
        //            }

        //            float height = totalDensity2D / (float)NoiseMethod.Octaves;

        //            int yMax = Mathf.FloorToInt(height * WorldConfiguration.ChunkHeight);

        //            for (int y = 0; y < WorldConfiguration.ChunkHeight; y++)
        //            {
        //                float density = Noise.Generate(horizontalWorldPos.x * NoiseMethod.Frequency, (y * WorldConfiguration.BlocScale) * NoiseMethod.Frequency, horizontalWorldPos.y * NoiseMethod.Frequency);

        //                if (y > yMax || density <= NoiseMethod.DensityThreshold)
        //                    chunk.SetBloc(x, y, z, BlocsDefinition[0].id);
        //                else
        //                    chunk.SetBloc(x, y, z, BlocsDefinition[2].id);
        //            }
        //        }
        //}

        float falloff(float y)
        {
            float c = 3;
            return y / (Mathf.Pow(2, c) - 1) * (1 - y) + 1;
        }

        public void RenderChunk(Chunk chunk)
        {
            if (chunk == null)
                throw new ArgumentNullException(nameof(chunk));

            chunk.UpdateChunk();
        }

        public void SetBloc(Vector3 worldCoordinates, byte bloc)
        {
            try
            {
                var chunk = GetChunk(worldCoordinates);

                var blocPosition = new Vector3Int(
                    Mathf.FloorToInt((worldCoordinates.x - chunk.transform.position.x) / WorldConfiguration.BlocScale),
                    Mathf.FloorToInt(worldCoordinates.y / WorldConfiguration.BlocScale),
                    Mathf.FloorToInt((worldCoordinates.z - chunk.transform.position.z) / WorldConfiguration.BlocScale)
                );

                chunk.SetBloc(blocPosition.x, blocPosition.y, blocPosition.z, bloc);
            }
            catch (Exception) { }
        }

        public byte GetBloc(Vector3 worldCoordinates)
        {
            try
            {
                var chunk = GetChunk(worldCoordinates);

                if (chunk == null)
                    return 0;

                var blocPosition = new Vector3Int(
                    Mathf.FloorToInt((worldCoordinates.x - chunk.transform.position.x) / WorldConfiguration.BlocScale),
                    Mathf.FloorToInt((worldCoordinates.y - chunk.transform.position.y) / WorldConfiguration.BlocScale),
                    Mathf.FloorToInt((worldCoordinates.z - chunk.transform.position.z) / WorldConfiguration.BlocScale)
                );

                return chunk.GetBloc(blocPosition.x, blocPosition.y, blocPosition.z);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public Chunk GetChunk(Vector3 worldCoordinates)
        {
            var nPosition = new Vector3Int(
                Mathf.FloorToInt(worldCoordinates.x / WorldConfiguration.ChunkWorldSize),
                Mathf.FloorToInt(worldCoordinates.y / WorldConfiguration.ChunkWorldSize),
                Mathf.FloorToInt(worldCoordinates.z / WorldConfiguration.ChunkWorldSize)
            );

            var chunkObject = transform.Find(GetChunkNameFromPosition(nPosition));

            if (chunkObject != null)
                return chunkObject.GetComponent<Chunk>();

            return null;
        }
    }
}
