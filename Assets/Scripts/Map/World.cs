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
using Prism.NoiseMethods;
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
        public GameObject Player;
        public WorldConfiguration WorldConfiguration;
        public NoiseMethod NoiseMethod;
        public GameObject ChunkPrefab;


        #region ServiceAccessors
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
        #endregion

        #region Utilities
        public Vector3Int GetChunkPositionFromWorldPosition(Vector3 worldPosition)
        {
            return new Vector3Int(
                Mathf.FloorToInt(worldPosition.x / WorldConfiguration.ChunkWorldSize),
                Mathf.FloorToInt(worldPosition.y / WorldConfiguration.ChunkWorldSize),
                Mathf.FloorToInt(worldPosition.z / WorldConfiguration.ChunkWorldSize)
            );
        }

        public static string GetChunkNameFromPosition(Vector3Int position)
        {
            return $"{position.x}_{position.y}_{position.z}";
        }

        public static Vector3Int GetChunkPositionFromName(string name)
        {
            var parts = name.Split('_');
            return new Vector3Int(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
        }
        #endregion

        #region UnityLifecycle
        private void Start()
        {
            if (!BlocService.Loaded) return;

            foreach (var bloc in BlocService.Blocs)
            {
                Debug.Log($"Bloc : {bloc.Key} | isVisible = {bloc.Value.IsSolid}");
            }

            StartCoroutine(CreateWorldFromPosition(Player.transform.position));
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Recreating & Rendering map");
                StartCoroutine(CreateWorldFromPosition(Player.transform.position));
            }

            // TODO : Remake this function when serialization will be redone

            //if (Input.GetKeyDown(KeyCode.C))
            //{
            //    using (var sw = new StreamWriter("C:\\Users\\Kaddle\\Desktop\\prims-world-debug.json"))
            //    {
            //        string jsonData = JsonConvert.SerializeObject(DebugList, Formatting.Indented);

            //        sw.Write(jsonData);
            //        sw.Close();
            //    }
            //}
        }
        #endregion


        public IEnumerator CreateWorldFromPosition(Vector3 position)
        {
            var currentChunk = GetChunkPositionFromWorldPosition(position);

            List<Chunk> chunksToRender = new List<Chunk>();
            List<string> newChunks = new List<string>();

            // Load or generate visible chunks
            for (int x = currentChunk.x - WorldConfiguration.HorizontalRenderDistance; x <= currentChunk.x + WorldConfiguration.HorizontalRenderDistance; x++)
                for (int y = currentChunk.y - WorldConfiguration.VerticalRenderDistance; y <= currentChunk.y + WorldConfiguration.VerticalRenderDistance; y++)
                    for (int z = currentChunk.z - WorldConfiguration.HorizontalRenderDistance; z <= currentChunk.z + WorldConfiguration.HorizontalRenderDistance; z++)
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
                            yield return null;
                        }
                    }

            // Render new chunks
            foreach (var chunk in chunksToRender)
            {
                RenderChunk(chunk);
                yield return null;
            }


            // Destroy current rendered chunks that are not in the render distance range.
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                if (!newChunks.Contains(transform.GetChild(i).name))
                {
                    Destroy(transform.GetChild(i).gameObject);
                }
            }

        }

        #region ChunksManagement
        public Chunk CreateEmptyChunk(Vector3Int position)
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

            return chunk;
        }
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
            chunk.Blocs = NoiseMethod.FillChunk(chunkWorldPosition, WorldConfiguration);

            return chunk;
        }

        public void RenderChunk(Chunk chunk)
        {
            if (chunk == null)
                throw new ArgumentNullException(nameof(chunk));

            chunk.UpdateChunk();
        }
        #endregion

        #region WorldElementsAccessors
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

        #endregion
    }
}
