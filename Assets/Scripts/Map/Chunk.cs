using Prism.Map.Configuration;
using Prism.NoiseConfigurations;
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

        public Vector2Int NormalPosition;
        public Vector3Int WorldPosition;
        public World World => transform.parent.GetComponent<World>();

        public byte[,,] Blocs;

        public bool shouldUpdate = false;

        private void Update()
        {
            if(shouldUpdate)
            {
                shouldUpdate = false;
                UpdateChunk();
            }
        }

        public void Initialize(Vector3Int worldPosition, int chunkSize)
        {
            Blocs = new byte[chunkSize, chunkSize, chunkSize];
            name = $"{worldPosition.x}_{worldPosition.y}_{worldPosition.z}";
            WorldPosition = worldPosition;

            _filter = GetComponent<MeshFilter>();
            _collider = GetComponent<MeshCollider>();
        }

        public void Init()
        {
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
            try
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
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public void Generate(NoiseMethod noiseMethod)
        {
            Blocs = noiseMethod.FillChunk(transform.position, World.WorldConfiguration);
        }

        public bool InRange(int index)
        {
            if (index < 0 || index >= World.WorldConfiguration.ChunkSideLength)
                return false;

            return true;
        }

        public bool InRangeUp(int y)
        {
            if (y < 0 || y >= World.WorldConfiguration.MaxWorldHeight)
                return false;

            return true;
        }

        public void UpdateChunk()
        {
            MeshData meshData = new MeshData();

            for (int x = 0; x < World.WorldConfiguration.ChunkSideLength; x++)
                for (int y = 0; y < World.WorldConfiguration.ChunkSideLength; y++)
                    for (int z = 0; z < World.WorldConfiguration.ChunkSideLength; z++)
                    {
                        meshData = World.BlocService.GetMeshDataForBloc(meshData, (BlocType)Blocs[x, y, z], this, new Vector3Int(x, y, z), World.WorldConfiguration.BlocScale);
                        //meshData = World.BlocsDefinition[Blocs[x, y, z]].FillData(this, x, y, z, meshData, World.BlocsDefinition, World.WorldConfiguration.BlocScale);
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
