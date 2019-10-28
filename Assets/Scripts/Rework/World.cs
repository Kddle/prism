using Prism.Rework.Structs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Prism.Rework
{

    public class World : MonoBehaviour
    {
        public NoiseConfiguration SurfaceNoiseConfiguration;
        public NoiseConfiguration CaveNoiseConfiguration;

        public GameObject ChunkPrefab;

        public int ChunkSize;
        public int MaxHeight = 128;

        public int HRenderDistance;
        public int VRenderDistance;

        public bool UseGPU = true;
        public Vector3 StartPosition = Vector3.zero;

        float start_time;
        void Start()
        {
            start_time = Time.realtimeSinceStartup;
            StartCoroutine(CreateWorld(StartPosition));
        }

        void Update()
        {
            if(Input.GetKeyDown(KeyCode.G))
            {
                Debug.Log("Refreshing map");
                StartCoroutine(UpdateWorld(StartPosition));
            }
        }
        /// <summary>
        /// Create the world from scratch.
        /// </summary>
        /// <param name="position">Player's position</param>
        public IEnumerator CreateWorld(Vector3 position)
        {
            var currentChunk = GetChunkPositionFromWorldPosition(position);

            for (int x = currentChunk.x - HRenderDistance; x <= currentChunk.x + HRenderDistance; x++)
                for (int y = currentChunk.y - VRenderDistance; y <= currentChunk.y + VRenderDistance; y++)
                    for (int z = currentChunk.z - HRenderDistance; z <= currentChunk.z + HRenderDistance; z++)
                    {
                        var chunkWorldPosition = new Vector3(x * ChunkSize, y * ChunkSize, z * ChunkSize);
                        var chunkMatrixPosition = new Vector3Int(x, y, z);

                        var chunkObject = Instantiate(ChunkPrefab, chunkWorldPosition, Quaternion.Euler(Vector3.zero), transform) as GameObject;
                        var chunk = chunkObject.GetComponent<Chunk>();

                        chunk.Init(this, chunkWorldPosition, chunkMatrixPosition, $"{x}_{y}_{z}", ChunkSize, SurfaceNoiseConfiguration.ToNoiseConfig(), CaveNoiseConfiguration.ToNoiseConfig(), MaxHeight);

                        // try load chunk save file
                        // if not found, generate :
                        chunk.Generate(gpu: true);
                        yield return null;
                    }

            foreach (var chunk in transform.GetComponentsInChildren<Chunk>())
            {
                chunk.Render(gpu: UseGPU);
                yield return null;
            }

            float total_time = Time.realtimeSinceStartup - start_time;
            Debug.Log((UseGPU ? "[GPU]" : "[CPU]") + $" Done in : {total_time} seconds");
        }

        /// <summary>
        /// Update individually each chunk provided.
        /// </summary>
        /// <param name="chunks">Chunks to update</param>
        public IEnumerator UpdateChunks(Chunk[] chunks)
        {
            foreach (var chunk in chunks)
            {
                chunk.Render(UseGPU);
                yield return null;
            }
        }

        /// <summary>
        /// Update the entire world from the position provided.
        /// </summary>
        /// <param name="position">Player's position</param>
        public IEnumerator UpdateWorld(Vector3 position)
        {
            var currentChunk = GetChunkPositionFromWorldPosition(position);
            List<string> newChunks = new List<string>();

            for (int x = currentChunk.x - HRenderDistance; x <= currentChunk.x + HRenderDistance; x++)
                for (int y = currentChunk.y - VRenderDistance; y <= currentChunk.y + VRenderDistance; y++)
                    for (int z = currentChunk.z - HRenderDistance; z <= currentChunk.z + HRenderDistance; z++)
                    {
                        newChunks.Add($"{x}_{y}_{z}");
                    }

            List<string> renderedChunks = new List<string>();
            foreach(Transform child in transform)
            {
                renderedChunks.Add(child.name);
            }

            var chunksToRender = newChunks.Where(x => !renderedChunks.Contains(x)).ToArray();
            var chunksToDestroy = renderedChunks.Where(x => !newChunks.Contains(x)).ToArray();

            List<Chunk> generatedChunks = new List<Chunk>();

            // Generate
            foreach(var chunkName in chunksToRender)
            {
                var chunkPosition = chunkName.Split('_');
                int x = int.Parse(chunkPosition[0]);
                int y = int.Parse(chunkPosition[1]);
                int z = int.Parse(chunkPosition[2]);

                var chunkWorldPosition = new Vector3(x * ChunkSize, y * ChunkSize, z * ChunkSize);
                var chunkMatrixPosition = new Vector3Int(x, y, z);

                var chunkObject = Instantiate(ChunkPrefab, chunkWorldPosition, Quaternion.Euler(Vector3.zero), transform) as GameObject;
                var chunk = chunkObject.GetComponent<Chunk>();

                chunk.Init(this, chunkWorldPosition, chunkMatrixPosition, $"{x}_{y}_{z}", ChunkSize, SurfaceNoiseConfiguration.ToNoiseConfig(), CaveNoiseConfiguration.ToNoiseConfig(), MaxHeight);

                // try load chunk save file
                // if not found, generate :
                chunk.Generate(gpu: true);
                generatedChunks.Add(chunk);
                yield return null;
            }

            // Render
            foreach(var chunk in generatedChunks)
            {
                chunk.Render(UseGPU);
                yield return null;
            }

            // Destroy
            foreach(var chunk in chunksToDestroy)
            {
                Destroy(transform.Find(chunk).gameObject);
                yield return null;
            }
        }

        public Vector3Int GetChunkPositionFromWorldPosition(Vector3 worldPosition)
        {
            return new Vector3Int(
                Mathf.FloorToInt(worldPosition.x / ChunkSize),
                Mathf.FloorToInt(worldPosition.y / ChunkSize),
                Mathf.FloorToInt(worldPosition.z / ChunkSize)
            );
        }

        public uint GetBloc(Vector3 worldPosition)
        {
            var chunk = GetChunk(worldPosition);

            if (chunk == null)
                return 0;

            var blocPosition = new Vector3Int(
                    Mathf.FloorToInt(worldPosition.x - chunk.transform.position.x),
                    Mathf.FloorToInt(worldPosition.y - chunk.transform.position.y),
                    Mathf.FloorToInt(worldPosition.z - chunk.transform.position.z)
                );

            return chunk.GetBloc(blocPosition.x, blocPosition.y, blocPosition.z);
        }

        public Chunk GetChunk(Vector3 worldPosition)
        {
            var matrixPosition = new Vector3Int(
                Mathf.FloorToInt(worldPosition.x / ChunkSize),
                Mathf.FloorToInt(worldPosition.y / ChunkSize),
                Mathf.FloorToInt(worldPosition.z / ChunkSize)
            );

            var chunkObject = transform.Find($"{matrixPosition.x}_{matrixPosition.y}_{matrixPosition.z}");

            if (chunkObject != null)
            {
                return chunkObject.GetComponent<Chunk>();
            }

            return null;
        }

        public List<uint[]> GetNeighors(Chunk chunk)
        {
            var upObj = GetChunk(new Vector3(chunk.WorldPosition.x, chunk.WorldPosition.y + 48, chunk.WorldPosition.z));
            var downObj = GetChunk(new Vector3(chunk.WorldPosition.x, chunk.WorldPosition.y - 16, chunk.WorldPosition.z));
            var leftObj = GetChunk(new Vector3(chunk.WorldPosition.x - 16, chunk.WorldPosition.y, chunk.WorldPosition.z));
            var rightObj = GetChunk(new Vector3(chunk.WorldPosition.x + 48, chunk.WorldPosition.y, chunk.WorldPosition.z));
            var frontObj = GetChunk(new Vector3(chunk.WorldPosition.x, chunk.WorldPosition.y, chunk.WorldPosition.z + 48));
            var backObj = GetChunk(new Vector3(chunk.WorldPosition.x, chunk.WorldPosition.y, chunk.WorldPosition.z - 16));

            return new List<uint[]>()
            {
                upObj == null ? new uint[ChunkSize * ChunkSize * ChunkSize] : upObj.Blocs,
                downObj == null ? new uint[ChunkSize * ChunkSize * ChunkSize] : downObj.Blocs,
                leftObj == null ? new uint[ChunkSize * ChunkSize * ChunkSize] : leftObj.Blocs,
                rightObj == null ? new uint[ChunkSize * ChunkSize * ChunkSize] : rightObj.Blocs,
                frontObj == null ? new uint[ChunkSize * ChunkSize * ChunkSize] : frontObj.Blocs,
                backObj == null ? new uint[ChunkSize * ChunkSize * ChunkSize] : backObj.Blocs,
            };
        }

        
    }
}
