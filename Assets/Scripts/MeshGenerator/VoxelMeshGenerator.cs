using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelMeshGenerator : IMeshGenerator
{ 
   public override void GenerateChunkMesh(in TerrainChunk _chunk)
    {
        VoxelMeshData meshData = new VoxelMeshData();

        Debug.Log(_chunk.clipValue);

        Vector3 centerOffset = new Vector3(_chunk.dims / 2.0f, _chunk.dims / 2.0f, _chunk.dims / 2.0f);

        for (int x = 0; x < _chunk.dims; x++)
        {
            for (int y = 0; y < _chunk.dims; y++)
            {
                for (int z = 0; z < _chunk.dims; z++)
                {                
                    if (_chunk[x, y, z] <= _chunk.clipValue)
                        continue;

                    Vector3 pos = new Vector3(x, y, z) - centerOffset;                  

                    if (x + 1 >= _chunk.dims || _chunk[x+1,y,z] <= _chunk.clipValue)
                        meshData.AddSquare(pos, new Vector3(1,0,0));
                    if (x - 1 < 0 || _chunk[x - 1, y, z] <= _chunk.clipValue)
                        meshData.AddSquare(pos, new Vector3(-1, 0, 0));

                    if (y + 1 >= _chunk.dims || _chunk[x, y + 1, z] <= _chunk.clipValue)
                        meshData.AddSquare(pos, new Vector3(0, 1, 0));
                    if (y - 1 < 0 || _chunk[x, y - 1, z] <= _chunk.clipValue)
                        meshData.AddSquare(pos, new Vector3(0, -1, 0));

                    if (z + 1 >= _chunk.dims || _chunk[x, y, z + 1] <= _chunk.clipValue)
                        meshData.AddSquare(pos, new Vector3(0, 0, 1));
                    if (z - 1 < 0 || _chunk[x, y, z - 1] <= _chunk.clipValue)
                        meshData.AddSquare(pos, new Vector3(0, 0, -1));
                }               
            }
        }

        _chunk.SetMesh(meshData);
    }

    public override void Init(ComputeShader _shader,float clipValue, int dims) {; }
}


