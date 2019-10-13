//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;

//public class World : MonoBehaviour
//{
//    public Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();
//    public GameObject ChunkPrefab;

//    public List<Bloc> BlocsDefinition = new List<Bloc>();

//    public Vector3 CenterPosition = new Vector3(512,0,512);
//    public Vector2Int VerticalRenderDistance = new Vector2Int(-1, 3);
//    public Vector2Int HorizontalRenderDistance = new Vector2Int(-4, 4);

//    public int ChunkDimensionInBloc = 16;
//    public float BlocSize = 0.5f;

//    public GameObject Player;

//    private void Start()
//    {
//        //for (int x = -4; x < 4; x++)
//        //    for (int y = -1; y < 3; y++)
//        //        for (int z = -4; z < 4; z++)
//        //        {
//        //            CreateChunk(x * (Chunk.ChunkSize * Bloc.BlocSize / 2f), y * (Chunk.ChunkSize * Bloc.BlocSize / 2f), z * (Chunk.ChunkSize * Bloc.BlocSize / 2f));
//        //        }

//        CreateWorldFromPosition();
//    }

//    public void CreateWorldFromPosition()
//    {
//        CenterPosition = new Vector3(Player.transform.position.x, 0, Player.transform.position.z);

//        Vector3Int CurrentChunkPosition = new Vector3Int(Mathf.FloorToInt(CenterPosition.x / (ChunkDimensionInBloc * BlocSize)), Mathf.FloorToInt(CenterPosition.y / (ChunkDimensionInBloc * BlocSize)), Mathf.FloorToInt(CenterPosition.z / (ChunkDimensionInBloc * BlocSize)));

//        for (int x = CurrentChunkPosition.x + HorizontalRenderDistance.x; x < CurrentChunkPosition.x + HorizontalRenderDistance.y; x++)
//            for (int y = CurrentChunkPosition.y + VerticalRenderDistance.x; y < CurrentChunkPosition.y + VerticalRenderDistance.y; y++)
//                for (int z = CurrentChunkPosition.z + HorizontalRenderDistance.x; z < CurrentChunkPosition.z + HorizontalRenderDistance.y; z++)
//                {
//                    CreateChunk(x, y, z, CurrentChunkPosition);
//                }

//        //var player = Instantiate(PlayerPrefab, new Vector3()
//        Player.GetComponent<Rigidbody>().isKinematic = false;
//    }

//    public void CreateChunk(int x, int y, int z, Vector3Int referenceChunkPosition)
//    {
//        float chunkWorldDimension = ChunkDimensionInBloc * BlocSize;

//        Vector3 worldPos = new Vector3(
//            x * chunkWorldDimension,
//            y * chunkWorldDimension,
//            z * chunkWorldDimension
//        );

//        Vector3Int worldPosNormalized = new Vector3Int(x, y, z);

//        var chunkObject = Instantiate(ChunkPrefab, Vector3.zero, Quaternion.Euler(Vector3.zero)) as GameObject;

//        Chunk newChunk = chunkObject.GetComponent<Chunk>();

//        newChunk.transform.parent = transform;

//        newChunk.pos = worldPos;
//        newChunk.normalizedPosition = worldPosNormalized;

//        newChunk.world = this;
//        newChunk.name = $"{worldPos.x}_{worldPos.y}_{worldPos.z}";
//        newChunk.sizeInBlocs = ChunkDimensionInBloc;
//        newChunk.blocSize = BlocSize;

//        chunks.Add(worldPosNormalized, newChunk);

//        //bool loaded = Serialization.Load(newChunk);

//        //if (loaded)
//        //    return;

//        var terrainGenerationMethod = new TerrainGeneration(BlocsDefinition, ChunkDimensionInBloc, BlocSize);
//        newChunk = terrainGenerationMethod.GenerateChunk3D(newChunk);

//    }



//    public Chunk GetChunk(float x, float y, float z)
//    {
//        Vector3Int pos = new Vector3Int(
//            (int)(x / (ChunkDimensionInBloc * BlocSize)),
//            (int)(y / (ChunkDimensionInBloc * BlocSize)),
//            (int)(z / (ChunkDimensionInBloc * BlocSize))
//        );

