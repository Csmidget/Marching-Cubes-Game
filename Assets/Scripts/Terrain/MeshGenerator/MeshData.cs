using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;
    public Vector3[] normals;
    bool calculateNormals = true;

    public MeshData()
    {

    }

    public MeshData(bool _calculateNormals) : this()
    {
        calculateNormals = _calculateNormals;
    }

    public Mesh CreateMesh(bool collider)
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        if (!collider)
        {
            if (calculateNormals)
                mesh.RecalculateNormals();
            else
                mesh.normals = normals;
        }

        return mesh;
    }
}
