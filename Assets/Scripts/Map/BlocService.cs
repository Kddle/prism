using Newtonsoft.Json;
using Prism.Map;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Prism.Services
{
    public class BlocService
    {
        private Dictionary<BlocType, BlocData> _blocs;
        public Dictionary<BlocType, BlocData> Blocs => _blocs;

        public const string _FILENAME = "blocs.prism";

        public const float _TILESIZE = 0.25f;

        public bool Loaded => Blocs != null;

        public BlocService()
        {
            _blocs = new Dictionary<BlocType, BlocData>();
            Load();
        }

        public BlocService(bool load = true)
        {
            _blocs = new Dictionary<BlocType, BlocData>();

            if (load)
                Load();
        }

        string SavePath
        {
            get
            {
                string saveFolder = Path.Combine(Directory.GetCurrentDirectory(), "prism_data");

                if (!Directory.Exists(saveFolder))
                    Directory.CreateDirectory(saveFolder);

                return saveFolder;
            }
        }

        string FilePath => Path.Combine(SavePath, _FILENAME);

        public bool Load()
        {
            if (!File.Exists(FilePath))
                return false;


            IFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(FilePath, FileMode.Open);

            string fileContent = formatter.Deserialize(stream) as string;
            stream.Close();

            BlocData[] blocsData = JsonConvert.DeserializeObject<BlocData[]>(fileContent);

            if (blocsData != null && blocsData.Length > 0)
            {
                foreach (var blocData in blocsData)
                {
                    _blocs.Add(blocData.BlocType, blocData);
                }

                return true;
            }

            return false;
        }

        public void SaveBlocs(string json)
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.None);

            formatter.Serialize(stream, json);
            stream.Close();
        }

        public void SaveBlocs()
        {
            var blocsData = _blocs.Values.ToArray();
            var fileContent = JsonConvert.SerializeObject(blocsData);

            SaveBlocs(fileContent);
        }

        public void SaveBlocs(BlocData[] data)
        {
            var fileContent = JsonConvert.SerializeObject(data);
            SaveBlocs(fileContent);
        }

        public MeshData GetMeshDataForBloc(MeshData meshData, BlocType blocType, Chunk chunk, Vector3Int blocPosition, float blocScale)
        {
            if (!Blocs[blocType].IsVisible)
                return meshData;

            if (!Blocs[(BlocType)chunk.GetBloc(blocPosition.x, blocPosition.y + 1, blocPosition.z)].IsSolid)
            {
                meshData = FaceDataUp(blocType, (blocPosition.x * blocScale), (blocPosition.y * blocScale), (blocPosition.z * blocScale), meshData, blocScale / 2f);
            }

            if (!Blocs[(BlocType)chunk.GetBloc(blocPosition.x, blocPosition.y - 1, blocPosition.z)].IsSolid)
            {
                meshData = FaceDataDown(blocType, (blocPosition.x * blocScale), (blocPosition.y * blocScale), (blocPosition.z * blocScale), meshData, blocScale / 2f);
            }

            if (!Blocs[(BlocType)chunk.GetBloc(blocPosition.x, blocPosition.y, blocPosition.z + 1)].IsSolid)
            {
                meshData = FaceDataNorth(blocType, (blocPosition.x * blocScale), (blocPosition.y * blocScale), (blocPosition.z * blocScale), meshData, blocScale / 2f);
            }

            if (!Blocs[(BlocType)chunk.GetBloc(blocPosition.x, blocPosition.y, blocPosition.z - 1)].IsSolid)
            {
                meshData = FaceDataSouth(blocType, (blocPosition.x * blocScale), (blocPosition.y * blocScale), (blocPosition.z * blocScale), meshData, blocScale / 2f);
            }

            if (!Blocs[(BlocType)chunk.GetBloc(blocPosition.x + 1, blocPosition.y, blocPosition.z)].IsSolid)
            {
                meshData = FaceDataEast(blocType, (blocPosition.x * blocScale), (blocPosition.y * blocScale), (blocPosition.z * blocScale), meshData, blocScale / 2f);
            }

            if (!Blocs[(BlocType)chunk.GetBloc(blocPosition.x - 1, blocPosition.y, blocPosition.z)].IsSolid)
            {
                meshData = FaceDataWest(blocType, (blocPosition.x * blocScale), (blocPosition.y * blocScale), (blocPosition.z * blocScale), meshData, blocScale / 2f);
            }

            return meshData;
        }

        protected virtual MeshData FaceDataUp
         (BlocType blocType, float x, float y, float z, MeshData meshData, float radius)
        {
            meshData.vertices.Add(new Vector3(x - radius, y + radius, z + radius));
            meshData.vertices.Add(new Vector3(x + radius, y + radius, z + radius));
            meshData.vertices.Add(new Vector3(x + radius, y + radius, z - radius));
            meshData.vertices.Add(new Vector3(x - radius, y + radius, z - radius));

            meshData.AddQuadTriangles();

            meshData.uv.AddRange(FaceUVs(Direction.U, blocType));

            return meshData;
        }

        protected virtual MeshData FaceDataDown
            (BlocType blocType, float x, float y, float z, MeshData meshData, float radius)
        {
            meshData.vertices.Add(new Vector3(x - radius, y - radius, z - radius));
            meshData.vertices.Add(new Vector3(x + radius, y - radius, z - radius));
            meshData.vertices.Add(new Vector3(x + radius, y - radius, z + radius));
            meshData.vertices.Add(new Vector3(x - radius, y - radius, z + radius));

            meshData.AddQuadTriangles();

            meshData.uv.AddRange(FaceUVs(Direction.D, blocType));
            return meshData;
        }

        protected virtual MeshData FaceDataNorth
            (BlocType blocType, float x, float y, float z, MeshData meshData, float radius)
        {
            meshData.vertices.Add(new Vector3(x + radius, y - radius, z + radius));
            meshData.vertices.Add(new Vector3(x + radius, y + radius, z + radius));
            meshData.vertices.Add(new Vector3(x - radius, y + radius, z + radius));
            meshData.vertices.Add(new Vector3(x - radius, y - radius, z + radius));

            meshData.AddQuadTriangles();
            meshData.uv.AddRange(FaceUVs(Direction.N, blocType));
            return meshData;
        }

        protected virtual MeshData FaceDataEast
            (BlocType blocType, float x, float y, float z, MeshData meshData, float radius)
        {
            meshData.vertices.Add(new Vector3(x + radius, y - radius, z - radius));
            meshData.vertices.Add(new Vector3(x + radius, y + radius, z - radius));
            meshData.vertices.Add(new Vector3(x + radius, y + radius, z + radius));
            meshData.vertices.Add(new Vector3(x + radius, y - radius, z + radius));

            meshData.AddQuadTriangles();
            meshData.uv.AddRange(FaceUVs(Direction.E, blocType));
            return meshData;
        }

        protected virtual MeshData FaceDataSouth
            (BlocType blocType, float x, float y, float z, MeshData meshData, float radius)
        {
            meshData.vertices.Add(new Vector3(x - radius, y - radius, z - radius));
            meshData.vertices.Add(new Vector3(x - radius, y + radius, z - radius));
            meshData.vertices.Add(new Vector3(x + radius, y + radius, z - radius));
            meshData.vertices.Add(new Vector3(x + radius, y - radius, z - radius));

            meshData.AddQuadTriangles();
            meshData.uv.AddRange(FaceUVs(Direction.S, blocType));
            return meshData;
        }

        protected virtual MeshData FaceDataWest
            (BlocType blocType, float x, float y, float z, MeshData meshData, float radius)
        {
            meshData.vertices.Add(new Vector3(x - radius, y - radius, z + radius));
            meshData.vertices.Add(new Vector3(x - radius, y + radius, z + radius));
            meshData.vertices.Add(new Vector3(x - radius, y + radius, z - radius));
            meshData.vertices.Add(new Vector3(x - radius, y - radius, z - radius));

            meshData.AddQuadTriangles();
            meshData.uv.AddRange(FaceUVs(Direction.W, blocType));
            return meshData;
        }

        public virtual Tile TexturePosition(Direction direction, BlocType blocType)
        {
            int x = 0;
            int y = 0;

            if (Blocs[blocType].TextureData != null)
            {
                switch (Blocs[blocType].TextureData.Length)
                {
                    // Handle invisible blocs
                    case 0:
                        break;
                        // Handle blocs that uses the same texture on every sides
                    case 2:
                        x = Blocs[blocType].TextureData[0];
                        y = Blocs[blocType].TextureData[1];
                        break;
                    case 6:
                        /* Handle blocs with 3 textures :
                         * Array values [0-1] = Up Face
                         * Array values [2-3] = Down Face
                         * Array values [4-5] = Rest of Faces
                         */
                        switch (direction)
                        {
                            case Direction.U:
                                x = Blocs[blocType].TextureData[0];
                                y = Blocs[blocType].TextureData[1];
                                break;
                            case Direction.D:
                                x = Blocs[blocType].TextureData[2];
                                y = Blocs[blocType].TextureData[3];
                                break;
                            default:
                                x = Blocs[blocType].TextureData[4];
                                y = Blocs[blocType].TextureData[5];
                                break;
                        }
                        break;
                    default:
                        throw new NotImplementedException("Blocs cannot handle that much textures yet.");
                }
            }

            Tile tile = new Tile();
            tile.x = x;
            tile.y = y;

            return tile;
        }

        public virtual Vector2[] FaceUVs(Direction direction, BlocType blocType)
        {
            Vector2[] uvs = new Vector2[4];
            Tile tilePos = TexturePosition(direction, blocType);

            uvs[0] = new Vector2(_TILESIZE * tilePos.x + _TILESIZE, _TILESIZE * tilePos.y);
            uvs[1] = new Vector2(_TILESIZE * tilePos.x + _TILESIZE, _TILESIZE * tilePos.y + _TILESIZE);
            uvs[2] = new Vector2(_TILESIZE * tilePos.x, _TILESIZE * tilePos.y + _TILESIZE);
            uvs[3] = new Vector2(_TILESIZE * tilePos.x, _TILESIZE * tilePos.y);

            return uvs;
        }
    }
}
