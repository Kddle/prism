using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Rework.Structs
{
    public struct NoiseConfig
    {
        public int octaves;
        public int noiseType;
        public float frequency;
        public float frequencyMultiplier;
        public float persistence;
        public float smoothFactor;
        public float amplitude;
    }
}
