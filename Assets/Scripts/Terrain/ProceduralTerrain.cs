using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class ProceduralTerrain : MonoBehaviour
{
    //######################
    #region Public Variables

    public bool autoUpdate = true;

    public Transform viewer;
    public GameObject chunkPrefab;

    public TerrainSettings terrainSettings;

    public static ProceduralTerrain Instance
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

    private Queue<TerrainChunk> outdatedChunks;

    private static ProceduralTerrain instance = null;

    // The chunk the viewer was in last update.
    Vector3 previousViewerChunk;

    #endregion
    //######################
    #region Public Functions

    public void ModifyTerrainAtPoint(Vector3 _point, float _change)
    {
        Vector3 nearestPoint = new Vector3(Mathf.Round(_point.x), Mathf.Round(_point.y), Mathf.Round(_point.z));
        Vector3 chunkPos = ChunkAtPoint(nearestPoint);
        int dims = settings.chunkDims;

        //Get the real modulo value for each point (plain % operator can return negative numbers. We only want 0 - chunkDims.)
        int posInChunkX = Mathf.RoundToInt ((nearestPoint.x % dims + dims) % dims);
        int posInChunkY = Mathf.RoundToInt ((nearestPoint.y % dims + dims) % dims);
        int posInChunkZ = Mathf.RoundToInt ((nearestPoint.z % dims + dims) % dims);
        
        //
        ModifyPointInChunk(chunkPos, posInChunkX, posInChunkY, posInChunkZ, _change);

        //All chunks store an extra layer of data so that they can merge seamlessly with neighbours.
        //We need to ensure that any neighbours storing points on the edge of the chunk are also updated.
        if (posInChunkX == 0)
            ModifyPointInChunk(chunkPos + Vector3.left, dims, posInChunkY, posInChunkZ, _change);
        if (posInChunkY == 0)
            ModifyPointInChunk(chunkPos + Vector3.down, posInChunkX, dims, posInChunkZ, _change);
        if (posInChunkZ == 0)
            ModifyPointInChunk(chunkPos + Vector3.back, posInChunkX, posInChunkY, dims, _change);

        if (posInChunkX == 0 && posInChunkY == 0)
            ModifyPointInChunk(chunkPos + new Vector3(-1, -1, 0), dims, dims, posInChunkZ, _change);
        if (posInChunkX == 0 && posInChunkZ == 0)
            ModifyPointInChunk(chunkPos + new Vector3(-1, 0, -1), dims, posInChunkY, dims, _change);
        if (posInChunkY == 0 && posInChunkZ == 0) 
            ModifyPointInChunk(chunkPos + new Vector3(0, -1, -1), posInChunkX, dims, dims, _change);

        if (posInChunkX == 0 && posInChunkY == 0 && posInChunkZ == 0)
            ModifyPointInChunk(chunkPos + new Vector3(-1, -1, -1), dims, dims, dims, _change);
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

    void Start()
    {
        Reset();
        GenerateMap();
    }

    void Update()
    {
        UpdateGeneratedChunks();
        UpdateVisibleChunks();
    }

    #endregion
    //######################
    #region Initialization

    private void Init()
    {
        settings = terrainSettings.Get();
        terrainChunkOffsets = InitializeChunkoffsets(settings.maxRenderDistance);

        outdatedChunks = new Queue<TerrainChunk>();

        if (terrainData != null)
        {
            foreach (var chunk in terrainData.Values)
            {
                chunk.Destroy();
            }
        }
        terrainData = new Dictionary<Vector3, TerrainChunk>();

        activeChunks = new List<TerrainChunk>();

        noiseMap = new NoiseMap3D(settings.seed, settings.frequency,settings.offset);

        meshGenerator = MeshGeneratorFactory.Create(terrainSettings.renderType);
        meshGenerator.Init(settings);   
    }

    public void GenerateMap()
    {
        Init();

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        RenderVisibleChunks(GetViewerChunk(), true);
        stopwatch.Stop();
        Debug.Log("Mapgen time: " + stopwatch.Elapsed);
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
    //    Stopwatch stopwatch = new Stopwatch();

    //    stopwatch.Start();

        Vector3 viewerChunkPos = GetViewerChunk();

        if (viewerChunkPos == previousViewerChunk)
            return;
       
        float sqrRenderDistance = settings.maxRenderDistance * settings.maxRenderDistance;

        //Clear any chunks that are no longer within render distance (They remain cached but are disabled)
        for (int i = 0; i < activeChunks.Count; i++)
        {
            if ((viewerChunkPos - activeChunks[i].position).sqrMagnitude > sqrRenderDistance)
            {
                activeChunks[i].SetActive(false);
                activeChunks.RemoveAt(i);
            }
        }

        RenderVisibleChunks(viewerChunkPos, false);

        previousViewerChunk = viewerChunkPos;

    //    stopwatch.Stop();

    //    Debug.Log(stopwatch.ElapsedMilliseconds);

    }

    private void RenderVisibleChunks(in Vector3 _viewerChunk, in bool _generateImmediately)
    {
        float sqrMinRenderDistance = settings.minRenderDistance * settings.minRenderDistance;
        float sqrMaxRenderDistance = settings.maxRenderDistance * settings.maxRenderDistance;

        for (int i = 0; i < terrainChunkOffsets.Count; i++)
        {
            if (terrainChunkOffsets[i].sqrMagnitude > sqrMaxRenderDistance)
                break;

            RenderChunk(_viewerChunk, terrainChunkOffsets[i], _generateImmediately, sqrMinRenderDistance);
        }
    }

    private void RenderChunk(in Vector3 _viewerChunk, in Vector3 _chunkOffset, in bool _generateImmediately, in float _sqrMinRenderDistance)
    {
        Vector3 chunkPos = _viewerChunk + _chunkOffset;

        if (terrainData.TryGetValue(chunkPos, out TerrainChunk chunk))
        {
            // Inefficient. Should be a much better way of doing this.
            // Checks if a chunk should now be generated with high priority (For chunks that do not yet have a mesh and are within minimum render distance)
            if (!chunk.HasMesh && _chunkOffset.sqrMagnitude <= _sqrMinRenderDistance)
               meshGenerator.EnqueueChunk(chunk, true);

            if (!chunk.IsActive)
            {
                chunk.SetActive(true);
                activeChunks.Add(terrainData[chunkPos]);
            }
        }
        else
        {
            if (_generateImmediately)
            {
                chunk = CreateChunk(chunkPos);
            }
            else
            {
                chunk = CreateEmptyChunk(chunkPos);

                // sqrmagnitude would be more efficient. Would need to store sqrMinRenderDistance aswell
                if (_chunkOffset.sqrMagnitude <= _sqrMinRenderDistance)
                    meshGenerator.EnqueueChunk(chunk, true);
                else
                    meshGenerator.EnqueueChunk(chunk);
            }

            terrainData.Add(chunkPos, chunk);
            activeChunks.Add(terrainData[chunkPos]);
        }
    }
    #endregion
    //######################
    #region Chunk Generation

    /// <summary>
    /// Checks for any newly generated chunk meshes and applies them to their gameobjects. Can only be done on the main thread.
    /// If multithreading isn't enabled, will also generate chunk meshes manually
    /// </summary>
    private void UpdateGeneratedChunks()
    {

        meshGenerator.Update();

        while(outdatedChunks.Count > 0)
        {
            TerrainChunk currentChunk = outdatedChunks.Dequeue();
            meshGenerator.GenerateChunkMesh(currentChunk);
            
        }
    }

    #endregion
    //######################
    #region Miscellaneous Functions

    private Vector3 GetViewerChunk()
    {
        return ChunkAtPoint(viewer.position);
    }

    private Vector3 ChunkAtPoint(in Vector3 _point)
    {
        //Ensure we are rounding up from 0.5
        float roundup = 0.001f;
        return new Vector3(Mathf.Round((_point.x - settings.halfDims + roundup) / settings.chunkDims), Mathf.Round((_point.y - settings.halfDims + roundup) / settings.chunkDims), Mathf.Round((_point.z - settings.halfDims + roundup) / settings.chunkDims));
    }

    private void ModifyPointInChunk(in Vector3 _chunkPos, in int _posInChunkX, in int _posInChunkY, in int _posInChunkZ, in float _changeAmount)
    {
        //This is a dumb hack that should be resolved in another way. Essentially checks if we're about to make the first change to a chunk, if yes add it to the queue to regenerate mesh.
        if (!terrainData[_chunkPos].MeshOutdated)
            outdatedChunks.Enqueue(terrainData[_chunkPos]);

        terrainData[_chunkPos][_posInChunkX, _posInChunkY, _posInChunkZ] = Mathf.Clamp(terrainData[_chunkPos][_posInChunkX, _posInChunkY, _posInChunkZ] + _changeAmount,0,1);
    }

    private Vector3 RoundVec3ToInt(Vector3 _vec)
    {
        return new Vector3(Mathf.Round(_vec.x), Mathf.Round(_vec.y), Mathf.Round(_vec.z));
    }

    private TerrainChunk CreateEmptyChunk(Vector3 _chunkPosition)
    {
        TerrainChunk chunk = new TerrainChunk(noiseMap, _chunkPosition, settings.chunkDims, transform, chunkPrefab);

        chunk.terrainMap = noiseMap.EvaluateChunk(chunk.rawPosition, chunk.dims + 1);

        return chunk;
    }

    private TerrainChunk CreateChunk(Vector3 _chunkPosition)
    {
        TerrainChunk chunk = CreateEmptyChunk(_chunkPosition);

        meshGenerator.GenerateChunkMesh(chunk);

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
