using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RenderType { Voxel, MarchingCubes, ComputeShader }

public abstract class IMeshGenerator
{
    public abstract void GenerateChunkMesh(in TerrainChunk _chunkData);

    public abstract void Init(ComputeShader _shader, float clipValue, int dims);
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
