﻿using Prism.Components;
using UnityEditor;
using UnityEngine;

namespace Prism.EditorExtensions
{
    [CustomEditor(typeof(MapGenerator))]
    public class MapGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            MapGenerator mapGen = (MapGenerator)target;

            if(DrawDefaultInspector())
            {
                if(mapGen.autoUpdate)
                    mapGen.GenerateMap();
            }

            if(GUILayout.Button("Generate"))
            {
                mapGen.GenerateMap();
            }
        }
    }
}
