using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Map;
using Prism.Map.Configuration;
using UnityEngine;

namespace Prism.NoiseConfigurations
{
    [CreateAssetMenu(fileName = "NewDefaultUnityPerlinNoise2DConfiguration", menuName = "Prism/Noise Methods/ Unity Perlin Noise 2D")]
    public class Default2DPerlinNoise : NoiseMethod
    {
        public int BaseHeight = 20;

        public override byte[,,] FillChunk(Vector3 chunkWorldPosition, WorldConfiguration worldConfiguration)
        {
            float[,] maxHeights = new float[worldConfiguration.ChunkSideLength, worldConfiguration.ChunkSideLength];

            byte[,,] blocs = new byte[worldConfiguration.ChunkSideLength, worldConfiguration.ChunkSideLength, worldConfiguration.ChunkSideLength];

            for (int x = 0; x < worldConfiguration.ChunkSideLength; x++)
                for (int z = 0; z < worldConfiguration.ChunkSideLength; z++)
                {
                    Vector2 blocWorldPosition = new Vector2(
                            chunkWorldPosition.x + (x * worldConfiguration.BlocScale),
                            chunkWorldPosition.z + (z * worldConfiguration.BlocScale)
                        );

                    maxHeights[x, z] = Get2DNoise(blocWorldPosition.x, blocWorldPosition.y);

                    for (int y = 0; y < worldConfiguration.ChunkSideLength; y++)
                    {
                        if (y < BaseHeight)
                            blocs[x, y, z] = World.BlocService.Blocs[BlocType.ROCK].Id;
                        else if (y <= Mathf.RoundToInt(maxHeights[x, z] * worldConfiguration.ChunkSideLength))
                            blocs[x, y, z] = World.BlocService.Blocs[BlocType.ROCK].Id;
                        else
                            blocs[x, y, z] = World.BlocService.Blocs[BlocType.AIR].Id;
                    }
                }

            return blocs;
        }

        protected float Get2DNoise(float x, float z)
        {
            float total = 0;
            float frequency = Frequency;
            float amplitude = Amplitude;
            float maxValue = 0;            // Used for normalizing result to 0.0 - 1.0
            for (int i = 0; i < Octaves; i++)
            {
                total += Mathf.PerlinNoise(x * frequency, z * frequency) * amplitude;

                maxValue += amplitude;

                amplitude *= Persistence;
                frequency *= FrequencyMultiplier;
            }

            return total / maxValue;
        }
    }
}
