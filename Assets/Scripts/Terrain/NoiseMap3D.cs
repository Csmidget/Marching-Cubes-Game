using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;

public class NoiseMap3D
{
    Noise noise;
    float frequency;
    Vector3 offset;

    public NoiseMap3D(int _seed, float _frequency, Vector3 _offset)
    {
        noise = new Noise(_seed);
        offset = _offset;
        frequency = _frequency;
    }

    public float Evaluate(Vector3 _point)
    {
        float noiseValue = noise.Evaluate((_point + offset) * frequency);

        noiseValue = (noiseValue + 1) * 0.5f;

        return noiseValue;
    }    
}
