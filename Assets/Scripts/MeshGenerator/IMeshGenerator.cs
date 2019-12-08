using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class IMeshGenerator
{
    protected float clipValue;

    public abstract void GenerateChunkMesh(in TerrainChunk _chunkData);
    public virtual void Init(TerrainSettingsManager.TerrainSettings _settings) { clipValue = _settings.clipPercent; }

    public virtual void Dispose() {; }
}

public static class MeshGeneratorFactory
{
    public static IMeshGenerator Create(RenderType _renderType)
    {
        switch (_renderType)
        {
            case RenderType.Voxel:
                return new VoxelMeshGenerator();
            case RenderType.MarchingCubes:
                return new MarchingCubesMeshGenerator();
            case RenderType.ComputeShader:
                return new ComputeShaderMeshGenerator();
        }
        return null;
    }
}
