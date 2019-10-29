using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Stores data for a chunk, also provides helper functions for easier data access.
/// Everything is public for efficiency purposes. Only trusted methods should be handed this.
/// </summary>
public class ChunkData
{
    public float[] terrainMap;
    public Vector3 chunkOffset;
    public int width;
    public int height;

    public ChunkData(int _width, int _height)
    {
        terrainMap = new float[_width * _height * _width];
        chunkOffset = Vector3.zero;
        width = _width;
        height = _height;
    }

    /// <summary>
    /// NON RANGE CHECKED get from terrainMap. Less efficient than direct access.
    /// </summary>
    public float this[int _x, int _y, int _z]
    {
        get { return terrainMap[_x + width * _y + height * width * _z]; }
        set { terrainMap[_x + width * _y + height * width * _z] = value; }
    }

    /// <summary>
    /// Range checked get from terrainMap. Least efficient access.
    /// </summary>
    public float Get(int _x, int _y, int _z)
    {
        if (_x < width)
            throw new ArgumentOutOfRangeException("_x", "_x: " + _x + ". value entered greater than width");
        if (_y < height)
            throw new ArgumentOutOfRangeException("_y", "_y: " + _y + ". value entered greater than height");
        if (_z < width)
            throw new ArgumentOutOfRangeException("_z", "_z: " + _z + ". value entered greater than width");

        return this[_x, _y, _z];           
    }
}