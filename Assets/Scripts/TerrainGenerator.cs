using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    #region Public Variables
    /////////////////////////
    public bool autoUpdate = true;
    [Range(0,64)]
    public int chunkWidth = 2;
    [Range(0, 255)]
    public int chunkHeight = 2;
    [Range(0, 100)]
    public int clipPercent;

    //Seed for the noise generator
    public int seed;

    //The noise maps frequency (aka, Zoom)
    [Range(0,1)]
    public float frequency;
    
    //Noise Offset
    public Vector3 offset;

    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    /////////////////////////
    #endregion

    //The noisemap used to generate terrain
    private NoiseMap3D noiseMap;
    private MeshGenerator meshGenerator;

    //Used for lerping
    float maxNoise;
    float minNoise;

    ChunkData terrainData;

    // Start is called before the first frame update
    public void GenerateMap()
    {
        terrainData = new ChunkData(chunkWidth,chunkHeight);
        meshGenerator = new MeshGenerator();
        minNoise = float.MaxValue;
        maxNoise = float.MinValue;

        noiseMap = new NoiseMap3D(seed,frequency);
        int i = 0;
        for (int z = 0; z < chunkWidth; z++)
        {
            for (int y = 0; y < chunkHeight; y++)
            {
                for (int x = 0; x < chunkWidth; x++)
                {
                    terrainData.terrainMap[i] = noiseMap.Evaluate(new Vector3(x, y, z) + offset);
                    minNoise = Mathf.Min(terrainData.terrainMap[i], minNoise);
                    maxNoise = Mathf.Max(terrainData.terrainMap[i], maxNoise);
                    i++;
                }
            }
        }

        for (int j = 0; j < chunkWidth * chunkHeight * chunkWidth; j++)
        {
            if (terrainData.terrainMap[j] / maxNoise * 100 <= clipPercent)
                terrainData.terrainMap[j] = 0;
        }

        var meshData = meshGenerator.GenerateChunkMesh(terrainData);
        meshFilter.sharedMesh = meshData.CreateMesh();
    }

    /// <summary>
    /// Debug temp.
    /// </summary>
    private void OnDrawGizmos()
    {
    //    if (terrainData != null)
    //    {
    //        int i = 0;
    //        for (int z = 0; z < chunkWidth; z++)
    //        {
    //            for (int y = 0; y < chunkHeight; y++)
    //            {
    //                for (int x = 0; x < chunkWidth; x++)
    //                {                    
    //                    Gizmos.color = Color.Lerp(Color.white, Color.black, Mathf.InverseLerp(minNoise,maxNoise, terrainData.terrainMap[i]));
    //                    if (terrainData.terrainMap[i] / maxNoise * 100 > clipPercent)
    //                        Gizmos.DrawSphere(transform.position + new Vector3(x, y, z),Mathf.InverseLerp(((float)clipPercent)/100.0f,maxNoise, terrainData.terrainMap[i]) /2 );
    //                    i++;
    //                }
    //            }
    //        }
    //    }
    }
}
