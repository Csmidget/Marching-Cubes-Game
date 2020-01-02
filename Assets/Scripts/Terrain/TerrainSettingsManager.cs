using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RenderType { Voxel, MarchingCubes, ComputeShader }

[System.Serializable]
public class TerrainSettingsManager
{
    public RenderType renderType;

    [SerializeField]
    [ConditionalHide("renderType", 0)]
    private TerrainSettings voxelTerrainSettings;
    [SerializeField]
    [ConditionalHide("renderType", 1)]
    private TerrainSettings marchingCubesTerrainSettings;
    [SerializeField]
    [ConditionalHide("renderType", 2)]
    private ComputeShaderTerrainSettings computeShaderTerrainSettings;

    public TerrainSettings Get()
    {
        switch (renderType)
        {
            case RenderType.Voxel:
                return voxelTerrainSettings;
            case RenderType.MarchingCubes:
                return marchingCubesTerrainSettings;
            case RenderType.ComputeShader:
                return computeShaderTerrainSettings;
        }

        throw new System.Exception("Error: Unable to generate settings for type: " + renderType);
    }

    [System.Serializable]
    public class TerrainSettings
    {
        public bool multiThreaded = true;

        //In settings so it can be passed to mesh generators.
        public readonly int chunkDims = 16;

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
    public class ComputeShaderTerrainSettings : TerrainSettings
    {
        public ComputeShader shader;
    }
}
