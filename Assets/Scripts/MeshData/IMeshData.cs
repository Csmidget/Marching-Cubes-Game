using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IMeshData
{
    public List<Vector3> vertices;
    public List<int> triangles;
    public List<Vector2> uvs;

    public IMeshData()
    {
        vertices = new List<Vector3>();
        uvs = new List<Vector2>();
        triangles = new List<int>();
    }

    public virtual Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateNormals();

        return mesh;
    }

}
