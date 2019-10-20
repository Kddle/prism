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
            Load(true);
        }

        public BlocService(bool load = true)
        {
            _blocs = new Dictionary<BlocType, BlocData>();

            if (load)
                Load(true);
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

        public bool Load(bool debug = false)
        {
            if (!File.Exists(FilePath))
                return false;


            string fileContent;

            if (debug)
            {
                using (StreamReader sr = new StreamReader(FilePath))
                {
                    fileContent = sr.ReadToEnd();
                    sr.Close();
                }
            }
            else
            {
                IFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream(FilePath, FileMode.Open);

                fileContent = formatter.Deserialize(stream) as string;
                stream.Close();
            }

            BlocData[] blocsData = JsonConvert.DeserializeObject<BlocData[]>(fileContent);

            if (blocsData != null && blocsData.Length > 0)
            {
                for (int i = 0; i < blocsData.Length; i++)
                {
                    _blocs.Add((BlocType)i, blocsData[i]);
                }

                return true;
            }

            return false;
        }

        public BlocData[] GetBlocsDefinitionArray()
        {
            return Blocs.Values.ToArray();
        }

        public void SaveBlocs(string json, bool debug = false)
        {
            if (debug)
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.None);

                formatter.Serialize(stream, json);
                stream.Close();
            }
            else
            {
                using(StreamWriter sw = new StreamWriter(FilePath))
                {
                    sw.Write(json);
                    sw.Close();
                }
            }
        }

        public void SaveBlocs(bool debug = false)
        {
            var blocsData = _blocs.Values.ToArray();
            var fileContent = JsonConvert.SerializeObject(blocsData);

            SaveBlocs(fileContent, debug);
        }

        public void SaveBlocs(BlocData[] data, bool debug = false)
        {
            var fileContent = JsonConvert.SerializeObject(data);
            SaveBlocs(fileContent, debug);
        }

        #region MeshAndTextureServices

        //public MeshData GetMeshDataForBlocFromShader(MeshData meshData, BlocType blocType, Chunk chunk, Vector3Int blocPosition, float blocScale)
        //{

        //}

        public MeshData GetMeshDataForBloc(MeshData meshData, BlocType blocType, Chunk chunk, Vector3Int blocPosition, float blocScale)
        {
            // If the bloc we want to build is not visible => return
            if (Blocs[blocType].IsVisible == 0)
                return meshData;

            // Else, get neighbors
            if (Blocs[(BlocType)chunk.GetBloc(blocPosition.x, blocPosition.y + 1, blocPosition.z)].IsVisible == 0)
            {
                meshData = GetMeshDataUpFace(blocType, (blocPosition.x * blocScale), (blocPosition.y * blocScale), (blocPosition.z * blocScale), meshData, blocScale / 2f);
            }

            if (Blocs[(BlocType)chunk.GetBloc(blocPosition.x, blocPosition.y - 1, blocPosition.z)].IsVisible == 0)
            {
                meshData = GetMeshDataDownFace(blocType, (blocPosition.x * blocScale), (blocPosition.y * blocScale), (blocPosition.z * blocScale), meshData, blocScale / 2f);
            }

            if (Blocs[(BlocType)chunk.GetBloc(blocPosition.x, blocPosition.y, blocPosition.z + 1)].IsVisible == 0)
            {
                meshData = GetMeshDataNorthFace(blocType, (blocPosition.x * blocScale), (blocPosition.y * blocScale), (blocPosition.z * blocScale), meshData, blocScale / 2f);
            }

            if (Blocs[(BlocType)chunk.GetBloc(blocPosition.x, blocPosition.y, blocPosition.z - 1)].IsVisible == 0)
            {
                meshData = GetMeshDataSouthFace(blocType, (blocPosition.x * blocScale), (blocPosition.y * blocScale), (blocPosition.z * blocScale), meshData, blocScale / 2f);
            }

            if (Blocs[(BlocType)chunk.GetBloc(blocPosition.x + 1, blocPosition.y, blocPosition.z)].IsVisible == 0)
            {
                meshData = GetMeshDataEastFace(blocType, (blocPosition.x * blocScale), (blocPosition.y * blocScale), (blocPosition.z * blocScale), meshData, blocScale / 2f);
            }

            if (Blocs[(BlocType)chunk.GetBloc(blocPosition.x - 1, blocPosition.y, blocPosition.z)].IsVisible == 0)
            {
                meshData = GetMeshDataWestFace(blocType, (blocPosition.x * blocScale), (blocPosition.y * blocScale), (blocPosition.z * blocScale), meshData, blocScale / 2f);
            }

            return meshData;
        }

        protected virtual MeshData GetMeshDataUpFace
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

        protected virtual MeshData GetMeshDataDownFace
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

        protected virtual MeshData GetMeshDataNorthFace
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

        protected virtual MeshData GetMeshDataEastFace
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

        protected virtual MeshData GetMeshDataSouthFace
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

        protected virtual MeshData GetMeshDataWestFace
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

        //    if (Blocs[blocType].TextureData != null)
        //    {
        //        switch (Blocs[blocType].TextureData.Length)
        //        {
        //             Handle invisible blocs
        //            case 0:
        //                break;
        //        Handle blocs that uses the same texture on every sides
        //            case 2:
        //                x = Blocs[blocType].TextureData[0];
        //        y = Blocs[blocType].TextureData[1];
        //        break;
        //            case 6:
        //                /* Handle blocs with 3 textures :
        //                 * Array values [0-1] = Up Face
        //                 * Array values [2-3] = Down Face
        //                 * Array values [4-5] = Rest of Faces
        //                 */
        //                switch (direction)
        //        {
        //            case Direction.U:
        //                x = Blocs[blocType].TextureData[0];
        //                y = Blocs[blocType].TextureData[1];
        //                break;
        //            case Direction.D:
        //                x = Blocs[blocType].TextureData[2];
        //                y = Blocs[blocType].TextureData[3];
        //                break;
        //            default:
        //                x = Blocs[blocType].TextureData[4];
        //                y = Blocs[blocType].TextureData[5];
        //                break;
        //        }
        //        break;
        //        default:
        //                throw new NotImplementedException("Blocs cannot handle that much textures yet.");
        //    }
        //}

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

        #endregion
    }
}
