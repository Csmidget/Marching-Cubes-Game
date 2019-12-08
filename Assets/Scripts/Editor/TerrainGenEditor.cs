using System.Collections;
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
            {
                terrainGen.Reset();
                terrainGen.GenerateMap();
            }
        }

        if (GUILayout.Button("Generate"))
        {
            terrainGen.Reset();
            terrainGen.GenerateMap();
        }
        if (GUILayout.Button("Clear"))
        {
            terrainGen.Reset();
        }
    }

}
