using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class IMeshGenerator
{
    protected Queue<TerrainChunk> priorityChunkQueue;
    protected Queue<TerrainChunk> chunkQueue;

    protected float clipValue;

    public IMeshGenerator(TerrainSettings _settings)
    {
        clipValue = _settings.clipPercent;
        priorityChunkQueue = new Queue<TerrainChunk>();
        chunkQueue = new Queue<TerrainChunk>();
    }

    public virtual void Update()
    {
        if (priorityChunkQueue.Count > 0)
        {
            TerrainChunk currentChunk = priorityChunkQueue.Dequeue();
            GenerateChunkMesh(currentChunk);
        }
        else if (chunkQueue.Count > 0)
        {
            TerrainChunk currentChunk = chunkQueue.Dequeue();
            GenerateChunkMesh(currentChunk);
        }
    }

    public virtual void Enqueue(TerrainChunk _chunk, bool _highPriority = false)
    {
        if (_highPriority)
            priorityChunkQueue.Enqueue(_chunk);
        else
            chunkQueue.Enqueue(_chunk);
    }

    public virtual void GenerateChunksImmediately()
    {
        while (priorityChunkQueue.Count > 0)
            GenerateChunkMesh(priorityChunkQueue.Dequeue());
        while (chunkQueue.Count > 0)
            GenerateChunkMesh(chunkQueue.Dequeue());

    }
    
    public abstract void GenerateChunkMesh(in TerrainChunk _chunkData);

    public virtual void Dispose() {; }

}

public static class MeshGeneratorFactory
{
    public static IMeshGenerator Create(ProceduralTerrainSettings _typeSettings)
    {

        TerrainSettings settings = _typeSettings.Get();

        switch (_typeSettings.renderType)
        {
            case RenderType.Basic:
                return new BasicMeshGenerator(settings);
            case RenderType.ComputeShader:
                return new ComputeShaderMeshGenerator((ComputeShaderTerrainSettings)settings);
            case RenderType.JobSystemPartial:
                return new JobSystemPartialMeshGenerator(settings);
            case RenderType.JobSystemFull:
                return new JobSystemFullMeshGenerator(settings);
        }

        throw new System.Exception("Unable to create MeshGenerator for RenderType: " + _typeSettings.renderType.ToString());
    }
}
