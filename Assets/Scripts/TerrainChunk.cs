using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Stores data for a chunk, also provides helper functions for easier data access.
/// Everything is public for efficiency purposes. Only trusted methods should be handed this.
/// </summary>
public class TerrainChunk
{
    GameObject chunkObject;

    public float[] terrainMap;
    public Vector3 chunkPosition;
    public Vector3 rawPosition;
    public int width;
    public int height;
    public int depth;

    IMeshData meshData;
    MeshFilter meshFilter;

    public TerrainChunk(Vector3 _position, int _width, int _height, int _depth, Transform _parent, GameObject _chunkPrefab)
    {
        terrainMap = new float[_width * _height * _depth];
        chunkPosition = _position;
        width = _width;
        height = _height;
        depth = _depth;
        rawPosition = new Vector3(chunkPosition.x * width, chunkPosition.y * height, chunkPosition.z * depth);

        chunkObject = GameObject.Instantiate(_chunkPrefab, _parent);
        chunkObject.name = "Chunk " + chunkPosition;
        chunkObject.transform.position = rawPosition;
        meshFilter = chunkObject.GetComponent<MeshFilter>();
    }

    /// <summary>
    /// NON RANGE CHECKED get from terrainMap. Less efficient than direct access.
    /// </summary>
    public float this[int _x, int _y, int _z]
    {
        get { return terrainMap[_x + width * _y + height * width * _z]; }
        set { terrainMap[_x + width * _y + height * width * _z] = value; }
    }

    public void SetMesh(IMeshData _meshData)
    {
        meshData = _meshData;
        meshFilter.sharedMesh = meshData.CreateMesh();
    }

    public void Destroy()
    {
        if (chunkObject != null)
            GameObject.DestroyImmediate(chunkObject);
    }

    public void SetVisible(bool visible)
    {
        chunkObject.SetActive(visible);
    }

    public bool IsVisible()
    {
        return chunkObject.activeSelf;
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
        if (_z < depth)
            throw new ArgumentOutOfRangeException("_z", "_z: " + _z + ". value entered greater than depth");

        return this[_x, _y, _z];           
    }
}