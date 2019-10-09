using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class World : MonoBehaviour
{
    public Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();
    public GameObject ChunkPrefab;

    public List<Bloc> BlocsDefinition = new List<Bloc>();

    private void Start()
    {
        for (int x = -4; x < 4; x++)
            for (int y = -1; y < 3; y++)
                for (int z = -4; z < 4; z++)
                {
                    CreateChunk(x * 16, y * 16, z * 16);
                }
    }
    public void CreateChunk(int x, int y, int z)
    {
        Vector3Int worldPos = new Vector3Int(x, y, z);

        var chunkObject = Instantiate(ChunkPrefab, new Vector3(x, y, z), Quaternion.Euler(Vector3.zero), transform) as GameObject;

        Chunk newChunk = chunkObject.GetComponent<Chunk>();

        newChunk.pos = worldPos;
        newChunk.world = this;
        newChunk.name = $"{worldPos.x}_{worldPos.y}_{worldPos.z}";

        chunks.Add(worldPos, newChunk);

        bool loaded = Serialization.Load(newChunk);

        if (loaded)
            return;

        var terrainGenerationMethod = new TerrainGeneration(BlocsDefinition);
        newChunk = terrainGenerationMethod.GenerateChunk3D(newChunk);

    }



    public Chunk GetChunk(int x, int y, int z)
    {
        Vector3Int pos = new Vector3Int();
        float multiple = Chunk.ChunkSize;
        pos.x = Mathf.FloorToInt(x / multiple) * Chunk.ChunkSize;
        pos.y = Mathf.FloorToInt(y / multiple) * Chunk.ChunkSize;
        pos.z = Mathf.FloorToInt(z / multiple) * Chunk.ChunkSize;

        Chunk containerChunk = null;
        chunks.TryGetValue(pos, out containerChunk);
        return containerChunk;
    }

    public void Destroy(int x, int y, int z)
    {
        Chunk chunk = null;
        if (chunks.TryGetValue(new Vector3Int(x, y, z), out chunk))
        {
            Serialization.SaveChunk(chunk);
            Destroy(chunk.gameObject);
            chunks.Remove(new Vector3Int(x, y, z));
        }
    }

    public void Destroy(Vector3Int position)
    {
        Chunk chunk = null;
        if (chunks.TryGetValue(position, out chunk))
        {
            Serialization.SaveChunk(chunk);
            Destroy(chunk.gameObject);
            chunks.Remove(position);
        }
    }

    public byte GetBloc(int x, int y, int z)
    {
        Chunk containerChunk = GetChunk(x, y, z);
        if (containerChunk != null)
        {
            byte bloc = containerChunk.GetBlock(
                x - containerChunk.pos.x,
                y - containerChunk.pos.y,
                z - containerChunk.pos.z
            );

            return bloc;
        }
        else
        {
            return BlocsDefinition[0].id;
        }
    }

    public void SetBloc(int x, int y, int z, byte bloc)
    {
        Chunk chunk = GetChunk(x, y, z);

        if (chunk != null)
        {
            chunk.SetBloc(x - chunk.pos.x, y - chunk.pos.y, z - chunk.pos.z, bloc);
            chunk.update = true;

            UpdateIfEqual(x - chunk.pos.x, 0, new Vector3Int(x - 1, y, z));
            UpdateIfEqual(x - chunk.pos.x, Chunk.ChunkSize - 1, new Vector3Int(x + 1, y, z));
            UpdateIfEqual(x - chunk.pos.y, 0, new Vector3Int(x, y - 1, z));
            UpdateIfEqual(x - chunk.pos.y, Chunk.ChunkSize - 1, new Vector3Int(x, y + 1, z));
            UpdateIfEqual(x - chunk.pos.z, 0, new Vector3Int(x, y, z - 1));
            UpdateIfEqual(x - chunk.pos.z, Chunk.ChunkSize - 1, new Vector3Int(x, y, z + 1));
        }
    }

    void UpdateIfEqual(int i, int j, Vector3Int pos)
    {
        if (i == j)
        {
            Chunk chunk = GetChunk(pos.x, pos.y, pos.z);
            if (chunk != null)
                chunk.update = true;
        }
    }
}
