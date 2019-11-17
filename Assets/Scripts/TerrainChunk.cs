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
    private int rawWidth;
    private int rawHeight;
    private int rawDepth;
    public float clipValue;

    IMeshData meshData;
    MeshFilter meshFilter;
    MeshCollider meshCollider;


    public TerrainChunk(Vector3 _position, int _width, int _height, int _depth,float _clipValue, Transform _parent, GameObject _chunkPrefab)
    {
        chunkPosition = _position;
        width = _width;
        height = _height;
        depth = _depth;
        rawWidth = _width + 1;
        rawHeight = _height + 1;
        rawDepth = _depth + 1;
        
        terrainMap = new float[rawWidth * rawHeight * rawDepth];

        clipValue = _clipValue;
        rawPosition = new Vector3(chunkPosition.x * width, chunkPosition.y * height, chunkPosition.z * depth);

        chunkObject = GameObject.Instantiate(_chunkPrefab, _parent);
        chunkObject.name = "Chunk " + chunkPosition;
        chunkObject.transform.position = rawPosition;
        meshFilter = chunkObject.GetComponent<MeshFilter>();
        meshCollider = chunkObject.GetComponent<MeshCollider>();
    }

    /// <summary>
    /// NON RANGE CHECKED get from terrainMap. Less efficient than direct access.
    /// </summary>
    public float this[int _x, int _y, int _z]
    {
        get { return terrainMap[_x + rawWidth * _y + rawHeight * rawWidth * _z]; }
        set { terrainMap[_x + rawWidth * _y + rawHeight * rawWidth * _z] = value; }
    }

    public void SetMesh(IMeshData _meshData)
    {
        meshData = _meshData;
        meshFilter.sharedMesh = meshData.CreateMesh();
        meshCollider.sharedMesh = meshData.CreateMesh();
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