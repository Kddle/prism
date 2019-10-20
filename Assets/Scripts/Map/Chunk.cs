using Prism.Map.Configuration;
using Prism.NoiseMethods;
using Prism.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Prism.Map
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class Chunk : MonoBehaviour
    {
        MeshFilter _filter;
        MeshCollider _collider;
        public bool UseRenderForCollision = true;

        public World World => transform.parent.GetComponent<World>();

        public byte[,,] Blocs;

        public bool shouldUpdate = false;

        private void FixedUpdate()
        {
            if (shouldUpdate)
            {
                shouldUpdate = false;
                UpdateChunk(World.BlocService.GetBlocsDefinitionArray(), World.MeshChunkComputeShader);
            }
        }

        public void Initialize(Vector3Int worldPosition, int chunkSize)
        {
            Blocs = new byte[chunkSize, chunkSize, chunkSize];

            name = $"{worldPosition.x}_{worldPosition.y}_{worldPosition.z}";

            _filter = GetComponent<MeshFilter>();
            _collider = GetComponent<MeshCollider>();
        }

        public void SetBloc(int x, int y, int z, byte bloc)
        {
            if (InRange(x) && InRange(y) && InRange(z))
                Blocs[x, y, z] = bloc;
            else
                World.SetBloc(
                    new Vector3(
                        transform.position.x + (x * World.WorldConfiguration.BlocScale),
                        transform.position.y + (y * World.WorldConfiguration.BlocScale),
                        transform.position.z + (z * World.WorldConfiguration.BlocScale)
                    ),
                    bloc);
        }

        public byte GetBloc(int x, int y, int z)
        {
            if (InRange(x) && InRange(y) && InRange(z))
                return Blocs[x, y, z];
            else
                return World.GetBloc(
                    new Vector3(
                        transform.position.x + (x * World.WorldConfiguration.BlocScale),
                        transform.position.y + (y * World.WorldConfiguration.BlocScale),
                        transform.position.z + (z * World.WorldConfiguration.BlocScale)
                    ));
        }

        public bool InRange(int index)
        {
            if (index < 0 || index >= World.WorldConfiguration.ChunkSideLength)
                return false;

            return true;
        }

        public void UpdateChunk(BlocData[] blocDefinitions, ComputeShader meshComputeShader)
        {
            /*
            // --- ComputeShader Test failed
            // Get chunk bloc + neighbors
            var blocsWithNeighbors = GetBlocsDataForComputeShader();
            var blocsDefinitionsData = blocDefinitions;
            int[] meshInfos = new int[2]; // 0 = vertices count; 1 = triangles count

            // Create a buffer of the chunk's blocs
            ComputeBuffer blocsBuffer = new ComputeBuffer(blocsWithNeighbors.Length, sizeof(int));
            blocsBuffer.SetData(blocsWithNeighbors);

            // Create a buffer for the mesh data that will be calculated by the GPU
            ComputeBuffer meshInfosBuffer = new ComputeBuffer(2, sizeof(int));
            meshInfosBuffer.SetData(meshInfos);

            // Prepare Mesh Compute Shader execution
            int prepareKernel = meshComputeShader.FindKernel("CalculateVisibleFaces");

            meshComputeShader.SetInt("chunkLength", World.WorldConfiguration.ChunkSideLength);
            meshComputeShader.SetBuffer(prepareKernel, "blocsBuffer", blocsBuffer);
            meshComputeShader.SetBuffer(prepareKernel, "meshInfos", meshInfosBuffer);

            meshComputeShader.Dispatch(prepareKernel, 1,1,1);

            meshInfosBuffer.GetData(meshInfos);
            

            // Create the variables that will store the mesh data with values calculated by the GPU
            Vector3[] vertices = new Vector3[meshInfos[0]];
            int[] triangles = new int[meshInfos[1]];

            // Prepare data to be sent to the GPU to calculate the mesh elements

            int blocDataSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(BlocData));
            ComputeBuffer blocsDefinitionsBuffer = new ComputeBuffer(blocsDefinitionsData.Length, blocDataSize);
            blocsDefinitionsBuffer.SetData(blocsDefinitionsData);

            meshComputeShader.SetInt("blocDefinitionsLength", blocsDefinitionsData.Length);

            ComputeBuffer verticesBuffer = new ComputeBuffer(vertices.Length, sizeof(float) * 3);
            verticesBuffer.SetData(vertices);

            

            int kernel = meshComputeShader.FindKernel("ComputeMesh");
            meshComputeShader.SetBuffer(kernel, "blocsBuffer", blocsBuffer);
            meshComputeShader.SetBuffer(kernel, "blocsDefinitions", blocsDefinitionsBuffer);
            meshComputeShader.SetBuffer(kernel, "_vertices", verticesBuffer);

            meshComputeShader.Dispatch(kernel, 1,1,1);
            verticesBuffer.GetData(vertices);
            verticesBuffer.Dispose();
            blocsBuffer.Dispose();
            blocsDefinitionsBuffer.Dispose();

            // Triangles
            ComputeBuffer trianglesBuffer = new ComputeBuffer(triangles.Length, sizeof(int));
            trianglesBuffer.SetData(triangles);

            int trianglesKernel = meshComputeShader.FindKernel("ComputeTriangles");
            meshComputeShader.SetBuffer(trianglesKernel, "_triangles", trianglesBuffer);
            meshComputeShader.SetBuffer(trianglesKernel, "meshInfos", meshInfosBuffer);

            meshComputeShader.Dispatch(trianglesKernel, 1,1,1);
            trianglesBuffer.GetData(triangles);

            trianglesBuffer.Dispose();
            meshInfosBuffer.Dispose();

            //for (int i = 0; i < 1000; i++)
            //{
            //    Debug.Log(triangles[i]);
            //}

            if (_filter.mesh != null)
                _filter.mesh.Clear();
            else
                _filter.mesh = new Mesh();

            _filter.mesh.vertices = vertices;
            _filter.mesh.triangles = triangles;

            //_filter.mesh.uv = vertices..uv.ToArray();
            _filter.mesh.RecalculateNormals();

            //Debug.Log($"Chunk[{gameObject.name}] | Vertices [{_filter.mesh.vertices.Length}] | Triangles [{_filter.mesh.triangles.Length}]");

            if (UseRenderForCollision)
            {
                Mesh mesh = new Mesh();
                mesh.vertices = vertices;
                mesh.triangles = triangles;
                mesh.RecalculateNormals();

                if (_collider.sharedMesh != null)
                    _collider.sharedMesh.Clear();
                else
                    _collider.sharedMesh = new Mesh();

                _collider.sharedMesh.vertices = vertices;
                _collider.sharedMesh.triangles = triangles;
                _collider.sharedMesh.RecalculateNormals();
            }
            */

            MeshData meshData = new MeshData();

            for (int x = 0; x < World.WorldConfiguration.ChunkSideLength; x++)
                for (int y = 0; y < World.WorldConfiguration.ChunkSideLength; y++)
                    for (int z = 0; z < World.WorldConfiguration.ChunkSideLength; z++)
                    {
                        meshData = World.BlocService.GetMeshDataForBloc(meshData, (BlocType)Blocs[x, y, z], this, new Vector3Int(x, y, z), World.WorldConfiguration.BlocScale);
                    }

            RenderChunk(meshData);
        }

        public byte[] GetBlocsDataForComputeShader()
        {
            int length = World.WorldConfiguration.ChunkSideLength + 2;

            byte[] BlocsWithNeighbors = new byte[length * length * length];

            // Y Axis Neighbors
            for (int i = 0; i < World.WorldConfiguration.ChunkSideLength; i++)
                for (int j = 0; j < World.WorldConfiguration.ChunkSideLength; j++)
                {
                    // Up Chunk Face
                    BlocsWithNeighbors[(i + 1) + World.WorldConfiguration.ChunkSideLength * length + (j + 1) * length * length] = GetBloc(i, World.WorldConfiguration.ChunkSideLength, j);
                    // Down Chunk Face
                    BlocsWithNeighbors[(i + 1) + 0 * length + (j + 1) * length * length] = GetBloc(i, -1, j);

                    // Right Chunk Face
                    BlocsWithNeighbors[World.WorldConfiguration.ChunkSideLength + (i + 1) * length + (j + 1) * length * length] = GetBloc(World.WorldConfiguration.ChunkSideLength, i, j);
                    // Left Chunk Face
                    BlocsWithNeighbors[0 + (i + 1) * length + (j + 1) * length * length] = GetBloc(-1, i, j);

                    // Front Chunk Face
                    BlocsWithNeighbors[(i + 1) + (j + 1) * length + World.WorldConfiguration.ChunkSideLength * length * length] = GetBloc(i, j, World.WorldConfiguration.ChunkSideLength);
                    // Back Chunk Face
                    BlocsWithNeighbors[(i + 1) + (j + 1) * length + 0 * length * length] = GetBloc(i, j, -1);
                }

            for (int x = 0; x < World.WorldConfiguration.ChunkSideLength; x++)
                for (int y = 0; y < World.WorldConfiguration.ChunkSideLength; y++)
                    for (int z = 0; z < World.WorldConfiguration.ChunkSideLength; z++)
                    {
                        BlocsWithNeighbors[(x + 1) + (y + 1) * length + (z + 1) * length * length] = Blocs[x, y, z];
                    }

            return BlocsWithNeighbors;
        }

        void RenderChunk(MeshData meshData)
        {
            if (_filter.mesh != null)
                _filter.mesh.Clear();
            else
                _filter.mesh = new Mesh();

            _filter.mesh.vertices = meshData.vertices.ToArray();
            _filter.mesh.triangles = meshData.triangles.ToArray();

            _filter.mesh.uv = meshData.uv.ToArray();
            _filter.mesh.RecalculateNormals();

            Debug.Log($"Chunk[{gameObject.name}] | Vertices [{_filter.mesh.vertices.Length}] | Triangles [{_filter.mesh.triangles.Length}]");

            if (UseRenderForCollision)
            {
                Mesh mesh = new Mesh();
                mesh.vertices = _filter.mesh.vertices;
                mesh.triangles = _filter.mesh.triangles;
                mesh.RecalculateNormals();

                _collider.sharedMesh = mesh;
            }
        }
    }
}