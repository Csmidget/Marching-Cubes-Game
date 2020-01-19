using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RenderType { Voxel_LEGACY, MarchingCubes, ComputeShader, MarchingCubesJob }

[System.Serializable]
public class TerrainSettings 
{
    public RenderType renderType;

    [SerializeField]
    [ConditionalHide("renderType", 0)]
    private TerrainInnerSettings voxelTerrainSettings;
    [SerializeField]
    [ConditionalHide("renderType", 1)]
    private TerrainInnerSettings marchingCubesTerrainSettings;
    [SerializeField]
    [ConditionalHide("renderType", 2)]
    private ComputeShaderTerrainSettings computeShaderTerrainSettings;
    [SerializeField]
    [ConditionalHide("renderType", 3)]
    private TerrainInnerSettings marchingCubesJobTerrainSettings;

    public TerrainInnerSettings Get()
    {
        switch (renderType)
        {
            case RenderType.Voxel_LEGACY:
                return voxelTerrainSettings;
            case RenderType.MarchingCubes:
                return marchingCubesTerrainSettings;
            case RenderType.ComputeShader:
                return computeShaderTerrainSettings;
            case RenderType.MarchingCubesJob:
                return marchingCubesJobTerrainSettings;
        }

        throw new System.Exception("Error: Unable to generate settings for type: " + renderType);
    }

    [System.Serializable]
    public class TerrainInnerSettings
    {
        public bool multiThreaded = true;

        //In settings so it can be passed to mesh generators.
        public readonly int chunkDims = 16;
        public readonly int halfDims = 8;

        //Seed for the noise generator
        public int seed;

        //The noise maps frequency (aka, Zoom)
        [Range(0, 1)]
        public float frequency;

        [Range(0, 1)]
        public float clipPercent;

        //Noise Offset
        public Vector3 offset;

        [Range(0, 6)]
        public int minRenderDistance = 1; //How many chunks away from the players current chunk will be forced to render.
        [Range(6, 10)]
        public int maxRenderDistance = 6; //

    }

    [System.Serializable]
    public class ComputeShaderTerrainSettings : TerrainInnerSettings
    {
        public ComputeShader shader;
    }
}
