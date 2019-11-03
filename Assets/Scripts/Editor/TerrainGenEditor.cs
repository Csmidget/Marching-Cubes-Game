﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainGenerator))]
public class TerrainGenEditor : Editor
{

    public override void OnInspectorGUI()
    {
        TerrainGenerator terrainGen = (TerrainGenerator)target;

        if (DrawDefaultInspector())
        {
            if (terrainGen.autoUpdate)
                terrainGen.GenerateMap();
        }

        if (GUILayout.Button("Generate"))
        {
            terrainGen.GenerateMap();
        }
        if (GUILayout.Button("Reset"))
        {
            terrainGen.Reset();
        }
    }

}
