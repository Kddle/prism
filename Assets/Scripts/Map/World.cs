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

        public WorldConfiguration WorldConfiguration;
        public NoiseConfiguration NoiseConfiguration;
        public List<Bloc> BlocsDefinition = new List<Bloc>();

        public GameObject ChunkPrefab;

        public List<object> DebugList = new List<object>();
        /// <summary>
        /// Get the chunk's normalized position from the player's world position.
        /// </summary>
        public Vector2Int GetPlayerCurrentChunk => new Vector2Int(
                Mathf.FloorToInt(Player.transform.position.x / WorldConfiguration.ChunkWorldSize),
                Mathf.FloorToInt(Player.transform.position.z / WorldConfiguration.ChunkWorldSize)
        );

        private Vector2Int LastPlayerChunk;

        private IEnumerator CreateWorldFunction;
        private void Start()
        {
            CreateWorldFromPlayerPosition();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                CreateWorldFromPlayerPosition();
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

            chunk.Initialize(this, WorldConfiguration, chunkNormalPosition);

            // try load chunk data
            // else generate
            GenerateChunk5(ref chunk, false);
            chunk.shouldUpdate = true;
            // Save it
            LoadedChunks.Add(chunk.NormalPosition, chunk);
        }

        public void GenerateChunk2(ref Chunk chunk, bool debug = false)
        {
            for (int x = 0; x < WorldConfiguration.ChunkSideLength; x++)
                for (int z = 0; z < WorldConfiguration.ChunkSideLength; z++)
                {
                    float xWorldPos = chunk.transform.position.x + (x * WorldConfiguration.BlocScale);
                    float zWorldPos = chunk.transform.position.z + (z * WorldConfiguration.BlocScale);

                    float frequency = NoiseConfiguration.Frequency;
                    float amplitude = NoiseConfiguration.Amplitude;

                    var nHeight = Noise.Generate(xWorldPos, zWorldPos);
                    int yMax = Mathf.RoundToInt(nHeight * WorldConfiguration.ChunkHeight);

                    for (int y = 0; y < yMax; y++)
                    {
                        float tDensity = 0f;
                        float maxValue = 0;

                        for (int i = 0; i < NoiseConfiguration.Octaves; i++)
                        {
                            tDensity += Noise.Generate(xWorldPos * frequency, (y * WorldConfiguration.BlocScale) * frequency, zWorldPos * frequency) * amplitude;
                            maxValue += amplitude;
                            amplitude *= NoiseConfiguration.Persistence;
                            frequency *= NoiseConfiguration.FrequencyMultiplier;
                        }

                        var density = (tDensity / maxValue);

                        if (density > NoiseConfiguration.DensityThreshold)
                            chunk.SetBloc(x, y, z, BlocsDefinition[2].id);
                        else
                            chunk.SetBloc(x, y, z, BlocsDefinition[0].id);
                    }
                }
        }

        public void GenerateChunk(ref Chunk chunk, bool debug = false)
        {
            for (int x = 0; x < WorldConfiguration.ChunkSideLength; x++)
                for (int z = 0; z < WorldConfiguration.ChunkSideLength; z++)
                    for (int y = 0; y < WorldConfiguration.ChunkHeight; y++)
                    {
                        Vector3 blocWorldPosition = new Vector3(
                            chunk.transform.position.x + (x * WorldConfiguration.BlocScale),
                            (y * WorldConfiguration.BlocScale),
                            chunk.transform.position.z + (z * WorldConfiguration.BlocScale)
                        );

                        float tDensity = 0f;
                        float frequency = NoiseConfiguration.Frequency;
                        float amplitude = NoiseConfiguration.Amplitude;
                        float maxValue = 0;

                        for (int i = 0; i < NoiseConfiguration.Octaves; i++)
                        {
                            tDensity += Noise.Generate(blocWorldPosition.x * frequency, blocWorldPosition.y * frequency, blocWorldPosition.z * frequency) * amplitude;
                            maxValue += amplitude;
                            amplitude *= NoiseConfiguration.Persistence;
                            frequency *= NoiseConfiguration.FrequencyMultiplier;
                        }

                        var density = (tDensity / maxValue);
                        density += (y / WorldConfiguration.ChunkHeight);

                        if (density >= NoiseConfiguration.DensityThreshold)
                            chunk.SetBloc(x, y, z, BlocsDefinition[2].id);
                        else
                            chunk.SetBloc(x, y, z, BlocsDefinition[0].id);

                        if (debug)
                        {
                            DebugList.Add(new { x = density.ToString(), y = y.ToString() });
                        }
                    }


        }

        public void GenerateChunk5(ref Chunk chunk, bool debug = false)
        {
            float[,] heightMap = new float[WorldConfiguration.ChunkSideLength, WorldConfiguration.ChunkSideLength];

            float maxNoiseDensity = float.MinValue;
            float minNoiseDensity = float.MaxValue;

            for (int x = 0; x < WorldConfiguration.ChunkSideLength; x++)
                for (int z = 0; z < WorldConfiguration.ChunkSideLength; z++)
                {
                    Vector3 blocWorldPosition = new Vector3(
                            chunk.transform.position.x + (x * WorldConfiguration.BlocScale),
                            0,
                            chunk.transform.position.z + (z * WorldConfiguration.BlocScale)
                        );

                    float amplitude = NoiseConfiguration.Amplitude;
                    float frequency = NoiseConfiguration.Frequency;
                    float noiseHeight = 0;

                    for (int i = 0; i < NoiseConfiguration.Octaves; i++)
                    {
                        float sampleX = blocWorldPosition.x / NoiseConfiguration.Scale * frequency;
                        float sampleZ = blocWorldPosition.z / NoiseConfiguration.Scale * frequency;

                        float noiseValue = Mathf.PerlinNoise(sampleX, sampleZ);
                        noiseHeight += noiseValue * amplitude;

                        amplitude *= NoiseConfiguration.Persistence;
                        frequency *= NoiseConfiguration.Lacunarity;
                    }

                    if (noiseHeight > maxNoiseDensity)
                        maxNoiseDensity = noiseHeight;
                    else if (noiseHeight < minNoiseDensity)
                        minNoiseDensity = noiseHeight;

                    int yMax = Mathf.FloorToInt(noiseHeight * WorldConfiguration.ChunkHeight);


                    for (int y = 0; y < WorldConfiguration.ChunkHeight; y++)
                    {
                        amplitude = NoiseConfiguration.Amplitude;
                        frequency = NoiseConfiguration.Frequency;
                        float totalDensity = 0;
                        if (y >= yMax)
                        {
                            chunk.SetBloc(x, y, z, BlocsDefinition[0].id);
                        }
                        else
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                float sampleX = blocWorldPosition.x / NoiseConfiguration.Scale * frequency;
                                float sampleZ = blocWorldPosition.z / NoiseConfiguration.Scale * frequency;
                                float sampleY = y / NoiseConfiguration.Scale * frequency;

                                float densityZ = Mathf.PerlinNoise(sampleZ, sampleY) + (y / WorldConfiguration.ChunkHeight);
                                float densityX = Mathf.PerlinNoise(sampleY, sampleX) + (y / WorldConfiguration.ChunkHeight);
                                totalDensity += ((densityZ + densityX) * 0.4f) * amplitude;

                                amplitude *= NoiseConfiguration.Persistence;
                                frequency *= NoiseConfiguration.Lacunarity;
                            }

                            if (totalDensity >= NoiseConfiguration.DensityThreshold)
                                chunk.SetBloc(x, y, z, BlocsDefinition[0].id);
                            else
                                chunk.SetBloc(x, y, z, BlocsDefinition[2].id);
                        }
                    }
                }
        }

        public void GenerateChunk4(ref Chunk chunk, bool debug = false)
        {
            float maxNoiseDensity = float.MinValue;
            float minNoiseDensity = float.MaxValue;

            for (int x = 0; x < WorldConfiguration.ChunkSideLength; x++)
                for (int z = 0; z < WorldConfiguration.ChunkSideLength; z++)
                    for (int y = 0; y < WorldConfiguration.ChunkHeight; y++)
                    {
                        Vector3 blocWorldPosition = new Vector3(
                            chunk.transform.position.x + (x * WorldConfiguration.BlocScale),
                            (y * WorldConfiguration.BlocScale),
                            chunk.transform.position.z + (z * WorldConfiguration.BlocScale)
                        );

                        float amplitude = NoiseConfiguration.Amplitude;
                        float frequency = NoiseConfiguration.Frequency;
                        float noiseDensity = 0;

                        for (int i = 0; i < NoiseConfiguration.Octaves; i++)
                        {
                            float sampleX = blocWorldPosition.x / NoiseConfiguration.Scale * frequency;
                            float sampleY = blocWorldPosition.y / NoiseConfiguration.Scale * frequency;
                            float sampleZ = blocWorldPosition.z / NoiseConfiguration.Scale * frequency;

                            float noiseValue = Noise.Generate(sampleX, sampleY, sampleZ);
                            noiseDensity += noiseValue * amplitude;

                            amplitude *= NoiseConfiguration.Persistence;
                            frequency *= NoiseConfiguration.Lacunarity;
                        }

                        if (noiseDensity > maxNoiseDensity)
                            maxNoiseDensity = noiseDensity;
                        else if (noiseDensity < minNoiseDensity)
                            minNoiseDensity = noiseDensity;

                        noiseDensity -= ((float)y / (float)WorldConfiguration.ChunkHeight);

                        if (noiseDensity <= NoiseConfiguration.DensityThreshold)
                            chunk.SetBloc(x, y, z, BlocsDefinition[0].id);
                        else
                            chunk.SetBloc(x, y, z, BlocsDefinition[2].id);
                    }
        }

        public void GenerateChunk3(ref Chunk chunk, bool debug = false)
        {
            for (int x = 0; x < WorldConfiguration.ChunkSideLength; x++)
                for (int z = 0; z < WorldConfiguration.ChunkSideLength; z++)
                {
                    Vector2 horizontalWorldPos = new Vector2(chunk.transform.position.x + (x * WorldConfiguration.BlocScale), chunk.transform.position.z + (z * WorldConfiguration.BlocScale));

                    float totalDensity2D = 0;
                    float scale = 0.025f;

                    for (int j = 0; j < NoiseConfiguration.Octaves; j++)
                    {
                        totalDensity2D += Mathf.PerlinNoise(horizontalWorldPos.x * scale, horizontalWorldPos.y * scale);
                    }

                    float height = totalDensity2D / (float)NoiseConfiguration.Octaves;

                    int yMax = Mathf.FloorToInt(height * WorldConfiguration.ChunkHeight);

                    for (int y = 0; y < WorldConfiguration.ChunkHeight; y++)
                    {
                        float density = Noise.Generate(horizontalWorldPos.x * NoiseConfiguration.Frequency, (y * WorldConfiguration.BlocScale) * NoiseConfiguration.Frequency, horizontalWorldPos.y * NoiseConfiguration.Frequency);

                        if (y > yMax || density <= NoiseConfiguration.DensityThreshold)
                            chunk.SetBloc(x, y, z, BlocsDefinition[0].id);
                        else
                            chunk.SetBloc(x, y, z, BlocsDefinition[2].id);
                    }
                }
        }

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
                    Mathf.FloorToInt(worldCoordinates.y / WorldConfiguration.BlocScale),
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
            if (worldCoordinates.y < 0 || worldCoordinates.y >= WorldConfiguration.ChunkHeight * WorldConfiguration.BlocScale)
                return null;

            var chunkId = new Vector2Int(
                Mathf.FloorToInt(worldCoordinates.x / WorldConfiguration.ChunkWorldSize),
                Mathf.FloorToInt(worldCoordinates.z / WorldConfiguration.ChunkWorldSize)
            );

            LoadedChunks.TryGetValue(chunkId, out var chunk);
            return chunk;
        }
    }
}
