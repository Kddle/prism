using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prims
{
    public static class Noise
    {
        public static float[,] GenerateNoiseMap2D(int width, int height, float scale)
        {
            float[,] noiseMap = new float[width, height];

            if (scale <= 0)
                scale = 0.0001f;

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    float sampleX = x / scale;
                    float sampleY = y / scale;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                    noiseMap[x, y] = perlinValue;
                }

            return noiseMap;

        }
    }
}

