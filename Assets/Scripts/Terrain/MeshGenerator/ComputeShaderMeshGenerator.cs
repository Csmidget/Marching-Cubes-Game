using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

struct Triangle
{
    public Vector3 a;
    public Vector3 b;
    public Vector3 c;
}

class ComputeShaderMeshGenerator : IMeshGenerator
{
    ComputeShader shader;
    Int32 kernel;

    ComputeBuffer terrainValues;
    ComputeBuffer resultTriangles;
    ComputeBuffer triCountBuffer;
    TerrainSettingsManager.ComputeShaderTerrainSettings settings;

    public override void Init(TerrainSettingsManager.TerrainSettings _settings)
    {
        base.Init(_settings);

        //Cannot multithread compute shader calls.
        supportsMultiThreading = false;

        settings = (TerrainSettingsManager.ComputeShaderTerrainSettings)_settings;

        shader = settings.shader;
        shader.SetFloat("_clipValue", clipValue);
        kernel = shader.FindKernel("CSMain");

        int dims = settings.chunkDims;
        int rawDims = dims + 1;

        terrainValues = new ComputeBuffer(rawDims*rawDims*rawDims, sizeof(float));
        resultTriangles = new ComputeBuffer(dims*dims*dims * 5, 36, ComputeBufferType.Append);
        triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);

        shader.SetBuffer(kernel, "_terrainValues", terrainValues);
        shader.SetBuffer(kernel, "_resultTriangles", resultTriangles);
        shader.SetInt("dimLen", dims);    
    }

    public override void GenerateChunkMesh(in TerrainChunk _chunk)
    {
        MeshData meshData = new MeshData();

        terrainValues.SetData(_chunk.terrainMap);

        resultTriangles.SetCounterValue(0);
        
        shader.Dispatch(kernel, _chunk.dims / 8, _chunk.dims / 8, _chunk.dims / 8);
        
        //Get number of triangles generated.
        ComputeBuffer.CopyCount(resultTriangles, triCountBuffer, 0);
        int[] triCountArr = { 0 };
        triCountBuffer.GetData(triCountArr);
        int triCount = triCountArr[0];

        Triangle[] triangles = new Triangle[resultTriangles.count];

        resultTriangles.GetData(triangles);

        ProcessTriangles(meshData, triangles, triCount);

        meshData.CreateMesh();

        _chunk.SetMeshData(meshData);
    }

    public void ProcessTriangles(MeshData _meshData, Triangle[] _triangles, int _triCount)
    {

        for (int i = 0; i < _triCount; i++)
        {
            _meshData.vertices.Add(_triangles[i].a);
            _meshData.vertices.Add(_triangles[i].b);
            _meshData.vertices.Add(_triangles[i].c);

            _meshData.triangles.Add(i * 3);
            _meshData.triangles.Add(i * 3 + 1);
            _meshData.triangles.Add(i * 3 + 2);
        }
    }

    public override void Dispose()
    {
        terrainValues.Dispose();
        triCountBuffer.Dispose();
        resultTriangles.Dispose();
    }

    ~ComputeShaderMeshGenerator()
    {
        Dispose();
    }
}

