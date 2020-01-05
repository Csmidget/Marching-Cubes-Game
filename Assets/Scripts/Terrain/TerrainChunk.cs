using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores data for a chunk, also provides helper functions for easier data access.
/// Most variables are public for efficiency purposes. Only trusted classes should be able to access this.
/// </summary>
public class TerrainChunk
{
    GameObject chunkObject;

    public float[] terrainMap;
    public Vector3 position;
    public Vector3 rawPosition;
    public int dims;
    private int rawDims;
    public bool MeshOutdated { get; private set; }

    MeshData meshData;
    MeshFilter meshFilter;
    MeshCollider meshCollider;


    public TerrainChunk(Vector3 _position, int _dims, Transform _parent, GameObject _chunkPrefab)
    {
        position = _position;
        dims = _dims;
        rawDims = dims + 1;
        meshData = null;
        
        terrainMap = new float[rawDims * rawDims * rawDims];

        rawPosition = new Vector3(position.x * dims, position.y * dims, position.z * dims);

        chunkObject = GameObject.Instantiate(_chunkPrefab, _parent);
        chunkObject.name = "Chunk " + position;
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
        set { terrainMap[_x + rawDims * _y + rawDims * rawDims * _z] = value; MeshOutdated = true; }
    }

    public float this[Vector3Int _xyz]
    {
        get { return terrainMap[_xyz.x + rawDims * _xyz.y + rawDims * rawDims * _xyz.z]; }
        set { terrainMap[_xyz.x + rawDims * _xyz.y + rawDims * rawDims * _xyz.z] = value; MeshOutdated = true; }
    }
    public bool HasMesh
    {
        get { return meshData != null; }
    }

    public bool IsActive
    {
        get { return chunkObject.activeSelf; }
    }

    public void SetMeshData(MeshData _meshData)
    {
        meshData = _meshData;
    }

    public void ApplyMesh()
    {
        meshFilter.sharedMesh = meshData.CreateMesh();
        meshCollider.sharedMesh = meshData.CreateMesh();
        MeshOutdated = false;
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