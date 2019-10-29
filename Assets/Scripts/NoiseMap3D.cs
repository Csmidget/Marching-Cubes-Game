using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseMap3D
{
    Noise noise;
    float frequency;

    public NoiseMap3D(int _seed, float _frequency)
    {
        noise = new Noise(_seed);
        frequency = _frequency;
    }

    public float Evaluate(Vector3 _point)
    {
        return SampleNoise3D(_point);
    }
    
    private float SampleNoise3D (Vector3 _pos)
    {
        float noiseValue = noise.Evaluate(_pos*frequency);

        noiseValue = (noiseValue + 1) * 0.5f;

        return noiseValue;
    }
    

}
