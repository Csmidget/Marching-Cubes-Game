using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class IMeshGenerator
{
    protected float clipValue;
    protected bool supportsMultiThreading;

    public bool SupportsMultiThreading
    {
        get
        {
            return supportsMultiThreading;
        }
    }

    public abstract void GenerateChunkMesh(in TerrainChunk _chunkData);
    public virtual void Init(TerrainSettings.TerrainInnerSettings _settings) 
    { 
        clipValue = _settings.clipPercent;
        supportsMultiThreading = true;
    }

    public virtual void Dispose() {; }
}

public static class MeshGeneratorFactory
{
    public static IMeshGenerator Create(RenderType _renderType)
    {
        switch (_renderType)
        {
            case RenderType.Voxel_LEGACY:
                return new VoxelMeshGenerator();
            case RenderType.MarchingCubes:
                return new MarchingCubesMeshGenerator();
            case RenderType.ComputeShader:
                return new ComputeShaderMeshGenerator();
            case RenderType.MarchingCubesParallelJob:
                return new MCubesParallelJobMeshGenerator();
            case RenderType.MarchingCubeIndividualJob:
                return new MCubesIndividualJobMeshGenerator();
        }

        throw new System.Exception("Unable to create MeshGenerator for RenderType: " + _renderType.ToString());
    }
}
