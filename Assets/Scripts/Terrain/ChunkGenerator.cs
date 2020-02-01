using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkGenerator
{
    private IMeshGenerator meshGenerator;
    private NoiseGenerator noiseGenerator;

    //Outdated chunks will have their mesh regenerated immediately.
    private Queue<TerrainChunk> outdatedChunks;

    //Priority chunks are chunks that sit within the minimum render distance. They will be generated first.
    private Queue<TerrainChunk> priorityChunkQueue;
    private Queue<TerrainChunk> chunkQueue;

    //The number of chunks to generate per frame.
    private int chunksPerFrame;


    public ChunkGenerator(ProceduralTerrainSettings _typeSettings)
    {
        var settings = _typeSettings.Get();

        meshGenerator = MeshGeneratorFactory.Create(_typeSettings);
        noiseGenerator = new NoiseGenerator(settings);
        chunksPerFrame = settings.chunksPerFrame;

        priorityChunkQueue = new Queue<TerrainChunk>();
        chunkQueue = new Queue<TerrainChunk>();
        outdatedChunks = new Queue<TerrainChunk>();
    }

    public void Enqueue(TerrainChunk _chunk, bool _highPriority = false)
    {
        if (_highPriority)
            priorityChunkQueue.Enqueue(_chunk);
        else
            chunkQueue.Enqueue(_chunk);
    }

    public void EnqueueOutdated(TerrainChunk _chunk)
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
                meshGenerator.Enqueue(chunk, true);
            }
            else if (chunkQueue.Count > 0)
            {
                chunk = chunkQueue.Dequeue();
                chunk.SetMap(noiseGenerator.EvaluateChunk(chunk.rawPosition, chunk.rawDims));
                meshGenerator.Enqueue(chunk);
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

    public void GenerateChunksImmediately()
    {
        while (priorityChunkQueue.Count > 0)
        {
            TerrainChunk chunk = priorityChunkQueue.Dequeue();
            chunk.SetMap(noiseGenerator.EvaluateChunk(chunk.rawPosition, chunk.rawDims));
            meshGenerator.Enqueue(chunk, true);
        }

        while (chunkQueue.Count > 0)
        {
            TerrainChunk chunk = chunkQueue.Dequeue();
            chunk.SetMap(noiseGenerator.EvaluateChunk(chunk.rawPosition, chunk.rawDims));
            meshGenerator.Enqueue(chunk);
        }

        meshGenerator.GenerateChunksImmediately();

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
