using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainManager))]
public class TerrainGenEditor : Editor
{

    public override void OnInspectorGUI()
    {
        TerrainManager terrainGen = (TerrainManager)target;

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
