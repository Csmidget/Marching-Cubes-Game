using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class ComputeShaderMeshData : IMeshData
{

    public void ProcessTriangles(Triangle[] _triangles,int _triCount)
    {
        
        for (int i = 0; i < _triCount; i++)
        {
            vertices.Add(_triangles[i].a);
            vertices.Add(_triangles[i].b);
            vertices.Add(_triangles[i].c);

            triangles.Add(i*3);
            triangles.Add(i*3+1);
            triangles.Add(i*3+2);
        }
    }
}

