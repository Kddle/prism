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
                UpdateChunk();
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

        public void UpdateChunk()
        {
            MeshData meshData = new MeshData();
            int chunkLength = World.WorldConfiguration.ChunkSideLength;
            float blocScale = World.WorldConfiguration.BlocScale;

            for (int x = 0; x < World.WorldConfiguration.ChunkSideLength; x++)
                for (int y = 0; y < World.WorldConfiguration.ChunkSideLength; y++)
                    for (int z = 0; z < World.WorldConfiguration.ChunkSideLength; z++)
                    {
                        meshData = GetMeshDataForBloc(meshData, Blocs[x, y, z], new Vector3Int(x, y, z), blocScale);
                    }

            RenderChunk(meshData);
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

            //Debug.Log($"Chunk[{gameObject.name}] | Vertices [{_filter.mesh.vertices.Length}] | Triangles [{_filter.mesh.triangles.Length}]");

            if (UseRenderForCollision)
            {
                Mesh mesh = new Mesh();
                mesh.vertices = _filter.mesh.vertices;
                mesh.triangles = _filter.mesh.triangles;
                mesh.RecalculateNormals();

                _collider.sharedMesh = mesh;
            }
        }

        #region MeshAndTextureServices

        public MeshData GetMeshDataForBloc(MeshData meshData, byte bloc, Vector3Int blocPosition, float blocScale)
        {
            // If the bloc we want to build is not visible => return
            if (bloc == (byte)BlocType.AIR)
                return meshData;

            // Else, get neighbors
            if (GetBloc(blocPosition.x, blocPosition.y + 1, blocPosition.z) == (byte)BlocType.AIR)
            {
                meshData = GetMeshDataUpFace(bloc, (blocPosition.x * blocScale), (blocPosition.y * blocScale), (blocPosition.z * blocScale), meshData, blocScale / 2f);
            }

            if (GetBloc(blocPosition.x, blocPosition.y - 1, blocPosition.z) == (byte)BlocType.AIR)
            {
                meshData = GetMeshDataDownFace(bloc, (blocPosition.x * blocScale), (blocPosition.y * blocScale), (blocPosition.z * blocScale), meshData, blocScale / 2f);
            }

            if (GetBloc(blocPosition.x, blocPosition.y, blocPosition.z + 1) == (byte)BlocType.AIR)
            {
                meshData = GetMeshDataNorthFace(bloc, (blocPosition.x * blocScale), (blocPosition.y * blocScale), (blocPosition.z * blocScale), meshData, blocScale / 2f);
            }

            if (GetBloc(blocPosition.x, blocPosition.y, blocPosition.z - 1) == (byte)BlocType.AIR)
            {
                meshData = GetMeshDataSouthFace(bloc, (blocPosition.x * blocScale), (blocPosition.y * blocScale), (blocPosition.z * blocScale), meshData, blocScale / 2f);
            }

            if (GetBloc(blocPosition.x + 1, blocPosition.y, blocPosition.z) == (byte)BlocType.AIR)
            {
                meshData = GetMeshDataEastFace(bloc, (blocPosition.x * blocScale), (blocPosition.y * blocScale), (blocPosition.z * blocScale), meshData, blocScale / 2f);
            }

            if (GetBloc(blocPosition.x - 1, blocPosition.y, blocPosition.z) == (byte)BlocType.AIR)
            {
                meshData = GetMeshDataWestFace(bloc, (blocPosition.x * blocScale), (blocPosition.y * blocScale), (blocPosition.z * blocScale), meshData, blocScale / 2f);
            }

            return meshData;
        }

        protected virtual MeshData GetMeshDataUpFace
         (byte blocType, float x, float y, float z, MeshData meshData, float radius)
        {
            meshData.vertices.Add(new Vector3(x - radius, y + radius, z + radius));
            meshData.vertices.Add(new Vector3(x + radius, y + radius, z + radius));
            meshData.vertices.Add(new Vector3(x + radius, y + radius, z - radius));
            meshData.vertices.Add(new Vector3(x - radius, y + radius, z - radius));

            meshData.AddQuadTriangles();

            meshData.uv.AddRange(FaceUVs(Direction.U, blocType));

            return meshData;
        }

        protected virtual MeshData GetMeshDataDownFace
            (byte blocType, float x, float y, float z, MeshData meshData, float radius)
        {
            meshData.vertices.Add(new Vector3(x - radius, y - radius, z - radius));
            meshData.vertices.Add(new Vector3(x + radius, y - radius, z - radius));
            meshData.vertices.Add(new Vector3(x + radius, y - radius, z + radius));
            meshData.vertices.Add(new Vector3(x - radius, y - radius, z + radius));

            meshData.AddQuadTriangles();

            meshData.uv.AddRange(FaceUVs(Direction.D, blocType));
            return meshData;
        }

        protected virtual MeshData GetMeshDataNorthFace
            (byte blocType, float x, float y, float z, MeshData meshData, float radius)
        {
            meshData.vertices.Add(new Vector3(x + radius, y - radius, z + radius));
            meshData.vertices.Add(new Vector3(x + radius, y + radius, z + radius));
            meshData.vertices.Add(new Vector3(x - radius, y + radius, z + radius));
            meshData.vertices.Add(new Vector3(x - radius, y - radius, z + radius));

            meshData.AddQuadTriangles();
            meshData.uv.AddRange(FaceUVs(Direction.N, blocType));
            return meshData;
        }

        protected virtual MeshData GetMeshDataEastFace
            (byte blocType, float x, float y, float z, MeshData meshData, float radius)
        {
            meshData.vertices.Add(new Vector3(x + radius, y - radius, z - radius));
            meshData.vertices.Add(new Vector3(x + radius, y + radius, z - radius));
            meshData.vertices.Add(new Vector3(x + radius, y + radius, z + radius));
            meshData.vertices.Add(new Vector3(x + radius, y - radius, z + radius));

            meshData.AddQuadTriangles();
            meshData.uv.AddRange(FaceUVs(Direction.E, blocType));
            return meshData;
        }

        protected virtual MeshData GetMeshDataSouthFace
            (byte blocType, float x, float y, float z, MeshData meshData, float radius)
        {
            meshData.vertices.Add(new Vector3(x - radius, y - radius, z - radius));
            meshData.vertices.Add(new Vector3(x - radius, y + radius, z - radius));
            meshData.vertices.Add(new Vector3(x + radius, y + radius, z - radius));
            meshData.vertices.Add(new Vector3(x + radius, y - radius, z - radius));

            meshData.AddQuadTriangles();
            meshData.uv.AddRange(FaceUVs(Direction.S, blocType));
            return meshData;
        }

        protected virtual MeshData GetMeshDataWestFace
            (byte blocType, float x, float y, float z, MeshData meshData, float radius)
        {
            meshData.vertices.Add(new Vector3(x - radius, y - radius, z + radius));
            meshData.vertices.Add(new Vector3(x - radius, y + radius, z + radius));
            meshData.vertices.Add(new Vector3(x - radius, y + radius, z - radius));
            meshData.vertices.Add(new Vector3(x - radius, y - radius, z - radius));

            meshData.AddQuadTriangles();
            meshData.uv.AddRange(FaceUVs(Direction.W, blocType));
            return meshData;
        }

        public virtual Tile TexturePosition(Direction direction, byte blocType)
        {
            int x = 0;
            int y = 0;

            BlocData blocData = World.BlocService.Blocs[(BlocType)blocType];

            switch (direction)
            {
                case Direction.U:
                    x = blocData.UpTextureCoordinates.x;
                    y = blocData.UpTextureCoordinates.y;
                    break;
                case Direction.D:
                    x = blocData.DownTextureCoordinates.x;
                    y = blocData.DownTextureCoordinates.y;
                    break;
                default:
                    x = blocData.RestTextureCoordinates.x;
                    y = blocData.RestTextureCoordinates.y;
                    break;
            }

            Tile tile = new Tile();
            tile.x = x;
            tile.y = y;

            return tile;
        }

        public virtual Vector2[] FaceUVs(Direction direction, byte blocType)
        {
            Vector2[] uvs = new Vector2[4];
            Tile tilePos = TexturePosition(direction, blocType);
            float tileSize = BlocService._TILESIZE;

            uvs[0] = new Vector2(tileSize * tilePos.x + tileSize, tileSize * tilePos.y);
            uvs[1] = new Vector2(tileSize * tilePos.x + tileSize, tileSize * tilePos.y + tileSize);
            uvs[2] = new Vector2(tileSize * tilePos.x, tileSize * tilePos.y + tileSize);
            uvs[3] = new Vector2(tileSize * tilePos.x, tileSize * tilePos.y);

            return uvs;
        }

        #endregion
    }
}