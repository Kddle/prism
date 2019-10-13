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

public static class Serialization
{
    static World world;

    public static string SavePath(string worldName)
    {
        string saveFolder = Path.Combine(Directory.GetCurrentDirectory(), "world_saves");

        if (!Directory.Exists(saveFolder))
            Directory.CreateDirectory(saveFolder);

        string worldSavePath = Path.Combine(saveFolder, $"{worldName}");

        if (!Directory.Exists(worldSavePath))
            Directory.CreateDirectory(worldSavePath);

        return worldSavePath;
    }

    public static void SaveChunk(Chunk chunk)
    {
        if (world == null)
            world = GameObject.FindObjectOfType<World>();

        string filepath = Path.Combine(SavePath(chunk.World.name), chunk.name);

        IFormatter formatter = new BinaryFormatter();
        Stream stream = new FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.None);

        formatter.Serialize(stream, chunk.Blocs);
        stream.Close();
    }

    public static bool Load(Chunk chunk)
    {
        if (world == null)
            world = GameObject.FindObjectOfType<World>();

        string filepath = Path.Combine(SavePath(chunk.World.name), chunk.name);

        if (!File.Exists(filepath))
            return false;

        IFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(filepath, FileMode.Open);

        chunk.Blocs = formatter.Deserialize(stream) as byte[,,];

        stream.Close();
        return true;
    }
}
