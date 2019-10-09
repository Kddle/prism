using SimplexNoise;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class MapUtils
{
    static int maxHeight = 40;
    static float scale = 0.01f;
    static int octaves = 6;
    static float persistence = 0.8f;

    public static int GenerateHeight(float x, float z)
    {
        float height = Map(0, maxHeight, 0, 1, fBM(x * scale, z * scale, octaves, persistence));
        return (int)height;
    }

    static float Map(float newMin, float newMax, float baseMin, float baseMax, float value)
    {
        return Mathf.Lerp(newMin, newMax, Mathf.InverseLerp(baseMin, baseMax, value));
    }

    // Fractal Bronian Motion
    static float fBM(float x, float z, int oct, float pers)
    {
        float total = 0;
        float frequency = 0.5f;
        float amplitude = 3f;
        float maxValue = 0;
        for (int i = 0; i < oct; i++)
        {
            total += Noise.Generate((x + 64)* frequency, (z + 64) * frequency) * amplitude;

            maxValue += amplitude;

            amplitude *= pers;
            frequency *= 2;
        }

        return total / maxValue;
    }
}
