//using SimplexNoise;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class TerrainGeneration
//{
//    float stoneBaseHeight = -24;
//    float stoneBaseNoise = 0.05f;
//    float stoneBaseNoiseHeight = 4;

//    float stoneMountainHeight = 48;
//    float stoneMountainFrequency = 0.008f;
//    float stoneMinHeight = -12;

//    float dirtBaseHeight = 1;
//    float dirtNoise = 0.04f;
//    float dirtNoiseHeight = 3;

//    List<Bloc> definitions;

//    float baseHeight = 12;
//    float maxHeight = 24;

//    float stoneHeight = 8;
//    float dirtHeight = 2;

//    int _chunkDimensionInBlocs;
//    float _blocSize;
//    float _chunkRealDimension;

//    public TerrainGeneration(List<Bloc> blocsDefinition, int chunkDimensionInBlocs, float blocSize)
//    {
//        definitions = blocsDefinition;
//        _blocSize = blocSize;
//        _chunkDimensionInBlocs = chunkDimensionInBlocs;
//        _chunkRealDimension = _blocSize * _chunkDimensionInBlocs;
//    }

//    //public Chunk GenerateChunk(Chunk chunk)
//    //{
//    //    for (int x = chunk.pos.x; x < chunk.pos.x + Chunk.ChunkSize; x++)
//    //        for (int z = chunk.pos.z; z < chunk.pos.z + Chunk.ChunkSize; z++)
//    //        {
//    //            chunk = ChunkColumnGeneration(chunk, x, z);
//    //        }

//    //    return chunk;
//    //}

//    public Chunk GenerateChunk3D(Chunk chunk)
//    {
//        for (float x = chunk.pos.x, xi = 0; x < chunk.pos.x + _chunkRealDimension; x += _blocSize, xi += 1)
//            for (float y = chunk.pos.y, yi = 0; y < chunk.pos.y + _chunkRealDimension; y += _blocSize, yi += 1)
//                for (float z = chunk.pos.z, zi = 0; z < chunk.pos.z + _chunkRealDimension; z += _blocSize, zi += 1)
//                {
//                    chunk = ChunkColumnGeneration(chunk, x, y, z, (int)xi, (int)yi, (int)zi);
//                }

//        return chunk;
//    }

//    public Chunk ChunkColumnGeneration(Chunk chunk, float x, float y, float z, int xi, int yi, int zi)
//    {
//        // TEST 1
//        //int baseHeight = 12;
//        //int max = 48;
//        //float value = GetNoiseNormalized(x, y, z, 0.035f, max);
//        //value *= baseHeight;

//        //if (value > max || y > baseHeight)
//        //    chunk.SetBloc(x - chunk.pos.x, y - chunk.pos.y, z - chunk.pos.z, definitions[0].id);
//        //else
//        //    chunk.SetBloc(x - chunk.pos.x, y - chunk.pos.y, z - chunk.pos.z, definitions[2].id);

//        //return chunk;

//        float totalDensity = 0;
//        float frequency = 0.015f;
//        float amplitude = 0.005f;
//        float maxValue = 0;
//        int octaves = 4;
//        float persistence = .5f;

//        for (int i = 0; i < octaves; i++)
//        {
//            totalDensity += Noise.Generate(x * frequency, y * frequency, z * frequency) * amplitude;

//            maxValue += amplitude;

//            amplitude *= persistence;
//            frequency *= 2f;
//        }

//        var density = totalDensity / maxValue;

//        density += (y / maxHeight);

//        if (density >= 0f)
//            chunk.SetBloc(xi, yi, zi, definitions[0].id);
//        else
//            chunk.SetBloc(xi, yi, zi, definitions[2].id);

//        // TEST 2
//        //if (y < baseHeight)
//        //{
//        //    float totalDensity = 0;
//        //    float frequency = 0.005f;
//        //    float amplitude = 0.05f;
//        //    float maxValue = 0;
//        //    int octaves = 4;
//        //    float persistence = .5f;

//        //    for (int i = 0; i < octaves; i++)
//        //    {
//        //        totalDensity += Noise.Generate(x * frequency, y * frequency, z * frequency) * amplitude;

//        //        maxValue += amplitude;

//        //        amplitude *= persistence;
//        //        frequency *= 2f;
//        //    }

//        //    var density = totalDensity / maxValue;

//        //    if (density >= 0.075f)
//        //        chunk.SetBloc(xi, yi, zi, definitions[0].id);
//        //    else
//        //        chunk.SetBloc(xi, yi, zi, definitions[1].id);
//        //}
//        //else
//        //{
//        //    float totalHeight = 0;
//        //    float frequency = 0.005f;
//        //    float amplitude = 0.004f;
//        //    float maxValue = 0;
//        //    int octaves = 6;
//        //    float persistence = 0.5f;

//        //    for (int i = 0; i < octaves; i++)
//        //    {
//        //        totalHeight += Noise.Generate(x * frequency, z * frequency) * amplitude;

//        //        maxValue += amplitude;

//        //        amplitude *= persistence;
//        //        frequency *= 2f;
//        //    }

//        //    var height = Mathf.Abs((totalHeight / maxValue)) * maxHeight;

//        //    if (y < height + baseHeight)
//        //        chunk.SetBloc(xi, yi, zi, definitions[2].id);
//        //    else
//        //        chunk.SetBloc(xi, yi, zi, definitions[0].id);
//        //}


//        return chunk;
//    }

//    //public Chunk ChunkColumnGeneration(Chunk chunk, int x, int z)
//    //{
//    //    int stoneHeight = Mathf.FloorToInt(stoneBaseHeight);
//    //    stoneHeight += GetNoise(x, 0, z, stoneMountainFrequency, Mathf.FloorToInt(stoneMountainHeight));

//    //    if (stoneHeight < stoneMinHeight)
//    //        stoneHeight = Mathf.FloorToInt(stoneMinHeight);

//    //    stoneHeight += GetNoise(x, 0, z, stoneBaseNoise, Mathf.FloorToInt(stoneBaseNoiseHeight));

//    //    int dirtHeight = stoneHeight + Mathf.FloorToInt(dirtBaseHeight);
//    //    dirtHeight += GetNoise(x, 100, z, dirtNoise, Mathf.FloorToInt(dirtNoiseHeight));

//    //    for (int y = chunk.pos.y; y < chunk.pos.y + Chunk.ChunkSize; y++)
//    //    {
//    //        if (y <= stoneHeight)
//    //        {
//    //            chunk.SetBloc(x - chunk.pos.x, y - chunk.pos.y, z - chunk.pos.z, definitions[1].id);
//    //        }
//    //        else if (y <= dirtHeight)
//    //        {
//    //            chunk.SetBloc(x - chunk.pos.x, y - chunk.pos.y, z - chunk.pos.z, definitions[2].id);
//    //        }
//    //        else
//    //        {
//    //            chunk.SetBloc(x - chunk.pos.x, y - chunk.pos.y, z - chunk.pos.z, definitions[0].id);
//    //        }

//    //    }

//    //    return chunk;
//    //}

//    public static int GetNoise(int x, int y, int z, float scale, int max)
//    {
//        return Mathf.FloorToInt((Noise.Generate(x * scale, y * scale, z * scale) + 1f) * (max / 2f));
//    }

//    public static float GetNoiseNormalized(float x, float y, float z, float scale, int max)
//    {
//        return Noise.Generate(x * scale, y * scale, z * scale) + 1f * (max / 2f);
//    }
//}
