using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RenderType { Voxel, MarchingCubes }

public abstract class IMeshGenerator
{
    public abstract void GenerateChunkMesh(TerrainChunk _chunkData);
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
        }
        return null;
    }
}
