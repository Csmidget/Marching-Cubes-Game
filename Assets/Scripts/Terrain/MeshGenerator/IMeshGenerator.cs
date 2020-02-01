using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class IMeshGenerator
{
    protected Queue<TerrainChunk> chunksToGeneratePriority;
    protected Queue<TerrainChunk> chunksToGenerate;

    protected float clipValue;

    public IMeshGenerator(float _clipPercent)
    {
        clipValue = _clipPercent;
        chunksToGeneratePriority = new Queue<TerrainChunk>();
        chunksToGenerate = new Queue<TerrainChunk>();
    }

    public virtual void Update()
    {
        if (chunksToGeneratePriority.Count > 0)
        {
            TerrainChunk currentChunk = chunksToGeneratePriority.Dequeue();
            GenerateChunkMesh(currentChunk);
        }
        else if (chunksToGenerate.Count > 0)
        {
            TerrainChunk currentChunk = chunksToGenerate.Dequeue();
            GenerateChunkMesh(currentChunk);
        }
    }

    public virtual void EnqueueChunk(TerrainChunk _chunk, bool _highPriority = false)
    {
        if (_highPriority)
            chunksToGeneratePriority.Enqueue(_chunk);
        else
            chunksToGenerate.Enqueue(_chunk);
    }
    
    public abstract void GenerateChunkMesh(in TerrainChunk _chunkData);

    public virtual void Dispose() {; }

}

public static class MeshGeneratorFactory
{
    public static IMeshGenerator Create(RenderType _renderType, float _clipPercent)
    {
        switch (_renderType)
        {
            case RenderType.MarchingCubes:
                return new MarchingCubesMeshGenerator(_clipPercent);
            case RenderType.ComputeShader:
                return new ComputeShaderMeshGenerator(_clipPercent);
            case RenderType.MarchingCubesParallelJob:
                return new MCubesParallelJobMeshGenerator(_clipPercent);
            case RenderType.MarchingCubeIndividualJob:
                return new MCubesIndividualJobMeshGenerator(_clipPercent);
        }

        throw new System.Exception("Unable to create MeshGenerator for RenderType: " + _renderType.ToString());
    }
}
