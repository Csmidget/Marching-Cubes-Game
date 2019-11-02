using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator
{
   public MeshData GenerateChunkMesh(ChunkData _chunk)
    {
        MeshData meshData = new MeshData();

        for (int x = 0; x < _chunk.width; x++)
        {
            for (int y = 0; y < _chunk.height; y++)
            {
                for (int z = 0; z < _chunk.width; z++)
                {
                    if (_chunk[x, y, z] == 0.0f)
                        continue;

                    if (x + 1 >= _chunk.width || _chunk[x+1,y,z] == 0)
                        meshData.AddSquare(new Vector3(x, y, z), new Vector3(1,0,0));
                    if (x - 1 < 0 || _chunk[x - 1, y, z] == 0)
                        meshData.AddSquare(new Vector3(x, y, z), new Vector3(-1, 0, 0));

                    if (y + 1 >= _chunk.height || _chunk[x, y + 1, z] == 0)
                        meshData.AddSquare(new Vector3(x, y, z), new Vector3(0, 1, 0));
                    if (y - 1 < 0 || _chunk[x, y - 1, z] == 0)
                        meshData.AddSquare(new Vector3(x, y, z), new Vector3(0, -1, 0));

                    if (z + 1 >= _chunk.width || _chunk[x, y, z + 1] == 0)
                        meshData.AddSquare(new Vector3(x, y, z), new Vector3(0, 0, 1));
                    if (z - 1 < 0 || _chunk[x, y, z - 1] == 0)
                        meshData.AddSquare(new Vector3(x, y, z), new Vector3(0, 0, -1));
                }               
            }
        }
        return meshData;
    }
}

public class MeshData
{
    public List<Vector3> vertices;
    public List<int> triangles;
    public List<Vector2> uvs;
    public List<Vector3> normals;

    int verticesIndex;

    public MeshData()
    {
        vertices = new List<Vector3>();
        uvs = new List<Vector2>();
        triangles = new List<int>();
        normals = new List<Vector3>();
    }

    public void AddSquare(Vector3 _inner, Vector3 _direction)
    {
        if (Mathf.Abs(_direction.x) == 1.0f)
        {
            float x = _inner.x + _direction.x * 0.5f;
            vertices.Add(new Vector3(x, _inner.y + 0.5f, _inner.z + 0.5f));
            vertices.Add(new Vector3(x, _inner.y - 0.5f, _inner.z + 0.5f));
            vertices.Add(new Vector3(x, _inner.y + 0.5f, _inner.z - 0.5f));
            vertices.Add(new Vector3(x, _inner.y - 0.5f, _inner.z - 0.5f));
        }
        else if (Mathf.Abs(_direction.y) == 1.0f)
        {
            float y = _inner.y + _direction.y * 0.5f;
            vertices.Add(new Vector3(_inner.x + 0.5f, y, _inner.z + 0.5f));
            vertices.Add(new Vector3(_inner.x - 0.5f, y, _inner.z + 0.5f));
            vertices.Add(new Vector3(_inner.x + 0.5f, y, _inner.z - 0.5f));
            vertices.Add(new Vector3(_inner.x - 0.5f, y, _inner.z - 0.5f));
        }
        else if (Mathf.Abs(_direction.z) == 1.0f)
        {
            float z = _inner.z + _direction.z * 0.5f;
            vertices.Add(new Vector3(_inner.x + 0.5f, _inner.y + 0.5f, z));
            vertices.Add(new Vector3(_inner.x - 0.5f, _inner.y + 0.5f, z));
            vertices.Add(new Vector3(_inner.x + 0.5f, _inner.y - 0.5f, z));
            vertices.Add(new Vector3(_inner.x - 0.5f, _inner.y - 0.5f, z));
        }

        normals.Add(_direction);
        normals.Add(_direction);
        normals.Add(_direction);
        normals.Add(_direction);

        if (_direction.x - _direction.y + _direction.z > 0)
        {
            triangles.Add(verticesIndex);
            triangles.Add(verticesIndex + 1);
            triangles.Add(verticesIndex + 2);

            triangles.Add(verticesIndex + 1);
            triangles.Add(verticesIndex + 3);
            triangles.Add(verticesIndex + 2);
        }
        else
        {
            triangles.Add(verticesIndex);
            triangles.Add(verticesIndex + 2);
            triangles.Add(verticesIndex + 1);

            triangles.Add(verticesIndex + 3);
            triangles.Add(verticesIndex + 1);
            triangles.Add(verticesIndex + 2);
        }

        verticesIndex += 4;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();

        return mesh;
    }
}
