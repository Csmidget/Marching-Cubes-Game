using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    #region Public Variables
    /////////////////////////
    public bool autoUpdate = true;

    public RenderType renderType;

    public ComputeShader shader;


    const int chunkDims = 16;


    public Transform viewer;
    public GameObject chunkPrefab;

    [Range(0, 1)]
    public float clipPercent;

    //Seed for the noise generator
    public int seed;

    //The noise maps frequency (aka, Zoom)
    [Range(0,1)]
    public float frequency;
    
    //Noise Offset
    public Vector3 offset;
    
    [Range(0,6)]
    public int renderDistance = 1; //How many chunks around the players current chunk will be rendered.
    /////////////////////////
    #endregion

    //The noisemap used to generate terrain
    private NoiseMap3D noiseMap;
    private IMeshGenerator meshGenerator;

    Dictionary<Vector3,TerrainChunk> terrainData;
    List<TerrainChunk> visibleChunks;

    //The chunk the viewer was in last update.
    Vector3 previousViewerChunk;

    // Start is called before the first frame update
    public void GenerateMap()
    {
        Stopwatch stopwatch = new Stopwatch();

        stopwatch.Start();
        
        Vector3 viewerChunk = viewer == null ? Vector3.zero : new Vector3(Mathf.Round(viewer.position.x / chunkDims), Mathf.Round(viewer.position.y / chunkDims), Mathf.Round(viewer.position.z / chunkDims));

        noiseMap = new NoiseMap3D(seed,frequency);
        visibleChunks = new List<TerrainChunk>();

        if (terrainData == null)
            terrainData = new Dictionary<Vector3, TerrainChunk>();

        meshGenerator = MeshGeneratorFactory.Create(renderType);
        meshGenerator.Init(shader, clipPercent, chunkDims);


        for (int i = -renderDistance; i <= renderDistance; i++)
        {
            for (int j = -renderDistance; j <= renderDistance ; j++)
            {
                for (int k = -renderDistance; k <= renderDistance ; k++)
                {
                    Vector3 chunkOffset = new Vector3(i , j , k);

                    Vector3 chunkPos = viewerChunk + chunkOffset;

                    TerrainChunk chunk;

                    if (!terrainData.ContainsKey(chunkPos))
                    {
                        chunk = CreateChunk(chunkPos);
                        terrainData.Add(chunkPos, chunk);
                    }
                    else
                        chunk = GenerateChunk(terrainData[chunkPos]);

                    visibleChunks.Add(chunk);
                }               
            }
        }

        stopwatch.Stop();

        UnityEngine.Debug.Log("Time to generate " + Mathf.Pow(renderDistance * 2, 3)  + " chunks: " + stopwatch.Elapsed);

    }

    public void Start()
    {
        Reset();
        GenerateMap();
    }

    public void Update()
    {
        UpdateVisibleChunks();
    }

    public void UpdateVisibleChunks()
    {
        Vector3 viewerChunk = viewer == null ? Vector3.zero : new Vector3(Mathf.Round(viewer.position.x / chunkDims), Mathf.Round(viewer.position.y / chunkDims), Mathf.Round(viewer.position.z / chunkDims));

        if (viewerChunk == previousViewerChunk)
            return;

        for (int i = 0; i < visibleChunks.Count; i++)
        {
            visibleChunks[i].SetVisible(false);
        }
        visibleChunks.Clear();

        for (int i = -renderDistance; i <= renderDistance; i++)
        {
            for (int j = -renderDistance; j <= renderDistance; j++)
            {
                for (int k = -renderDistance; k <= renderDistance; k++)
                {
                    Vector3 chunkOffset = new Vector3(i, j, k);

                    Vector3 chunkPos = viewerChunk + chunkOffset;

                    if (!terrainData.ContainsKey(chunkPos))
                    {
                        terrainData.Add(chunkPos, CreateChunk(chunkPos));
                        visibleChunks.Add(terrainData[chunkPos]);
                    }
                    else
                    {
                        terrainData[chunkPos].SetVisible(true);
                        visibleChunks.Add(terrainData[chunkPos]);
                    }
                    
                }
            }
        }

        previousViewerChunk = viewerChunk;
    }

    public TerrainChunk CreateChunk(Vector3 _chunkPosition)
    {
        TerrainChunk chunk = new TerrainChunk(_chunkPosition, chunkDims, clipPercent, transform,chunkPrefab);

        GenerateChunk(chunk);

        return chunk;
    }

    public TerrainChunk GenerateChunk(TerrainChunk _chunk)
    {
        int i = 0;
        for (int z = 0; z < _chunk.dims + 1; z++)
        {
            for (int y = 0; y < _chunk.dims + 1; y++)
            {
                for (int x = 0; x < _chunk.dims + 1; x++)
                {
                    _chunk.terrainMap[i] = noiseMap.Evaluate(new Vector3(x, y, z) + offset + _chunk.rawPosition);
                    _chunk.clipValue = clipPercent;
                    i++;
                }
            }
        }

        meshGenerator.GenerateChunkMesh(_chunk);

        return _chunk;
    }

    public void Reset()
    {
        if (terrainData != null)
        {
            foreach (var chunk in terrainData)
            {
                chunk.Value.Destroy();
            }
        }
        terrainData = new Dictionary<Vector3, TerrainChunk>();

        while (transform.childCount > 0)
        {
            foreach (Transform child in transform)
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }
}
