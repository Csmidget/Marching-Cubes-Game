using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshData
{
    public List<Vector3> vertices;
    public List<int> triangles;
    public List<Vector2> uvs;
    public List<Vector3> normals;
    bool calculateNormals = true;

    public MeshData()
    {
        vertices = new List<Vector3>();
        uvs = new List<Vector2>();
        triangles = new List<int>();
        normals = new List<Vector3>();
    }

    public MeshData(bool _calculateNormals) : this()
    {
        calculateNormals = _calculateNormals;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        if (calculateNormals)
            mesh.RecalculateNormals();
        else
            mesh.normals = normals.ToArray();

        return mesh;
    }
}
