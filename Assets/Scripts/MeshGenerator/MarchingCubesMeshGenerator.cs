using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchingCubesMeshGenerator : IMeshGenerator
{
    public override void GenerateChunkMesh(TerrainChunk _chunk)
    {
        MarchingCubesMeshData meshData = new MarchingCubesMeshData();

        _chunk.SetMesh(meshData);
    }
}
