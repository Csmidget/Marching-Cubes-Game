using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RenderType {Basic, ComputeShader, JobSystemFull, JobSystemPartial }

[System.Serializable]
public class ProceduralTerrainSettings 
{
    public bool autoUpdate = false;

    public RenderType renderType;

    [SerializeField]
    [ConditionalHide("renderType", 0)]
    private TerrainSettings basicTerrainSettings = null;
    [SerializeField]
    [ConditionalHide("renderType", 1)]
    private ComputeShaderTerrainSettings computeShaderTerrainSettings = null;
    [SerializeField]
    [ConditionalHide("renderType", 2)]
    private TerrainSettings jobSystemFullSettings = null;
    [SerializeField]
    [ConditionalHide("renderType", 3)]
    private TerrainSettings jobSystemPartialSettings = null;

    public TerrainSettings Get()
    {
        switch (renderType)
        {
            case RenderType.Basic:
                return basicTerrainSettings;
            case RenderType.ComputeShader:
                return computeShaderTerrainSettings;
            case RenderType.JobSystemFull:
                return jobSystemFullSettings;
            case RenderType.JobSystemPartial:
                return jobSystemPartialSettings;
        }

        throw new System.Exception("Error: Unable to generate settings for type: " + renderType);
    }
}

[System.Serializable]
public class TerrainSettings
{
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
    [Range(1, 10)]
    public int maxRenderDistance = 6; //

    [Range(1, 10)]
    public int chunksPerFrame = 1;

}

[System.Serializable]
public class ComputeShaderTerrainSettings : TerrainSettings
{
    public ComputeShader shader;
}
