using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    #region Public Variables
    /////////////////////////
    public bool autoUpdate = true;

    public Transform viewer;
    public GameObject chunkPrefab;

    public TerrainSettingsManager terrainSettings;
    /////////////////////////
    #endregion

    #region Private Variables
    //The noisemap used to generate terrain
    private NoiseMap3D noiseMap;
    private IMeshGenerator meshGenerator;
    private TerrainSettingsManager.TerrainSettings settings;

    Dictionary<Vector3, TerrainChunk> terrainData;
    List<TerrainChunk> loadedChunks;

    //The chunk the viewer was in last update.
    Vector3 previousViewerChunk;
    #endregion

    #region Monobehaviour Functions

    public void Start()
    {
        Reset();
        GenerateMap();
    }

    public void Update()
    {
        UpdateVisibleChunks();
    }
    #endregion

    #region Miscellaneous Functions

    // Start is called before the first frame update
    public void GenerateMap()
    {
        Stopwatch stopwatch = new Stopwatch();

        settings = terrainSettings.Get();

        stopwatch.Start();

        Vector3 viewerChunk = viewer == null ? Vector3.zero : new Vector3(Mathf.Round(viewer.position.x / settings.chunkDims), Mathf.Round(viewer.position.y / settings.chunkDims), Mathf.Round(viewer.position.z / settings.chunkDims));

        noiseMap = new NoiseMap3D(settings.seed, settings.frequency);
        loadedChunks = new List<TerrainChunk>();

        if (terrainData == null)
            terrainData = new Dictionary<Vector3, TerrainChunk>();

        meshGenerator = MeshGeneratorFactory.Create(terrainSettings.renderType);
        meshGenerator.Init(settings);

        RenderChunks(settings.renderDistance, viewerChunk);

        stopwatch.Stop();

        UnityEngine.Debug.Log("Time to generate " + Mathf.Pow(settings.renderDistance * 2 + 1, 3) + " chunks: " + stopwatch.Elapsed);

    }

    public void UpdateVisibleChunks()
    {
        Vector3 viewerChunk = viewer == null ? Vector3.zero : new Vector3(Mathf.Round(viewer.position.x / settings.chunkDims), Mathf.Round(viewer.position.y / settings.chunkDims), Mathf.Round(viewer.position.z / settings.chunkDims));

        if (viewerChunk == previousViewerChunk)
            return;

        for (int i = 0; i < loadedChunks.Count; i++)
        {
            loadedChunks[i].SetActive(false);
        }
        loadedChunks.Clear();

        RenderChunks(settings.renderDistance, viewerChunk);

        previousViewerChunk = viewerChunk;
    }

    public void RenderChunks(int _renderDistance, Vector3 _viewerChunk)
    {
        for (int i = -_renderDistance; i <= _renderDistance; i++)
        {
            for (int j = -_renderDistance; j <= _renderDistance; j++)
            {
                for (int k = -_renderDistance; k <= _renderDistance; k++)
                {
                    Vector3 chunkOffset = new Vector3(i, j, k);

                    Vector3 chunkPos = _viewerChunk + chunkOffset;

                    if (!terrainData.ContainsKey(chunkPos))
                    {
                        terrainData.Add(chunkPos, CreateChunk(chunkPos));
                        loadedChunks.Add(terrainData[chunkPos]);
                    }
                    else
                    {
                        terrainData[chunkPos].SetActive(true);
                        loadedChunks.Add(terrainData[chunkPos]);
                    }

                }
            }
        }
    }

    public TerrainChunk CreateChunk(Vector3 _chunkPosition)
    {
        var settings = terrainSettings.Get();
        TerrainChunk chunk = new TerrainChunk(_chunkPosition, settings.chunkDims, transform, chunkPrefab);

        GenerateChunk(chunk);

        return chunk;
    }

    public TerrainChunk GenerateChunk(TerrainChunk _chunk)
    {
        var settings = terrainSettings.Get();
        int i = 0;
        for (int z = 0; z < _chunk.dims + 1; z++)
        {
            for (int y = 0; y < _chunk.dims + 1; y++)
            {
                for (int x = 0; x < _chunk.dims + 1; x++)
                {
                    _chunk.terrainMap[i] = noiseMap.Evaluate(new Vector3(x, y, z) + settings.offset + _chunk.rawPosition);
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

        if (meshGenerator != null)
        {
            meshGenerator.Dispose();
            meshGenerator = null;
        }
    }

    #endregion
}
