using Prims;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Prism.Components
{
    public class MapGenerator : MonoBehaviour
    {
        public int MapWidth;
        public int MapHeight;
        public float NoiseScale;

        public bool autoUpdate = true;

        public void GenerateMap()
        {
            float[,] noiseMap = Noise.GenerateNoiseMap2D(MapWidth, MapHeight, NoiseScale);

            MapDisplay display = FindObjectOfType<MapDisplay>();
            display.DrawNoiseMap(noiseMap);
        }
    }
}

