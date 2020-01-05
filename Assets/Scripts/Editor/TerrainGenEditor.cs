using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ProceduralTerrain))]
public class TerrainGenEditor : Editor
{

    public override void OnInspectorGUI()
    {
        ProceduralTerrain terrainGen = (ProceduralTerrain)target;

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
