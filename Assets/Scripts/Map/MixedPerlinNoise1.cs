using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Map;
using Prism.Map.Configuration;
using Prism.NoiseFunc;
using UnityEngine;

namespace Prism.NoiseConfigurations
{
    [CreateAssetMenu(fileName = "NewMixedPerlinNoise1", menuName = "Prism/Noise Methods/Mixed Perlin Noise 1")]
    public class MixedPerlinNoise1 : Default2DPerlinNoise
    {
        public double Frequency3D;
        public double Amplitude3D;
        public int Octaves3D;
        public int Persistence3D;
        public double FrequencyMultiplier3D;
        public double AirThreshold;

        public override byte[,,] FillChunk(Vector3 chunkWorldPosition, WorldConfiguration worldConfiguration)
        {
            var pNoise = new Perlin();

            float[,] maxHeights = new float[worldConfiguration.ChunkSideLength, worldConfiguration.ChunkSideLength];
            byte[,,] blocs = new byte[worldConfiguration.ChunkSideLength, worldConfiguration.ChunkSideLength, worldConfiguration.ChunkSideLength];

            for (int x = 0; x < worldConfiguration.ChunkSideLength; x++)
                for (int z = 0; z < worldConfiguration.ChunkSideLength; z++)
                {
                    Vector2 blocWorldPosition = new Vector2(
                            chunkWorldPosition.x + (x * worldConfiguration.BlocScale),
                            chunkWorldPosition.z + (z * worldConfiguration.BlocScale)
                        );

                    maxHeights[x, z] = Get2DNoise(blocWorldPosition.x, blocWorldPosition.y) * worldConfiguration.MaxWorldHeight;
                }

            for (int x = 0; x < worldConfiguration.ChunkSideLength; x++)
                for (int z = 0; z < worldConfiguration.ChunkSideLength; z++)
                    for (int y = 0; y < worldConfiguration.ChunkSideLength; y++)
                    {
                        // We test the height with the world position.
                        float currentY = chunkWorldPosition.y + (y * worldConfiguration.BlocScale);

                        if (currentY < BaseHeight)
                            blocs[x, y, z] = World.BlocService.Blocs[BlocType.ROCK].Id;
                        else if (currentY <= maxHeights[x, z])
                            blocs[x, y, z] = World.BlocService.Blocs[BlocType.GRASS].Id;
                        else
                            blocs[x, y, z] = World.BlocService.Blocs[BlocType.AIR].Id;
                    }

            for (int x = 0; x < worldConfiguration.ChunkSideLength; x++)
                for (int z = 0; z < worldConfiguration.ChunkSideLength; z++)
                    for (int y = 0; y < worldConfiguration.ChunkSideLength; y++)
                    {
                        Vector3 blocWorldPosition = new Vector3(
                            chunkWorldPosition.x + (x * worldConfiguration.BlocScale),
                            chunkWorldPosition.y + (y * worldConfiguration.BlocScale),
                            chunkWorldPosition.z + (z * worldConfiguration.BlocScale)
                        );
                        var value = pNoise.OctavePerlin(blocWorldPosition.x, blocWorldPosition.y, blocWorldPosition.z, Octaves3D, Persistence3D, Frequency3D, Amplitude3D, FrequencyMultiplier3D);
                        bool isAir = value <= AirThreshold;

                        if (isAir)
                            blocs[x, y, z] = World.BlocService.Blocs[BlocType.AIR].Id;
                    }

            return blocs;
        }
    }
}
