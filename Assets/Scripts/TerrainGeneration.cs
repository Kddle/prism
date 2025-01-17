﻿using SimplexNoise;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGeneration
{
    float stoneBaseHeight = -24;
    float stoneBaseNoise = 0.05f;
    float stoneBaseNoiseHeight = 4;

    float stoneMountainHeight = 48;
    float stoneMountainFrequency = 0.008f;
    float stoneMinHeight = -12;

    float dirtBaseHeight = 1;
    float dirtNoise = 0.04f;
    float dirtNoiseHeight = 3;

    List<Bloc> definitions;

    float baseHeight = 12;
    float maxHeight = 40;

    public TerrainGeneration(List<Bloc> blocsDefinition)
    {
        definitions = blocsDefinition;
    }

    public Chunk GenerateChunk(Chunk chunk)
    {
        for (int x = chunk.pos.x; x < chunk.pos.x + Chunk.ChunkSize; x++)
            for (int z = chunk.pos.z; z < chunk.pos.z + Chunk.ChunkSize; z++)
            {
                chunk = ChunkColumnGeneration(chunk, x, z);
            }

        return chunk;
    }

    public Chunk GenerateChunk3D(Chunk chunk)
    {
        for (int x = chunk.pos.x; x < chunk.pos.x + Chunk.ChunkSize; x++)
            for (int y = chunk.pos.y; y < chunk.pos.y + Chunk.ChunkSize; y++)
                for (int z = chunk.pos.z; z < chunk.pos.z + Chunk.ChunkSize; z++)
                {
                    chunk = ChunkColumnGeneration(chunk, x, y, z);
                }

        return chunk;
    }

    public Chunk ChunkColumnGeneration(Chunk chunk, int x, int y, int z)
    {
        // TEST 1
        //int baseHeight = 12;
        //int max = 48;
        //float value = GetNoiseNormalized(x, y, z, 0.035f, max);
        //value *= baseHeight;

        //if (value > max || y > baseHeight)
        //    chunk.SetBloc(x - chunk.pos.x, y - chunk.pos.y, z - chunk.pos.z, definitions[0].id);
        //else
        //    chunk.SetBloc(x - chunk.pos.x, y - chunk.pos.y, z - chunk.pos.z, definitions[2].id);

        //return chunk;

        // TEST 2
        if(y < baseHeight)
        {
            float totalDensity = 0;
            float frequency = 0.05f;
            float amplitude = 0.05f;
            float maxValue = 0;
            int octaves = 4;
            float persistence = .5f;

            for (int i = 0; i < octaves; i++)
            {
                totalDensity += Noise.Generate(x * frequency, y * frequency, z * frequency) * amplitude;

                maxValue += amplitude;

                amplitude *= persistence;
                frequency *= 2f;
            }

            var density = totalDensity / maxValue;

            if (density >= 0)
                chunk.SetBloc(x - chunk.pos.x, y - chunk.pos.y, z - chunk.pos.z, definitions[1].id);
            else
                chunk.SetBloc(x - chunk.pos.x, y - chunk.pos.y, z - chunk.pos.z, definitions[0].id);
        }
        else
        {
            float totalHeight = 0;
            float frequency = 0.005f;
            float amplitude = 0.004f;
            float maxValue = 0;
            int octaves = 6;
            float persistence = 0.5f;

            for (int i = 0; i < octaves; i++)
            {
                totalHeight += Noise.Generate((x + 64) * frequency, (z + 64) * frequency) * amplitude;

                maxValue += amplitude;

                amplitude *= persistence;
                frequency *= 2f;
            }

            var height = Mathf.Abs((totalHeight / maxValue)) * maxHeight;

            if (y < height + baseHeight)
                chunk.SetBloc(x - chunk.pos.x, y - chunk.pos.y, z - chunk.pos.z, definitions[2].id);
            else
                chunk.SetBloc(x - chunk.pos.x, y - chunk.pos.y, z - chunk.pos.z, definitions[0].id);
        }
        

        return chunk;
    }

    public Chunk ChunkColumnGeneration(Chunk chunk, int x, int z)
    {
        int stoneHeight = Mathf.FloorToInt(stoneBaseHeight);
        stoneHeight += GetNoise(x, 0, z, stoneMountainFrequency, Mathf.FloorToInt(stoneMountainHeight));

        if (stoneHeight < stoneMinHeight)
            stoneHeight = Mathf.FloorToInt(stoneMinHeight);

        stoneHeight += GetNoise(x, 0, z, stoneBaseNoise, Mathf.FloorToInt(stoneBaseNoiseHeight));

        int dirtHeight = stoneHeight + Mathf.FloorToInt(dirtBaseHeight);
        dirtHeight += GetNoise(x, 100, z, dirtNoise, Mathf.FloorToInt(dirtNoiseHeight));

        for (int y = chunk.pos.y; y < chunk.pos.y + Chunk.ChunkSize; y++)
        {
            if (y <= stoneHeight)
            {
                chunk.SetBloc(x - chunk.pos.x, y - chunk.pos.y, z - chunk.pos.z, definitions[1].id);
            }
            else if (y <= dirtHeight)
            {
                chunk.SetBloc(x - chunk.pos.x, y - chunk.pos.y, z - chunk.pos.z, definitions[2].id);
            }
            else
            {
                chunk.SetBloc(x - chunk.pos.x, y - chunk.pos.y, z - chunk.pos.z, definitions[0].id);
            }

        }

        return chunk;
    }

    public static int GetNoise(int x, int y, int z, float scale, int max)
    {
        return Mathf.FloorToInt((Noise.Generate(x * scale, y * scale, z * scale) + 1f) * (max / 2f));
    }

    public static float GetNoiseNormalized(float x, float y, float z, float scale, int max)
    {
        return Noise.Generate(x * scale, y * scale, z * scale) + 1f * (max / 2f);
    }
}
