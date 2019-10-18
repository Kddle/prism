using Prism.Map;
using Prism.Map.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Prism.NoiseConfigurations
{
    public abstract class NoiseMethod : ScriptableObject
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
        float frequencyMultiplier;

        public abstract byte[,,] FillChunk(Vector3 chunkWorldPosition, 
            WorldConfiguration worldConfiguration);

        public int Octaves => octaves;
        public float Frequency => frequency;
        public float Amplitude => amplitude;
        public float Persistence => persistence;
        public float FrequencyMultiplier => frequencyMultiplier;
    }
}
