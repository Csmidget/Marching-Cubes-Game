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

    private MeshData meshData;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    public readonly Vector3 position;
    public readonly Vector3 rawPosition;

    public readonly int dims;
    public readonly int rawDims;

    public readonly int size;
    public readonly int rawSize;

    public float[] TerrainMap { get; private set; }
    public bool MeshOutdated { get; private set; }
    public bool HasMesh { get { return meshData != null; } }

    public TerrainChunk(Vector3 _position, int _dims, Transform _parent, GameObject _chunkPrefab)
    {
        position = _position;
        dims = _dims;
        rawDims = dims + 1;

        size = dims * dims * dims;
        rawSize = rawDims * rawDims * rawDims;

        meshData = null;
        MeshOutdated = true;
        TerrainMap = new float[rawDims * rawDims * rawDims];

        rawPosition = new Vector3(position.x * dims, position.y * dims, position.z * dims);

        chunkObject = GameObject.Instantiate(_chunkPrefab, _parent);
        chunkObject.name = "Chunk " + position;
        chunkObject.transform.position = rawPosition;
        meshFilter = chunkObject.GetComponent<MeshFilter>();
        meshCollider = chunkObject.GetComponent<MeshCollider>();
    }

    /// <summary>
    /// Replaces the entire terrainMap with a new map.
    /// </summary>
    /// <param name="_terrainMap"></param>
    public void SetMap(float[] _terrainMap)
    {
        TerrainMap = _terrainMap;
        MeshOutdated = true;
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
        if (Application.isPlaying)
        {
            Mesh.Destroy(meshFilter.sharedMesh);
            Mesh.Destroy(meshCollider.sharedMesh);
        }
        else
        {
            Mesh.DestroyImmediate(meshFilter.sharedMesh);
            Mesh.DestroyImmediate(meshCollider.sharedMesh);
        }

        meshFilter.sharedMesh = meshData.CreateMesh(false);
        meshCollider.sharedMesh = meshData.CreateMesh(true);
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
    /// Non range checked direct get/set from/to terrainMap.
    /// </summary>
    public float this[int _xyz]
    {
        get { return TerrainMap[_xyz]; }
        set { TerrainMap[_xyz] = value; MeshOutdated = true; }
    }

    /// <summary>
    /// Non range checked get/set from terrainMap. Less efficient than direct get/set.
    /// </summary>
    public float this[int _x, int _y, int _z]
    {
        get { return TerrainMap[_x + rawDims * _y + rawDims * rawDims * _z]; }
        set { TerrainMap[_x + rawDims * _y + rawDims * rawDims * _z] = value; MeshOutdated = true; }
    }

    /// <summary>
    /// Range checked get from terrainMap.
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

    /// <summary>
    /// Range checked set to terrainMap.
    /// </summary>
    public void Set(int _x, int _y, int _z, int _value)
    {
        if (_x < dims)
            throw new ArgumentOutOfRangeException("_x", "_x: " + _x + ". value entered greater than width");
        if (_y < dims)
            throw new ArgumentOutOfRangeException("_y", "_y: " + _y + ". value entered greater than height");
        if (_z < dims)
            throw new ArgumentOutOfRangeException("_z", "_z: " + _z + ". value entered greater than depth");

        MeshOutdated = true;

        this[_x, _y, _z] = _value;
    }
}