﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Prism.Map.Configuration
{
    [CreateAssetMenu(fileName = "NewWorldConfiguration", menuName = "Prism/World Configuration")]
    public class WorldConfiguration : ScriptableObject
    {
        [Tooltip("Length of X & Z side of a chunk in blocs.")]
        public int ChunkSideLength;

        [Tooltip("Max height of the world in blocs.")]
        public int MaxWorldHeight;

        [Tooltip("Size of 1 bloc unit. (Please keep it a multiple of 0.05.)")]
        public float BlocScale;

        public float ChunkWorldSize => ChunkSideLength * BlocScale;

        public int HorizontalRenderDistance;
        public int VerticalRenderDistance;
    }
}