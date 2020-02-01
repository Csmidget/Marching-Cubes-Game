using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class NoiseGenerator
{
    ChunkNoiseJob job;

    public NoiseGenerator(int _seed, float _frequency, Vector3 _offset)
    {
        job = new ChunkNoiseJob();
        job.frequency = _frequency;
        job.offset = _offset;
        job.noise = new Noise(_seed);
    }

    public void Dispose()
    {
        job.noise.Dispose();
    }

    public float[] EvaluateChunk(Vector3 _chunkRawPos, int _chunkRawDims)
    {
        job.noiseValues = new NativeArray<float>(_chunkRawDims * _chunkRawDims * _chunkRawDims, Allocator.Persistent);
        job.dimensions = _chunkRawDims;
        job._chunkPos = _chunkRawPos;

        var handle = job.Schedule();
        handle.Complete();

        float[] noiseValues = new float[_chunkRawDims * _chunkRawDims * _chunkRawDims];
        noiseValues = job.noiseValues.ToArray();

        job.noiseValues.Dispose();

        return noiseValues;
    }
}

[BurstCompile]
public struct ChunkNoiseJob : IJob
{
    public Noise noise;
    public NativeArray<float> noiseValues;
    public Vector3 _chunkPos;
    public int dimensions;
    public float frequency;
    public Vector3 offset;

    public void Execute()
    {
        int i = 0;
        for (int z = 0; z < dimensions; z++)
        {
            for (int y = 0; y < dimensions; y++)
            {
                for (int x = 0; x < dimensions; x++)
                {
                    noiseValues[i] = Evaluate(new Vector3(x, y, z) + _chunkPos);
                    i++;
                }
            }
        }
    }

    public float Evaluate(Vector3 _point)
    {
        float noiseValue = noise.Evaluate((_point + offset) * frequency);

        noiseValue = (noiseValue + 1) * 0.5f;

        return noiseValue;
    }
}

