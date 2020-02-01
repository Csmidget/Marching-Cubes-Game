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

        var settings = terrainGen.settings.Get();
        if (settings.maxRenderDistance < settings.minRenderDistance)
            settings.maxRenderDistance = settings.minRenderDistance;

        if (DrawDefaultInspector())
        {
            if (terrainGen.settings.autoUpdate)
            {
                terrainGen.Clear();
                terrainGen.Init();
            }
        }

        if (GUILayout.Button("Generate"))
        {
            terrainGen.Clear();
            terrainGen.Init();
        }
        if (GUILayout.Button("Clear"))
        {
            terrainGen.Clear();
        }
    }

}
