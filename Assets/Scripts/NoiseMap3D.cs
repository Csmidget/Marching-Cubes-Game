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
        float noiseValue = noise.Evaluate(_point * frequency);

        noiseValue = (noiseValue + 1) * 0.5f;

        return noiseValue;
    }  
}
