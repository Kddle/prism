using Prism.Rework.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Prism.Rework
{
    public enum NoiseType { PERLIN = 0, SIMPLEX = 1 }

    [CreateAssetMenu(fileName = "NewNoiseConfiguration", menuName = "Prism/Configuration Objects/Noise Configuration")]
    public class NoiseConfiguration : ScriptableObject
    {
        public int octaves;
        public NoiseType noiseType;
        public float frequency;
        public float frequencyMultiplier;
        public float persistence;
        public float smoothFactor;
        public float amplitude;
    }

    public static class NoiseConfigurationExtensions
    {
        public static NoiseConfig ToNoiseConfig(this NoiseConfiguration nc)
        {
            return new NoiseConfig()
            {
                frequency = nc.frequency,
                frequencyMultiplier = nc.frequencyMultiplier,
                noiseType = (int)nc.noiseType,
                octaves = nc.octaves,
                persistence = nc.persistence,
                smoothFactor = nc.smoothFactor,
                amplitude = nc.amplitude
            };
        }
    }
}