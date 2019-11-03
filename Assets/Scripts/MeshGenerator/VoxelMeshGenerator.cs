using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelMeshGenerator : IMeshGenerator
{ 
   public override void GenerateChunkMesh(TerrainChunk _chunk)
    {
        VoxelMeshData meshData = new VoxelMeshData();

        Vector3 centerOffset = new Vector3(_chunk.width / 2.0f, _chunk.height / 2.0f, _chunk.depth / 2.0f);

        for (int x = 0; x < _chunk.width; x++)
        {
            for (int y = 0; y < _chunk.height; y++)
            {
                for (int z = 0; z < _chunk.depth; z++)
                {                
                    if (_chunk[x, y, z] == 0.0f)
                        continue;

                    Vector3 pos = new Vector3(x, y, z) - centerOffset;

                    if (x + 1 >= _chunk.width || _chunk[x+1,y,z] == 0)
                        meshData.AddSquare(pos, new Vector3(1,0,0));
                    if (x - 1 < 0 || _chunk[x - 1, y, z] == 0)
                        meshData.AddSquare(pos, new Vector3(-1, 0, 0));

                    if (y + 1 >= _chunk.height || _chunk[x, y + 1, z] == 0)
                        meshData.AddSquare(pos, new Vector3(0, 1, 0));
                    if (y - 1 < 0 || _chunk[x, y - 1, z] == 0)
                        meshData.AddSquare(pos, new Vector3(0, -1, 0));

                    if (z + 1 >= _chunk.depth || _chunk[x, y, z + 1] == 0)
                        meshData.AddSquare(pos, new Vector3(0, 0, 1));
                    if (z - 1 < 0 || _chunk[x, y, z - 1] == 0)
                        meshData.AddSquare(pos, new Vector3(0, 0, -1));
                }               
            }
        }

        _chunk.SetMesh(meshData);
    }
}


