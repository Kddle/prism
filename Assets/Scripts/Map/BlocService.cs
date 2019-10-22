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
                using (StreamWriter sw = new StreamWriter(FilePath))
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
    }
}
