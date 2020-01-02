using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class TerrainGenerator : MonoBehaviour
{
    //######################
    #region Public Variables

    public bool autoUpdate = true;

    public Transform viewer;
    public GameObject chunkPrefab;

    public TerrainSettingsManager settingsManager;

    #endregion
    //######################
    #region Private Variables

    private NoiseMap3D noiseMap;

    private IMeshGenerator meshGenerator;
    
    // Terrain variables
    private Dictionary<Vector3, TerrainChunk> terrainData;
    private List<TerrainChunk> activeChunks;
    List<Vector3> terrainChunkOffsets;
    private TerrainSettingsManager.TerrainSettings settings;

    // Multithreading
    Thread generateChunksThread;
    private Queue<TerrainChunk> chunksToGeneratePriority;
    private Queue<TerrainChunk> chunksToGenerate;
    private Queue<TerrainChunk> generatedChunks;

    // The chunk the viewer was in last update.
    Vector3 previousViewerChunk;

    #endregion
    //######################
    #region Monobehaviour Functions

    public void Start()
    {
        Reset();
        GenerateMap();
    }

    public void Update()
    {
        UpdateGeneratedChunks();
        UpdateVisibleChunks();
    }

    #endregion
    //######################
    #region Multithreading

    public void GenerateChunksThread()
    {
        while (settings.multiThreaded)
            GenerateQueuedChunks();
    }

    #endregion
    //######################
    #region Initialization

    public void Init()
    {
        settings = settingsManager.Get();
        terrainChunkOffsets = InitializeChunkoffsets(settings.maxRenderDistance);

        generatedChunks = new Queue<TerrainChunk>();
        chunksToGenerate = new Queue<TerrainChunk>();
        chunksToGeneratePriority = new Queue<TerrainChunk>();

        if (terrainData == null)
            terrainData = new Dictionary<Vector3, TerrainChunk>();

        activeChunks = new List<TerrainChunk>();

        noiseMap = new NoiseMap3D(settings.seed, settings.frequency);
        
        meshGenerator = MeshGeneratorFactory.Create(settingsManager.renderType);
        meshGenerator.Init(settings);

        if (settings.multiThreaded)
        {
            ThreadStart threadStart = new ThreadStart(GenerateChunksThread);
            generateChunksThread = new Thread(threadStart);
        }

        if (settings.multiThreaded && meshGenerator.SupportsMultiThreading)
            generateChunksThread.Start();
    }

    public void GenerateMap()
    {
        Init();
       
        RenderVisibleChunks(settings.minRenderDistance, GetViewerChunk(), true);
    }

    // Generates a list of offsets for all chunks within render distance sorted in order of distance. Runs once and generated list is cached for reuse.
    public List<Vector3> InitializeChunkoffsets(int _renderDistance)
    {
        List<Vector3> outList = new List<Vector3>();

        for (int i = -_renderDistance; i < _renderDistance; i++)
        {
            for (int j = -_renderDistance; j < _renderDistance; j++)
            {
                for (int k = -_renderDistance; k < _renderDistance; k++)
                {
                    outList.Add(new Vector3(i, j, k));
                }
            }
        }

        outList.Sort((a, b) => a.sqrMagnitude.CompareTo(b.sqrMagnitude));

        return outList;
    }

    #endregion
    //######################
    #region Chunk Rendering

    public void UpdateVisibleChunks()
    {
        Vector3 viewerChunk = GetViewerChunk();

        if (viewerChunk == previousViewerChunk)
            return;

        for (int i = 0; i < activeChunks.Count; i++)
        {
            activeChunks[i].SetActive(false);
        }
        activeChunks.Clear();

        RenderVisibleChunks(settings.maxRenderDistance, viewerChunk, false);

        previousViewerChunk = viewerChunk;
    }

    public void RenderVisibleChunks(int _renderDistance, Vector3 _viewerChunk, bool _generateImmediately)
    {
        for (int i = 0; i < terrainChunkOffsets.Count; i++)
        {
            if (terrainChunkOffsets[i].magnitude > _renderDistance)
                break;

            RenderChunk(_viewerChunk, terrainChunkOffsets[i], _generateImmediately);
        }
    }

    public void RenderChunk(in Vector3 _viewerChunk, Vector3 _chunkOffset, bool _generateImmediately)
    {
        Vector3 chunkPos = _viewerChunk + _chunkOffset;

        if (terrainData.ContainsKey(chunkPos))
        {
            // Inefficient. Should be a much better way of doing this.
            // Checks if a chunk should now be generated with high priority (For chunks that do not yet have a mesh and are within minimum render distance)
            if (!terrainData[chunkPos].HasMesh() && _chunkOffset.magnitude <= settings.minRenderDistance)
                chunksToGeneratePriority.Enqueue(terrainData[chunkPos]);

            terrainData[chunkPos].SetActive(true);
            activeChunks.Add(terrainData[chunkPos]);
        }
        else
        {
            TerrainChunk chunk;

            if (_generateImmediately)
            {
                chunk = CreateChunk(chunkPos);
            }
            else
            {
                chunk = CreateEmptyChunk(chunkPos);

                // sqrmagnitude would be more efficient. Would need to store sqrMinRenderDistance aswell
                if (_chunkOffset.magnitude <= settings.minRenderDistance)
                    chunksToGeneratePriority.Enqueue(chunk);
                else
                    chunksToGenerate.Enqueue(chunk);
            }

            terrainData.Add(chunkPos, chunk);
            activeChunks.Add(terrainData[chunkPos]);
        }
    }
    #endregion
    //######################
    #region Chunk Generation
    public void GenerateQueuedChunks()
    {
        // Force all high priority chunks to be generated
        while (chunksToGeneratePriority.Count > 0)
        {
            TerrainChunk currentChunk = chunksToGeneratePriority.Dequeue();
            GenerateChunk(currentChunk);
            generatedChunks.Enqueue(currentChunk);
        }

        // Generate a single low priority chunk.
        if (chunksToGenerate.Count > 0)
        {
            TerrainChunk currentChunk = chunksToGenerate.Dequeue();
            GenerateChunk(currentChunk);
            generatedChunks.Enqueue(currentChunk);
        }
    }

    /// <summary>
    /// Checks for any newly generated chunk meshes and applies them to their gameobjects. Can only be done on the main thread.
    /// If multithreading isn't enabled, will also generate chunk meshes manually
    /// </summary>
    public void UpdateGeneratedChunks()
    {

        if (!generateChunksThread.IsAlive && settings.multiThreaded && meshGenerator.SupportsMultiThreading)
            generateChunksThread.Start();

        if (!settings.multiThreaded)
            GenerateQueuedChunks();

        // Assign newly generated terrain meshes to their chunk game objects.
        while (generatedChunks.Count > 0)
        {
            TerrainChunk chunk = generatedChunks.Dequeue();
            chunk.ApplyMesh();
        }
    }

    public TerrainChunk GenerateChunk(TerrainChunk _chunk)
    {
        if (_chunk.HasMesh())
            return _chunk;

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

    #endregion
    //######################
    #region Miscellaneous Functions

    private Vector3 GetViewerChunk()
    {
        return viewer == null ? Vector3.zero : new Vector3(Mathf.Round(viewer.position.x / settings.chunkDims), Mathf.Round(viewer.position.y / settings.chunkDims), Mathf.Round(viewer.position.z / settings.chunkDims));
    }


    private TerrainChunk CreateEmptyChunk(Vector3 _chunkPosition)
    {
        return new TerrainChunk(_chunkPosition, settings.chunkDims, transform, chunkPrefab);
    }

    private TerrainChunk CreateChunk(Vector3 _chunkPosition)
    {
        TerrainChunk chunk = CreateEmptyChunk(_chunkPosition);

        GenerateChunk(chunk);

        chunk.ApplyMesh();

        return chunk;
    }

    /// <summary>
    /// Clears all terrain data and ensure any chunk objects are cleared
    /// </summary>
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

        // Unity doesn't seem to properly clear all children in a single loop. The while ensures all children are cleared.
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
    //######################
}
