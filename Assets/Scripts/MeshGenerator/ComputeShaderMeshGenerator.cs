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

    public override void Init(ComputeShader _shader, float clipValue,int dims)
    {
        shader = _shader;
        shader.SetFloat("_clipValue", clipValue);
        kernel = shader.FindKernel("CSMain");

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
        ComputeShaderMeshData meshData = new ComputeShaderMeshData();

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

        meshData.ProcessTriangles(triangles,triCount);

        meshData.CreateMesh();    

        _chunk.SetMesh(meshData);
    }

    ~ComputeShaderMeshGenerator()
    {
        terrainValues.Dispose();
        triCountBuffer.Dispose();
        resultTriangles.Dispose();
    }
}

