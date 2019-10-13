//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
//public class Chunk : MonoBehaviour
//{
//    public static int ChunkSize = 16;
//    public bool update = true;
//    public World world;
//    public Vector3 pos;
//    public Vector3Int normalizedPosition;
//    public int sizeInBlocs;
//    public float blocSize;
//    public bool UseRenderForCollision = true;

//    public byte[,,] blocs = new byte[ChunkSize, ChunkSize, ChunkSize];

//    MeshFilter _filter;
//    MeshCollider _collider;
//    // Start is called before the first frame update
//    void Start()
//    {
//        _filter = GetComponent<MeshFilter>();
//        _collider = GetComponent<MeshCollider>();
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        if(update)
//        {
//            update = false;
//            UpdateChunk();
//        }
//    }

//    public byte GetBlock(int x, int y, int z)
//    {
//        if (InRange(x) && InRange(y) && InRange(z))
//            return world.BlocsDefinition[blocs[x, y, z]].id;

//        return world.GetBloc(pos.x + x * blocSize, pos.y + y * blocSize, pos.z + z * blocSize);
//    }

//    public void SetBloc(int x, int y, int z, byte bloc)
//    {
//        if(InRange(x) && InRange(y) && InRange(z))
//        {
//            blocs[x, y, z] = bloc;
//        }
//        else
//        {
//            world.SetBloc(pos.x + x * blocSize, pos.y + y * blocSize, pos.z + z * blocSize, bloc);
//        }
//    }

//    public bool InRange(int index)
//    {
//        if (index < 0 || index >= sizeInBlocs)
//            return false;

//        return true;
//    }

//    void UpdateChunk()
//    {
//        MeshData data = new MeshData();

//        for (int x = 0; x < sizeInBlocs; x++)
//            for (int y = 0; y < sizeInBlocs; y++)
//                for (int z = 0; z < sizeInBlocs; z++)
//                {
//                    data = world.BlocsDefinition[blocs[x, y, z]].FillData(this, x, y, z, data, world.BlocsDefinition, pos, blocSize);
//                }

//        RenderMesh(data);
//    }

//    void RenderMesh(MeshData meshData)
//    {
//        _filter.mesh.Clear();
//        _filter.mesh.vertices = meshData.vertices.ToArray();
//        _filter.mesh.triangles = meshData.triangles.ToArray();

//        _filter.mesh.uv = meshData.uv.ToArray();
//        _filter.mesh.RecalculateNormals();

//        if (UseRenderForCollision)
//        {
//            Mesh mesh = new Mesh();
//            mesh.vertices = _filter.mesh.vertices;
//            mesh.triangles = _filter.mesh.triangles;
//            mesh.RecalculateNormals();

//            _collider.sharedMesh = mesh;
//        }
//    }
//}
