using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Prism.Map.Configuration
{
    [CreateAssetMenu(fileName = "NewNoiseConfiguration", menuName = "Prism/Noise Configuration")]
    public class NoiseConfiguration : ScriptableObject
    {
        [SerializeField]
        int octaves;
        [SerializeField]
        float frequency;
        [SerializeField]
        float amplitude;
        [SerializeField]
        float persistence;

        [SerializeField]
        float scale;

        [SerializeField]
        float densityThreshold;
        [SerializeField]
        float frequencyMultiplier;

        [SerializeField]
        float lacunarity;

        public int Octaves => octaves;
        public float Frequency => frequency;
        public float Amplitude => amplitude;
        public float Persistence => persistence;
        public float Scale => scale;
        public float DensityThreshold => densityThreshold;
        public float FrequencyMultiplier => frequencyMultiplier;

        public float Lacunarity => lacunarity;
    }
}
