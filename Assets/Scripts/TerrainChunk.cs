﻿using System;
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
    public int dims;
    private int rawDims;

    IMeshData meshData;
    MeshFilter meshFilter;
    MeshCollider meshCollider;


    public TerrainChunk(Vector3 _position, int _dims, Transform _parent, GameObject _chunkPrefab)
    {
        chunkPosition = _position;
        dims = _dims;
        rawDims = dims + 1;
        
        terrainMap = new float[rawDims * rawDims * rawDims];

        rawPosition = new Vector3(chunkPosition.x * dims, chunkPosition.y * dims, chunkPosition.z * dims);

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
        get { return terrainMap[_x + rawDims * _y + rawDims * rawDims * _z]; }
        set { terrainMap[_x + rawDims * _y + rawDims * rawDims * _z] = value; }
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

    public void SetActive(bool visible)
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
        if (_x < dims)
            throw new ArgumentOutOfRangeException("_x", "_x: " + _x + ". value entered greater than width");
        if (_y < dims)
            throw new ArgumentOutOfRangeException("_y", "_y: " + _y + ". value entered greater than height");
        if (_z < dims)
            throw new ArgumentOutOfRangeException("_z", "_z: " + _z + ". value entered greater than depth");

        return this[_x, _y, _z];           
    }

    public int Size()
    {
        return dims * dims * dims;
    }

    public int RawSize()
    {
        return rawDims * rawDims * rawDims;
    }
}