//        Chunk containerChunk = null;

//        chunks.TryGetValue(pos, out containerChunk);
//        return containerChunk;
//    }

//    public Chunk GetChunkFromNormalizedPosition(Vector3Int position)
//    {
//        Chunk containerChunk = null;
//        chunks.TryGetValue(position, out containerChunk);
//        return containerChunk;
//    }

//    public static float Truncate(float value, int digits)
//    {
//        double mult = Math.Pow(10.0, digits);
//        double result = Math.Truncate(mult * value) / mult;
//        return (float)result;
//    }

//    // TODO : ADAPT TO FLOAT
//    public void Destroy(int x, int y, int z)
//    {
//        Chunk chunk = null;
//        if (chunks.TryGetValue(new Vector3Int(x, y, z), out chunk))
//        {
//            Serialization.SaveChunk(chunk);
//            Destroy(chunk.gameObject);
//            chunks.Remove(new Vector3Int(x, y, z));
//        }
//    }

//    public void Destroy(Vector3Int position)
//    {
//        Chunk chunk = null;
//        if (chunks.TryGetValue(position, out chunk))
//        {
//            Serialization.SaveChunk(chunk);
//            Destroy(chunk.gameObject);
//            chunks.Remove(position);
//        }
//    }

//    public byte GetBloc(float x, float y, float z)
//    {
//        Chunk containerChunk = GetChunk(x, y, z);
//        if (containerChunk != null)
//        {
//            byte bloc = containerChunk.GetBlock(
//                Mathf.FloorToInt(Mathf.Abs(x - containerChunk.pos.x) / BlocSize),
//                Mathf.FloorToInt(Mathf.Abs(y - containerChunk.pos.y) / BlocSize),
//                Mathf.FloorToInt(Mathf.Abs(z - containerChunk.pos.z) / BlocSize)
//            );

//            return bloc;
//        }
//        else
//        {
//            return BlocsDefinition[0].id;
//        }
//    }

//    public void SetBloc(float x, float y, float z, byte bloc)
//    {
//        Chunk chunk = GetChunk(x, y, z);

//        if (chunk != null)
//        {
//            chunk.SetBloc(
//                Mathf.FloorToInt((x - chunk.pos.x) * ChunkDimensionInBloc),
//                Mathf.FloorToInt((y - chunk.pos.y) * ChunkDimensionInBloc),
//                Mathf.FloorToInt((y - chunk.pos.y) * ChunkDimensionInBloc),
//                bloc
//            );

//            chunk.update = true;

//            UpdateIfEqual(Mathf.FloorToInt((x - chunk.pos.x) * ChunkDimensionInBloc), 0, chunk.normalizedPosition + new Vector3Int(-1, 0, 0));
//            UpdateIfEqual(Mathf.FloorToInt((x - chunk.pos.x) * ChunkDimensionInBloc), ChunkDimensionInBloc - 1, chunk.normalizedPosition + new Vector3Int(1, 0, 0));
//            UpdateIfEqual(Mathf.FloorToInt((y - chunk.pos.y) * ChunkDimensionInBloc), 0, chunk.normalizedPosition + new Vector3Int(0, -1, 0));
//            UpdateIfEqual(Mathf.FloorToInt((y - chunk.pos.y) * ChunkDimensionInBloc), ChunkDimensionInBloc - 1, chunk.normalizedPosition + new Vector3Int(0, 1, 0));
//            UpdateIfEqual(Mathf.FloorToInt((y - chunk.pos.y) * ChunkDimensionInBloc), 0, chunk.normalizedPosition + new Vector3Int(0, 1, -1));
//            UpdateIfEqual(Mathf.FloorToInt((y - chunk.pos.y) * ChunkDimensionInBloc), ChunkDimensionInBloc - 1, chunk.normalizedPosition + new Vector3Int(0, 0, 1));
//        }
//    }

//    void UpdateIfEqual(float i, float j, Vector3Int pos)
//    {
//        if (i == j)
//        {
//            Chunk chunk = GetChunkFromNormalizedPosition(pos);
//            if (chunk != null)
//                chunk.update = true;
//        }
//    }
//}
