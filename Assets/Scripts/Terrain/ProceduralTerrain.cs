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

    public EditorTerrainSettings terrainSettings;

    #endregion
    //######################
    #region Private Variables

    private ChunkGenerator chunkGenerator;

    // Terrain variables
    private Dictionary<Vector3, TerrainChunk> terrainData;
    private List<TerrainChunk> activeChunks;

    //Offsets representing every chunk that is within the max render distance.
    List<Vector3> renderDistanceChunkOffsets;

    private TerrainSettings settings;

    // The chunk the viewer was in last update.
    Vector3 previousViewerChunk;

    #endregion
    //######################
    #region Terrain Modification

    public void ModifyTerrainAtPoint(Vector3 _point, float _change)
    {
        Vector3 nearestPoint = new Vector3(Mathf.Round(_point.x), Mathf.Round(_point.y), Mathf.Round(_point.z));
        Vector3 chunkPos = ChunkAtPoint(nearestPoint);
        int dims = settings.chunkDims;

        //Get the real modulo value for each point (plain % operator can return negative numbers. We only want 0 - chunkDims.)
        //This tells us which point in the chunk we have to modify.
        int posInChunkX = Mathf.RoundToInt((nearestPoint.x % dims + dims) % dims);
        int posInChunkY = Mathf.RoundToInt((nearestPoint.y % dims + dims) % dims);
        int posInChunkZ = Mathf.RoundToInt((nearestPoint.z % dims + dims) % dims);

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

    private void ModifyPointInChunk(in Vector3 _chunkPos, in int _posInChunkX, in int _posInChunkY, in int _posInChunkZ, in float _changeAmount)
    {
        //If the mesh isn't already outdated and therefore queued, enqueue it.
        if (!terrainData[_chunkPos].MeshOutdated)
            chunkGenerator.EnqueueOutdated(terrainData[_chunkPos]);

        terrainData[_chunkPos][_posInChunkX, _posInChunkY, _posInChunkZ] = Mathf.Clamp(terrainData[_chunkPos][_posInChunkX, _posInChunkY, _posInChunkZ] + _changeAmount, 0, 1);
    }

    #endregion
    //######################
    #region Monobehaviour Functions

    void Start()
    {
        Clear();
        Init();
    }

    void Update()
    {
        chunkGenerator.Update();
        UpdateVisibleChunks();
    }

    #endregion
    //######################
    #region Initialization

    public void Init()
    {
        settings = terrainSettings.Get();
        renderDistanceChunkOffsets = GetChunkOffsets();

        if (terrainData != null)
        {
            foreach (var chunk in terrainData.Values)
            {
                chunk.Destroy();
            }
        }
        terrainData = new Dictionary<Vector3, TerrainChunk>();

        activeChunks = new List<TerrainChunk>();

        chunkGenerator = new ChunkGenerator(terrainSettings);

        RenderVisibleChunks(ViewerChunk());
        chunkGenerator.GenerateChunksImmediately();
    }

    // Generates a list of offsets for all chunks within render distance sorted by distance. Runs once and generated list is cached for reuse.
    //This is done so that all chunks around the viewer can quickly be looped through starting from the nearest chunks and working outwards.
    //Pre-generating and ordering the offsets of these chunks allows for performant execution of this.
    private List<Vector3> GetChunkOffsets()
    {
        int renderDistance = settings.maxRenderDistance;
        float sqrRenderDistance = settings.maxRenderDistance * settings.maxRenderDistance;

        List<Vector3> offsetList = new List<Vector3>();

        for (int i = -renderDistance; i < renderDistance; i++)
        {
            for (int j = -renderDistance; j < renderDistance; j++)
            {
                for (int k = -renderDistance; k < renderDistance; k++)
                {
                    Vector3 vec = new Vector3(i, j, k);
                    if (vec.sqrMagnitude <= sqrRenderDistance)
                        offsetList.Add(vec);
                }
            }
        }

        offsetList.Sort((a, b) => a.sqrMagnitude.CompareTo(b.sqrMagnitude));

        return offsetList;
    }
    #endregion
    //######################
    #region Chunk Rendering

    private void UpdateVisibleChunks()
    {
        Vector3 viewerChunkPos = ViewerChunk();

        if (viewerChunkPos == previousViewerChunk)
            return;

        float sqrRenderDistance = settings.maxRenderDistance * settings.maxRenderDistance;

        //Clear any chunks that are no longer within render distance (They remain cached but are disabled)
        for (int i = activeChunks.Count - 1; i >= 0; i--)
        {
            var chunk = activeChunks[i];
            var pos = chunk.position;

            if ((viewerChunkPos - pos).sqrMagnitude > sqrRenderDistance)
            {
                activeChunks[i].SetActive(false);
                activeChunks.RemoveAt(i);
            }
        }

        RenderVisibleChunks(viewerChunkPos);

        previousViewerChunk = viewerChunkPos;
    }

    private void RenderVisibleChunks(in Vector3 _viewerChunk)
    {
        float sqrMinRenderDistance = settings.minRenderDistance * settings.minRenderDistance;

        for (int i = 0; i < renderDistanceChunkOffsets.Count; i++)
            RenderChunk(_viewerChunk, renderDistanceChunkOffsets[i], sqrMinRenderDistance);

    }

    private void RenderChunk(in Vector3 _viewerChunk, in Vector3 _chunkOffset, in float _sqrMinRenderDistance)
    {
        Vector3 chunkPos = _viewerChunk + _chunkOffset;

        if (terrainData.TryGetValue(chunkPos, out TerrainChunk chunk))
        {
            // Inefficient. Should be a much better way of doing this.
            // Checks if a chunk should now be generated with high priority (For chunks that do not yet have a mesh and are within minimum render distance)
            if (!chunk.HasMesh && _chunkOffset.sqrMagnitude <= _sqrMinRenderDistance)
                chunkGenerator.Enqueue(chunk, true);

            if (!chunk.IsActive)
            {
                chunk.SetActive(true);
                activeChunks.Add(terrainData[chunkPos]);
            }
        }
        else
        {
            // sqrmagnitude would be more efficient. Would need to store sqrMinRenderDistance aswell
            if (_chunkOffset.sqrMagnitude <= _sqrMinRenderDistance)
                CreateChunk(chunkPos, true);
            else
                CreateChunk(chunkPos);

            activeChunks.Add(terrainData[chunkPos]);
        }
    }
    #endregion
    //######################
    #region Miscellaneous Functions

    ~ProceduralTerrain()
    {
        Clear();
    }

    private Vector3 ViewerChunk()
    {
        return ChunkAtPoint(viewer.position);
    }

    private Vector3 ChunkAtPoint(in Vector3 _point)
    {
        //Ensure we are rounding up from 0.5
        float roundup = 0.001f;
        return new Vector3(Mathf.Round((_point.x - settings.halfDims + roundup) / settings.chunkDims), Mathf.Round((_point.y - settings.halfDims + roundup) / settings.chunkDims), Mathf.Round((_point.z - settings.halfDims + roundup) / settings.chunkDims));
    }

    private TerrainChunk CreateChunk(in Vector3 _chunkPosition, in bool _highPriority = false)
    {
        TerrainChunk chunk = new TerrainChunk(_chunkPosition, settings.chunkDims, transform, chunkPrefab);

        terrainData.Add(_chunkPosition, chunk);

        chunkGenerator.Enqueue(chunk, _highPriority);

        return chunk;
    }

    /// <summary>
    /// Clears all terrain data and ensure any chunk objects are cleared
    /// </summary>
    public void Clear()
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

        if (chunkGenerator != null)
        {
            chunkGenerator.Dispose();
            chunkGenerator = null;
        }
    }

    #endregion
    //######################
}
