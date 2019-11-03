using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class VoxelMeshData : IMeshData
{
    public List<Vector3> normals;

    int verticesIndex;

    public VoxelMeshData() :base()
    { 
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

    public override Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();

        return mesh;
    }
}