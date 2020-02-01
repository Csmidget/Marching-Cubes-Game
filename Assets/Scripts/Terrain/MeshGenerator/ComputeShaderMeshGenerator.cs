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
    TerrainSettings.ComputeShaderTerrainSettings settings;

    public ComputeShaderMeshGenerator(float _clipPercent) : base(_clipPercent)
    {
        shader = settings.shader;
        shader.SetFloat("_clipValue", _clipPercent);
        kernel = shader.FindKernel("CSMain");

        int dims = settings.chunkDims;
        int rawDims = dims + 1;

        terrainValues = new ComputeBuffer(rawDims * rawDims * rawDims, sizeof(float));
        resultTriangles = new ComputeBuffer(dims * dims * dims * 5, 36, ComputeBufferType.Append);
        triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);

        shader.SetBuffer(kernel, "_terrainValues", terrainValues);
        shader.SetBuffer(kernel, "_resultTriangles", resultTriangles);
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

        Triangle[] triData = new Triangle[resultTriangles.count];

        Vector3[] vertices = new Vector3[triCount*3];
        int[] triangles = new int[triCount*3];

        resultTriangles.GetData(triData);
        
        for (int i = 0; i < triCount; i++)
        {
           int startPos = i * 3;
           vertices[startPos] = triData[i].a;
           vertices[startPos+1] = triData[i].b;
           vertices[startPos+2] = triData[i].c;

           triangles[startPos] = startPos ;
           triangles[startPos+1] = startPos + 1;
           triangles[startPos+2] = startPos + 2;
        }

        meshData.vertices = vertices;
        meshData.triangles = triangles;

        _chunk.SetMeshData(meshData);
        _chunk.ApplyMesh();
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

