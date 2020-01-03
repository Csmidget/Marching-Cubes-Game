using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class TerrainManager : MonoBehaviour
{
    //######################
    #region Public Variables

    public bool autoUpdate = true;

    public Transform viewer;
    public GameObject chunkPrefab;

    public TerrainSettings settingsManager;

    public static TerrainManager Instance
    {
        get
        {
            return instance;
        }
    }


    #endregion
    //######################
    #region Private Variables

    private NoiseMap3D noiseMap;

    private IMeshGenerator meshGenerator;
    
    // Terrain variables
    private Dictionary<Vector3, TerrainChunk> terrainData;
    private List<TerrainChunk> activeChunks;
    List<Vector3> terrainChunkOffsets;
    private TerrainSettings.TerrainInnerSettings settings;

    // Multithreading
    Thread generateChunksThread;
    private Queue<TerrainChunk> chunksToGeneratePriority;
    private Queue<TerrainChunk> chunksToGenerate;
    private Queue<TerrainChunk> generatedChunks;

    private static TerrainManager instance = null;

    // The chunk the viewer was in last update.
    Vector3 previousViewerChunk;

    #endregion
    //######################
    #region Public Functions

    public void ModifyTerrainAtPoint(Vector3 _point, float _change)
    {
        Debug.Log("raw point: " + _point);
        Vector3 nearestPoint = new Vector3(Mathf.Round(_point.x), Mathf.Round(_point.y), Mathf.Round(_point.z));
        Vector3 chunk = ChunkAtPoint(nearestPoint);

        
        Debug.Log("chunk: " + chunk);
        Debug.Log("point: " + nearestPoint);

        int posInChunkX = Mathf.RoundToInt (((nearestPoint.x - settings.halfDims) % settings.chunkDims + 16) % settings.chunkDims);
        int posInChunkY = Mathf.RoundToInt (((nearestPoint.y - settings.halfDims) % settings.chunkDims + 16) % settings.chunkDims);
        int posInChunkZ = Mathf.RoundToInt (((nearestPoint.z - settings.halfDims) % settings.chunkDims + 16) % settings.chunkDims);

        Debug.Log("pointInChunk: " +  posInChunkX + "," + posInChunkY + "," + posInChunkZ);

        terrainData[chunk][posInChunkX, posInChunkY, posInChunkZ] += _change;
        meshGenerator.GenerateChunkMesh(terrainData[chunk]);
        terrainData[chunk].ApplyMesh();

        if (posInChunkX == 0)
        {
            terrainData[chunk + Vector3.left][settings.chunkDims, posInChunkY, posInChunkZ] += _change;
            meshGenerator.GenerateChunkMesh(terrainData[chunk + Vector3.left]);
            terrainData[chunk + Vector3.left].ApplyMesh();
        }
        if (posInChunkY == 0)
        {
            terrainData[chunk + Vector3.down][posInChunkX, settings.chunkDims, posInChunkZ] += _change;
            meshGenerator.GenerateChunkMesh(terrainData[chunk + Vector3.down]);
            terrainData[chunk + Vector3.down].ApplyMesh();
        }
        if (posInChunkZ == 0)
        {
            terrainData[chunk + Vector3.back][posInChunkX, posInChunkY, settings.chunkDims] += _change;
            meshGenerator.GenerateChunkMesh(terrainData[chunk + Vector3.back]);
            terrainData[chunk + Vector3.back].ApplyMesh();
        }
        
        if (posInChunkX == 0 && posInChunkY == 0)
        {
            terrainData[chunk + new Vector3(-1, -1, 0)][settings.chunkDims, settings.chunkDims, posInChunkZ] += _change;
            meshGenerator.GenerateChunkMesh(terrainData[chunk + new Vector3(-1, -1, 0)]);
            terrainData[chunk + new Vector3(-1, -1, 0)].ApplyMesh();
        }
        if (posInChunkX == 0 && posInChunkZ == 0)
        {
            terrainData[chunk + new Vector3(-1, 0, -1)][settings.chunkDims, posInChunkY, settings.chunkDims] += _change;
            meshGenerator.GenerateChunkMesh(terrainData[chunk + new Vector3(-1, 0, -1)]);
            terrainData[chunk + new Vector3(-1, 0, -1)].ApplyMesh();
        }
        if (posInChunkY == 0 && posInChunkZ == 0)
        {
            terrainData[chunk + new Vector3(0, -1, -1)][posInChunkX, settings.chunkDims, settings.chunkDims] += _change;
            meshGenerator.GenerateChunkMesh(terrainData[chunk + new Vector3(0, -1, -1)]);
            terrainData[chunk + new Vector3(0, -1, -1)].ApplyMesh();
        }
        
        if (posInChunkX == 0 && posInChunkY == 0 && posInChunkZ == 0)
        {
            terrainData[chunk + new Vector3(-1, -1, -1)][settings.chunkDims, settings.chunkDims, settings.chunkDims] += _change;
            meshGenerator.GenerateChunkMesh(terrainData[chunk + new Vector3(-1, -1, -1)]);
            terrainData[chunk + new Vector3(-1, -1, -1)].ApplyMesh();
        }


    }

    #endregion
    //######################
    #region Monobehaviour Functions

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Attempted to instantiate multiple TerrainManagers. Duplicate manager destroyed");
            Destroy(this);
        }
        else
        {
            instance = this;
        }

    }

    private void Start()
    {
        Reset();
        GenerateMap();
    }

    private void Update()
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

    private void Init()
    {
        settings = settingsManager.Get();
        terrainChunkOffsets = InitializeChunkoffsets(settings.maxRenderDistance);

        generatedChunks = new Queue<TerrainChunk>();
        chunksToGenerate = new Queue<TerrainChunk>();
        chunksToGeneratePriority = new Queue<TerrainChunk>();

        if (terrainData != null)
        {
            foreach (var chunk in terrainData)
            {
                chunk.Value.Destroy();
            }
        }
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
    private List<Vector3> InitializeChunkoffsets(int _renderDistance)
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

    private void UpdateVisibleChunks()
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

    private void RenderVisibleChunks(int _renderDistance, Vector3 _viewerChunk, bool _generateImmediately)
    {
        for (int i = 0; i < terrainChunkOffsets.Count; i++)
        {
            if (terrainChunkOffsets[i].magnitude > _renderDistance)
                break;

            RenderChunk(_viewerChunk, terrainChunkOffsets[i], _generateImmediately);
        }
    }

    private void RenderChunk(in Vector3 _viewerChunk, Vector3 _chunkOffset, bool _generateImmediately)
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
    private void GenerateQueuedChunks()
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
    private void UpdateGeneratedChunks()
    {

        if (!generateChunksThread.IsAlive && settings.multiThreaded && meshGenerator.SupportsMultiThreading)
            generateChunksThread.Start();

        if (!settings.multiThreaded)
            GenerateQueuedChunks();

        // Assign newly generated terrain meshes to their chunk game objects.
        while (generatedChunks.Count > 0)
        {
            TerrainChunk chunk = generatedChunks.Dequeue();

            if (chunk != null)
                chunk.ApplyMesh();
        }
    }

    private TerrainChunk GenerateChunk(TerrainChunk _chunk)
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
        return ChunkAtPoint(viewer.position);
    }

    private Vector3 ChunkAtPoint(Vector3 _point)
    {
        return new Vector3(Mathf.Round(_point.x / settings.chunkDims), Mathf.Round(_point.y / settings.chunkDims), Mathf.Round(_point.z / settings.chunkDims));
    }

    private Vector3 RoundVec3ToInt(Vector3 _vec)
    {
        return new Vector3(Mathf.Round(_vec.x), Mathf.Round(_vec.y), Mathf.Round(_vec.z));
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
