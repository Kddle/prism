using Prism.Rework.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Prism.Rework
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class Chunk : MonoBehaviour
    {
        public ComputeShader DataComputeShader;
        public ComputeShader ComputeFaceComputeShader;
        public ComputeShader FillMeshComputeShader;

        public NoiseConfig SurfaceConfig;
        public NoiseConfig CaveConfig;

        public Vector3Int MatrixPosition;
        public Vector3 WorldPosition;
        public int ChunkSize;
        public float AirThreshold = 0;
        public float MaxHeight;

        public uint[] Blocs;

        MeshFilter _filter;
        MeshRenderer _renderer;
        MeshCollider _collider;
        World _world;

        public void Init(World world, Vector3 worldPosition, Vector3Int matrixPosition, string name, int chunkSize, NoiseConfig surfaceConfig, NoiseConfig caveConfig, int maxHeight)
        {
            transform.name = name;
            WorldPosition = worldPosition;
            MatrixPosition = matrixPosition;
            ChunkSize = chunkSize;
            SurfaceConfig = surfaceConfig;
            CaveConfig = caveConfig;
            MaxHeight = maxHeight;
            _world = world;

            Blocs = new uint[chunkSize * chunkSize * chunkSize];

            _filter = GetComponent<MeshFilter>();
            _renderer = GetComponent<MeshRenderer>();
            _collider = GetComponent<MeshCollider>();
        }

        public void Generate(bool gpu = true)
        {
            if (gpu)
            {
                uint[] data = new uint[ChunkSize * ChunkSize * ChunkSize];
                NoiseConfig[] noises = new NoiseConfig[] { SurfaceConfig, CaveConfig };

                ComputeBuffer dataBuffer = new ComputeBuffer(ChunkSize * ChunkSize * ChunkSize, sizeof(uint));
                dataBuffer.SetData(data);

                ComputeBuffer noisesBuffer = new ComputeBuffer(2, Marshal.SizeOf(typeof(NoiseConfig)));
                noisesBuffer.SetData(noises);

                ComputeBuffer chunkPositionBuffer = new ComputeBuffer(3, sizeof(float));
                chunkPositionBuffer.SetData(new float[] { WorldPosition.x, WorldPosition.y, WorldPosition.z });

                int kernel = DataComputeShader.FindKernel("CSMain");
                DataComputeShader.SetFloat("maxHeight", MaxHeight);
                DataComputeShader.SetFloat("airThreshold", AirThreshold);
                DataComputeShader.SetBuffer(kernel, "data", dataBuffer);
                DataComputeShader.SetBuffer(kernel, "noises", noisesBuffer);
                DataComputeShader.SetBuffer(kernel, "chunkWorldPosition", chunkPositionBuffer);

                DataComputeShader.Dispatch(kernel, 4, 4, 4);

                dataBuffer.GetData(Blocs);

                // Cleanup
                dataBuffer.Release();
                noisesBuffer.Release();
                chunkPositionBuffer.Release();
                //
            }
            else
            {
                // cpu generation
            }
        }

        public void Render(bool gpu = true)
        {
            var chunkFaces = GetVisibleFaces(gpu); // gpu not working well
            RenderFaces(chunkFaces, gpu);
        }

        public ChunkFace[] GetVisibleFaces(bool gpu = true)
        {
            if (gpu)
            {
                var neighbors = _world.GetNeighors(this);

                ComputeBuffer chunkBlocsBuffer = new ComputeBuffer(ChunkSize * ChunkSize * ChunkSize, sizeof(uint));
                chunkBlocsBuffer.SetData(Blocs);

                ComputeBuffer upNeighborBuffer = new ComputeBuffer(ChunkSize * ChunkSize * ChunkSize, sizeof(uint));
                upNeighborBuffer.SetData(neighbors[0]);

                ComputeBuffer downNeighborBuffer = new ComputeBuffer(ChunkSize * ChunkSize * ChunkSize, sizeof(uint));
                downNeighborBuffer.SetData(neighbors[1]);

                ComputeBuffer leftNeighborBuffer = new ComputeBuffer(ChunkSize * ChunkSize * ChunkSize, sizeof(uint));
                leftNeighborBuffer.SetData(neighbors[2]);

                ComputeBuffer rightNeighborBuffer = new ComputeBuffer(ChunkSize * ChunkSize * ChunkSize, sizeof(uint));
                rightNeighborBuffer.SetData(neighbors[3]);

                ComputeBuffer frontNeighborBuffer = new ComputeBuffer(ChunkSize * ChunkSize * ChunkSize, sizeof(uint));
                frontNeighborBuffer.SetData(neighbors[4]);

                ComputeBuffer backNeighborBuffer = new ComputeBuffer(ChunkSize * ChunkSize * ChunkSize, sizeof(uint));
                backNeighborBuffer.SetData(neighbors[5]);

                int maxFaceLength = ((ChunkSize * ChunkSize * ChunkSize) * 6 * 4);

                ComputeBuffer facesBuffer = new ComputeBuffer(maxFaceLength, Marshal.SizeOf<ChunkFace>(), ComputeBufferType.Append);
                facesBuffer.SetData(new ChunkFace[maxFaceLength]);
                facesBuffer.SetCounterValue(0);

                int kernel = ComputeFaceComputeShader.FindKernel("CSMain");

                ComputeFaceComputeShader.SetBuffer(kernel, "blocs", chunkBlocsBuffer);
                ComputeFaceComputeShader.SetBuffer(kernel, "upNeighbor", upNeighborBuffer);
                ComputeFaceComputeShader.SetBuffer(kernel, "downNeighbor", downNeighborBuffer);
                ComputeFaceComputeShader.SetBuffer(kernel, "leftNeighbor", leftNeighborBuffer);
                ComputeFaceComputeShader.SetBuffer(kernel, "rightNeighbor", rightNeighborBuffer);
                ComputeFaceComputeShader.SetBuffer(kernel, "frontNeighbor", frontNeighborBuffer);
                ComputeFaceComputeShader.SetBuffer(kernel, "backNeighbor", backNeighborBuffer);
                ComputeFaceComputeShader.SetBuffer(kernel, "faces", facesBuffer);

                ComputeBuffer counterBuffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
                counterBuffer.SetData(new int[4]);

                ComputeFaceComputeShader.Dispatch(kernel, 4, 4, 4);

                ComputeBuffer.CopyCount(facesBuffer, counterBuffer, 0);

                int[] counter = new int[4];
                counterBuffer.GetData(counter);

                int facesLength = counter[0];

                ChunkFace[] faces = new ChunkFace[facesLength];
                facesBuffer.GetData(faces);

                // Cleanup
                chunkBlocsBuffer.Release();
                upNeighborBuffer.Release();
                downNeighborBuffer.Release();
                leftNeighborBuffer.Release();
                rightNeighborBuffer.Release();
                frontNeighborBuffer.Release();
                backNeighborBuffer.Release();
                facesBuffer.Release();
                counterBuffer.Release();
                //
                return faces;
            }
            else
            {
                List<ChunkFace> VisibleFaces = new List<ChunkFace>();
                for (int i = 0; i < ChunkSize * ChunkSize * ChunkSize; i++)
                {
                    int x = i % ChunkSize;
                    int y = (i / ChunkSize) % ChunkSize;
                    int z = i / (ChunkSize * ChunkSize);

                    if (GetBloc(x, y, z) != 0)
                    {
                        if (GetBloc(x, y + 1, z) == 0)
                            VisibleFaces.Add(new ChunkFace() { center = new Vector3(x, y, z), direction = 0 });

                        if (GetBloc(x, y - 1, z) == 0)
                            VisibleFaces.Add(new ChunkFace() { center = new Vector3(x, y, z), direction = 1 });

                        if (GetBloc(x - 1, y, z) == 0)
                            VisibleFaces.Add(new ChunkFace() { center = new Vector3(x, y, z), direction = 2 });

                        if (GetBloc(x + 1, y, z) == 0)
                            VisibleFaces.Add(new ChunkFace() { center = new Vector3(x, y, z), direction = 3 });

                        if (GetBloc(x, y, z + 1) == 0)
                            VisibleFaces.Add(new ChunkFace() { center = new Vector3(x, y, z), direction = 4 });

                        if (GetBloc(x, y, z - 1) == 0)
                            VisibleFaces.Add(new ChunkFace() { center = new Vector3(x, y, z), direction = 5 });
                    }
                }

                return VisibleFaces.ToArray();
            }
        }

        public void RenderFaces(ChunkFace[] faces, bool gpu = true)
        {
            Vector3[] vertices = new Vector3[faces.Length * 4];
            Vector2[] uvs = new Vector2[faces.Length * 4];
            int[] triangles = new int[faces.Length * 6];

            if (faces.Length > 0)
            {
                if (gpu)
                {
                    int groupThread = (faces.Length / 1024) + 1;

                    ComputeBuffer verticesBuffer = new ComputeBuffer(vertices.Length, Marshal.SizeOf<Vector3>());
                    verticesBuffer.SetData(vertices);

                    ComputeBuffer trianglesBuffer = new ComputeBuffer(triangles.Length, sizeof(int));
                    trianglesBuffer.SetData(triangles);

                    ComputeBuffer uvsBuffer = new ComputeBuffer(uvs.Length, Marshal.SizeOf<Vector2>());
                    uvsBuffer.SetData(uvs);

                    ComputeBuffer facesBuffer = new ComputeBuffer(faces.Length, Marshal.SizeOf<ChunkFace>());
                    facesBuffer.SetData(faces);


                    int kernel = FillMeshComputeShader.FindKernel("CSMain");
                    FillMeshComputeShader.SetBuffer(kernel, "vertices", verticesBuffer);
                    FillMeshComputeShader.SetBuffer(kernel, "triangles", trianglesBuffer);
                    FillMeshComputeShader.SetBuffer(kernel, "uvs", uvsBuffer);
                    FillMeshComputeShader.SetBuffer(kernel, "faces", facesBuffer);

                    FillMeshComputeShader.SetInt("faceCount", faces.Length);

                    FillMeshComputeShader.Dispatch(kernel, groupThread, 1, 1);

                    verticesBuffer.GetData(vertices);
                    trianglesBuffer.GetData(triangles);
                    uvsBuffer.GetData(uvs);

                    verticesBuffer.Release();
                    trianglesBuffer.Release();
                    uvsBuffer.Release();
                    facesBuffer.Release();
                }
                else
                {
                    int faceCount = 0;

                    foreach (var face in faces)
                    {
                        int vIndex = faceCount * 4;
                        int tIndex = faceCount * 6;

                        switch (face.direction)
                        {
                            case 0:
                                vertices[vIndex] = new Vector3(face.center.x - 0.5f, face.center.y + 0.5f, face.center.z + 0.5f);
                                vertices[vIndex + 1] = new Vector3(face.center.x + 0.5f, face.center.y + 0.5f, face.center.z + 0.5f);
                                vertices[vIndex + 2] = new Vector3(face.center.x + 0.5f, face.center.y + 0.5f, face.center.z - 0.5f);
                                vertices[vIndex + 3] = new Vector3(face.center.x - 0.5f, face.center.y + 0.5f, face.center.z - 0.5f);
                                break;
                            case 1:
                                vertices[vIndex] = new Vector3(face.center.x - 0.5f, face.center.y - 0.5f, face.center.z - 0.5f);
                                vertices[vIndex + 1] = new Vector3(face.center.x + 0.5f, face.center.y - 0.5f, face.center.z - 0.5f);
                                vertices[vIndex + 2] = new Vector3(face.center.x + 0.5f, face.center.y - 0.5f, face.center.z + 0.5f);
                                vertices[vIndex + 3] = new Vector3(face.center.x - 0.5f, face.center.y - 0.5f, face.center.z + 0.5f);
                                break;
                            case 2:
                                vertices[vIndex] = new Vector3(face.center.x - 0.5f, face.center.y - 0.5f, face.center.z + 0.5f);
                                vertices[vIndex + 1] = new Vector3(face.center.x - 0.5f, face.center.y + 0.5f, face.center.z + 0.5f);
                                vertices[vIndex + 2] = new Vector3(face.center.x - 0.5f, face.center.y + 0.5f, face.center.z - 0.5f);
                                vertices[vIndex + 3] = new Vector3(face.center.x - 0.5f, face.center.y - 0.5f, face.center.z - 0.5f);
                                break;
                            case 3:
                                vertices[vIndex] = new Vector3(face.center.x + 0.5f, face.center.y - 0.5f, face.center.z - 0.5f);
                                vertices[vIndex + 1] = new Vector3(face.center.x + 0.5f, face.center.y + 0.5f, face.center.z - 0.5f);
                                vertices[vIndex + 2] = new Vector3(face.center.x + 0.5f, face.center.y + 0.5f, face.center.z + 0.5f);
                                vertices[vIndex + 3] = new Vector3(face.center.x + 0.5f, face.center.y - 0.5f, face.center.z + 0.5f);
                                break;
                            case 4:
                                vertices[vIndex] = new Vector3(face.center.x + 0.5f, face.center.y - 0.5f, face.center.z + 0.5f);
                                vertices[vIndex + 1] = new Vector3(face.center.x + 0.5f, face.center.y + 0.5f, face.center.z + 0.5f);
                                vertices[vIndex + 2] = new Vector3(face.center.x - 0.5f, face.center.y + 0.5f, face.center.z + 0.5f);
                                vertices[vIndex + 3] = new Vector3(face.center.x - 0.5f, face.center.y - 0.5f, face.center.z + 0.5f);
                                break;
                            case 5:
                                vertices[vIndex] = new Vector3(face.center.x - 0.5f, face.center.y - 0.5f, face.center.z - 0.5f);
                                vertices[vIndex + 1] = new Vector3(face.center.x - 0.5f, face.center.y + 0.5f, face.center.z - 0.5f);
                                vertices[vIndex + 2] = new Vector3(face.center.x + 0.5f, face.center.y + 0.5f, face.center.z - 0.5f);
                                vertices[vIndex + 3] = new Vector3(face.center.x + 0.5f, face.center.y - 0.5f, face.center.z - 0.5f);
                                break;
                        }

                        triangles[tIndex] = vIndex;
                        triangles[tIndex + 1] = vIndex + 1;
                        triangles[tIndex + 2] = vIndex + 2;

                        triangles[tIndex + 3] = vIndex;
                        triangles[tIndex + 4] = vIndex + 2;
                        triangles[tIndex + 5] = vIndex + 3;

                        faceCount++;
                    }
                }
            }

            UpdateMesh(vertices, triangles, uvs);
        }

        void UpdateMesh(Vector3[] vertices, int[] triangles, Vector2[] uvs)
        {
            _filter.mesh = new Mesh();
            _filter.mesh.vertices = vertices;
            _filter.mesh.triangles = triangles;
            _filter.mesh.uv = uvs;

            _filter.mesh.RecalculateNormals();
        }

        public uint GetBloc(int x, int y, int z)
        {
            if (InRange(x) && InRange(y) && InRange(z))
                return Blocs[x + y * ChunkSize + z * ChunkSize * ChunkSize];
            else
                return _world.GetBloc(new Vector3(WorldPosition.x + x, WorldPosition.y + y, WorldPosition.z + z));
        }

        public bool InRange(int index)
        {
            return index >= 0 && index < ChunkSize;
        }
    }
}
