using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelMeshGenerator : IMeshGenerator
{

    public override void GenerateChunkMesh(in TerrainChunk _chunk)
    {
       MeshData meshData = new MeshData(false);
        int verticesIndex = 0;

        for (int x = 0; x < _chunk.dims; x++)
        {
            for (int y = 0; y < _chunk.dims; y++)
            {
                for (int z = 0; z < _chunk.dims; z++)
                {                
                    if (_chunk[x, y, z] <= clipValue)
                        continue;

                    Vector3 pos = new Vector3(x, y, z);                  

                    if (x + 1 >= _chunk.dims || _chunk[x+1,y,z] <= clipValue)
                        AddSquare(meshData, pos, new Vector3(1,0,0), ref verticesIndex);
                    if (x - 1 < 0 || _chunk[x - 1, y, z] <= clipValue)
                        AddSquare(meshData, pos, new Vector3(-1, 0, 0), ref verticesIndex);

                    if (y + 1 >= _chunk.dims || _chunk[x, y + 1, z] <= clipValue)
                        AddSquare(meshData, pos, new Vector3(0, 1, 0), ref verticesIndex);
                    if (y - 1 < 0 || _chunk[x, y - 1, z] <= clipValue)
                        AddSquare(meshData, pos, new Vector3(0, -1, 0), ref verticesIndex);

                    if (z + 1 >= _chunk.dims || _chunk[x, y, z + 1] <= clipValue)
                        AddSquare(meshData, pos, new Vector3(0, 0, 1), ref verticesIndex);
                    if (z - 1 < 0 || _chunk[x, y, z - 1] <= clipValue)
                        AddSquare(meshData, pos, new Vector3(0, 0, -1), ref verticesIndex);
                }               
            }
        }

        _chunk.SetMeshData(meshData);
    }

    public void AddSquare(MeshData _meshData, Vector3 _inner, Vector3 _direction, ref int _verticesIndex)
    {
   //     if (Mathf.Abs(_direction.x) == 1.0f)
   //     {
   //         float x = _inner.x + _direction.x * 0.5f;
   //         _meshData.vertices.Add(new Vector3(x, _inner.y + 0.5f, _inner.z + 0.5f));
   //         _meshData.vertices.Add(new Vector3(x, _inner.y - 0.5f, _inner.z + 0.5f));
   //         _meshData.vertices.Add(new Vector3(x, _inner.y + 0.5f, _inner.z - 0.5f));
   //         _meshData.vertices.Add(new Vector3(x, _inner.y - 0.5f, _inner.z - 0.5f));
   //     }
   //     else if (Mathf.Abs(_direction.y) == 1.0f)
   //     {
   //         float y = _inner.y + _direction.y * 0.5f;
   //         _meshData.vertices.Add(new Vector3(_inner.x + 0.5f, y, _inner.z + 0.5f));
   //         _meshData.vertices.Add(new Vector3(_inner.x - 0.5f, y, _inner.z + 0.5f));
   //         _meshData.vertices.Add(new Vector3(_inner.x + 0.5f, y, _inner.z - 0.5f));
   //         _meshData.vertices.Add(new Vector3(_inner.x - 0.5f, y, _inner.z - 0.5f));
   //     }
   //     else if (Mathf.Abs(_direction.z) == 1.0f)
   //     {
   //         float z = _inner.z + _direction.z * 0.5f;
   //         _meshData.vertices.Add(new Vector3(_inner.x + 0.5f, _inner.y + 0.5f, z));
   //         _meshData.vertices.Add(new Vector3(_inner.x - 0.5f, _inner.y + 0.5f, z));
   //         _meshData.vertices.Add(new Vector3(_inner.x + 0.5f, _inner.y - 0.5f, z));
   //         _meshData.vertices.Add(new Vector3(_inner.x - 0.5f, _inner.y - 0.5f, z));
   //     }
   //
   //     _meshData.normals.Add(_direction);
   //     _meshData.normals.Add(_direction);
   //     _meshData.normals.Add(_direction);
   //     _meshData.normals.Add(_direction);
   //
   //     if (_direction.x - _direction.y + _direction.z > 0)
   //     {
   //         _meshData.triangles.Add(_verticesIndex);
   //         _meshData.triangles.Add(_verticesIndex + 1);
   //         _meshData.triangles.Add(_verticesIndex + 2);
   //
   //         _meshData.triangles.Add(_verticesIndex + 1);
   //         _meshData.triangles.Add(_verticesIndex + 3);
   //         _meshData.triangles.Add(_verticesIndex + 2);
   //     }
   //     else
   //     {
   //         _meshData.triangles.Add(_verticesIndex);
   //         _meshData.triangles.Add(_verticesIndex + 2);
   //         _meshData.triangles.Add(_verticesIndex + 1);
   //
   //         _meshData.triangles.Add(_verticesIndex + 3);
   //         _meshData.triangles.Add(_verticesIndex + 1);
   //         _meshData.triangles.Add(_verticesIndex + 2);
   //     }
   //
   //     _verticesIndex += 4;
    }
}


