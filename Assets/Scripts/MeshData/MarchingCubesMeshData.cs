using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchingCubesMeshData : IMeshData
{
    int verticesIndex;

    public MarchingCubesMeshData() : base()
    {
        verticesIndex = 0;
    }

    public void AddMarchingCube(List<Vector3> _vertices, List<int> _triangles)
    {
        vertices.AddRange(_vertices);

        for (int i = 0; i < _triangles.Count; i++)
        {
          //  Debug.Log("index: " + verticesIndex);
          //  Debug.Log("before: " + _triangles[i]);
            _triangles[i] += verticesIndex;
          //  Debug.Log("after: " + _triangles[i]);
        }
       // Debug.Log(_triangles.Count);
        triangles.AddRange(_triangles);

        verticesIndex += _vertices.Count;
    }
}
