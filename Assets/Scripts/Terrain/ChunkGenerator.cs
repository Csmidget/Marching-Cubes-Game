using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkGenerator
{
    private IMeshGenerator meshGenerator;
    private NoiseGenerator noiseGenerator;

    private Queue<TerrainChunk> priorityChunkQueue;
    private Queue<TerrainChunk> chunkQueue;
    private Queue<TerrainChunk> outdatedChunks;

    private int chunksPerFrame;


    public ChunkGenerator(TerrainSettings _settings)
    {
        var innerSettings = _settings.Get();

        meshGenerator = MeshGeneratorFactory.Create(_settings.renderType, innerSettings.clipPercent);
        noiseGenerator = new NoiseGenerator(innerSettings.seed, innerSettings.frequency, innerSettings.offset);
        chunksPerFrame = innerSettings.chunksPerFrame;

        priorityChunkQueue = new Queue<TerrainChunk>();
        chunkQueue = new Queue<TerrainChunk>();
        outdatedChunks = new Queue<TerrainChunk>();
    }

    public void EnqueueChunk(TerrainChunk _chunk, bool _highPriority = false)
    {
        if (_highPriority)
            priorityChunkQueue.Enqueue(_chunk);
        else
            chunkQueue.Enqueue(_chunk);
    }

    public void EnqueueOutdatedChunk(TerrainChunk _chunk)
    {
        outdatedChunks.Enqueue(_chunk);
    }

    // Update is called once per frame
    public void Update()
    {
        for (int i = 0; i < chunksPerFrame; i++)
        {
            TerrainChunk chunk;

            if (priorityChunkQueue.Count > 0)
            {
                chunk = priorityChunkQueue.Dequeue();
                chunk.SetMap(noiseGenerator.EvaluateChunk(chunk.rawPosition, chunk.rawDims));
                meshGenerator.EnqueueChunk(chunk, true);
            }
            else if (chunkQueue.Count > 0)
            {
                chunk = chunkQueue.Dequeue();
                chunk.SetMap(noiseGenerator.EvaluateChunk(chunk.rawPosition, chunk.rawDims));
                meshGenerator.EnqueueChunk(chunk);
            }
            else
                break;           
        }

        meshGenerator.Update();

        while(outdatedChunks.Count > 0)
        {
            meshGenerator.GenerateChunkMesh(outdatedChunks.Dequeue());
        }

    }

    public void GenerateChunkImmediately(TerrainChunk _chunk)
    {
        _chunk.SetMap(noiseGenerator.EvaluateChunk(_chunk.rawPosition, _chunk.rawDims));
        meshGenerator.GenerateChunkMesh(_chunk);
    }

    public void GenerateAllChunks()
    {
        for (int i = 0; i < priorityChunkQueue.Count; i++)
        {
            GenerateChunkImmediately(priorityChunkQueue.Dequeue());
        }

        for (int i = 0; i < chunkQueue.Count; i++)
        {
            GenerateChunkImmediately(priorityChunkQueue.Dequeue());
        }
    }

    public void Dispose()
    {
        meshGenerator.Dispose();
        noiseGenerator.Dispose();
    }

    ~ChunkGenerator()
    {
        Dispose();
    }
}
