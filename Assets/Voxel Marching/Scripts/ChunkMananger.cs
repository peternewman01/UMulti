using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class ChunkMananger : MonoBehaviour
{
    public static ChunkMananger Instance;
    [SerializeField] private MarchingAlgorithm marchingAlgorithmPrefab;
    [Tooltip("How large a sub-cube is for chunk generation")]
    [SerializeField] private uint subCubeSize = 32;
    [Tooltip("How many sub-cubes tall a chunk is")]
    [SerializeField] private uint subCubesPerChunk = 16;
    [Tooltip("How many sub-cubes to generate per frame")]
    [SerializeField] private uint chunksPerFrame = 4;
    //[SerializeField] private MarchingAlgorithm algorithmPrefab;
    private List<Vector2Int> frontier = new();
    private Vector2Int playerChunk = Vector2Int.zero;
    private float seed = 0;
    public float Seed
    {
        get => seed;
        set { }
    }

    //TEMP
    public IslandGeneratorManager islandManager;

    private void OnValidate()
    {
        if (Instance == null)
        {
            Instance = this;

        }
        else if (Instance != this)
        {
            Debug.LogError("Chunk Manager already exists in scene");
            DestroyImmediate(this);
        }


    }

    private void Start()
    {
        islandManager.CreateNewGenerator(0);
        frontier.Add(Vector2Int.zero);
        SetSeed(UnityEngine.Random.Range(-10000, 10000));
        //PopulateFrontierWithChunks();
        StartCoroutine(ChunkRecursion());

        for(int i = 0; i < 16; i++)
        {
            islandManager.CreateNewGenerator();
        }
    }

    //TEMP
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.B))
        {
            islandManager.CreateNewGenerator();
        }

        if(FindFirstObjectByType<PlayerManager>())
        {
            playerChunk = WorldToChunk(FindFirstObjectByType<PlayerManager>().transform.position);
        }
    }

    private class DistanceCompare : IComparer<Vector2Int>
    {
        public int Compare(Vector2Int x, Vector2Int y)
        {
            float xDist = Vector2Int.Distance(Instance.GetPlayerChunk(), x);
            float yDist = Vector2Int.Distance(Instance.GetPlayerChunk(), y);

            if (xDist > yDist) return 1;
            else if (xDist < yDist) return -1;
            else return 0;
        }
    }

    private IEnumerator ChunkRecursion()
    {
        frontier.Sort(new DistanceCompare());
        uint cubeCount = 0;
        while (frontier.Count > 0 && cubeCount < chunksPerFrame * subCubesPerChunk)
        {
            Vector2Int current = frontier[0];
            cubeCount += GenerateChunk(current);
            frontier.RemoveAt(0);
        }

        float fps = 1.0f / Time.deltaTime;

        if (chunksPerFrame > 1 && fps < 30)
            chunksPerFrame--;
        else if (fps > 90)
            chunksPerFrame++;

            yield return new WaitForEndOfFrame();
        StartCoroutine(ChunkRecursion());
    }


    public uint GenerateChunk(Vector2Int chunk)
    {
        string name = "Chunk (" + chunk.x + ", " + chunk.y + ")";
        GameObject spawnedChunk = new GameObject();
        spawnedChunk.transform.parent = transform;
        spawnedChunk.name = "Chunk (" + chunk.x + ", " + chunk.y + ")";
        for (uint i = 0; i < subCubesPerChunk; i++)
        {
            MarchingAlgorithm subChunk = Instantiate(marchingAlgorithmPrefab);
            subChunk.name = "SubChunk " + i;
            subChunk.transform.parent = spawnedChunk.transform;
            subChunk.transform.position = ChunkToWorld(chunk) + new Vector3(0, i * subCubeSize, 0);
            subChunk.Init(chunk, i);
            subChunk.GenerateIsland();
        }

        return subCubesPerChunk;
    }

    public void AddChunksInRadiusAtPosition(Vector3 worldPos, float radius)
    {
        AddChunks(GetChunksInRadiusAtPosition(worldPos, radius));
    }

    public void AddChunks(Vector2Int[] chunks)
    {
        foreach(var chunk in chunks) {
            if (frontier.Contains(chunk)) continue;
            frontier.Add(chunk);
        }
    }

    public Vector2Int[] GetChunksInRadiusAtPosition(Vector3 worldPos, float radius)
    {
        Vector2Int min = WorldToChunk(new Vector3(worldPos.x - radius, worldPos.y, worldPos.z - radius));
        Vector2Int max = WorldToChunk(new Vector3(worldPos.x + radius, worldPos.y, worldPos.z + radius));
        Vector2Int centerChunk = WorldToChunk(worldPos);
        Vector2Int currentChunk = new();
        List<Vector2Int> chunkPositions = new();

        for(int x = min.x; x < max.x; x++)
        {
            for (int z = min.y; z < max.y; z++)
            {
                currentChunk.x = x;
                currentChunk.y = z;

                if(Vector2Int.Distance(currentChunk, currentChunk) < radius)
                {
                    chunkPositions.Add(currentChunk);
                }
            }
        }

        return chunkPositions.ToArray();
    }

    public Vector2Int WorldToChunk(Vector3 worldPos)
    {
        //Ex 67.39, 12.49, 123.69 --> 67/32 = >2 & <3 = 2, 123/32 = <4 & >3 = 3
        return new Vector2Int(Mathf.FloorToInt(worldPos.x / subCubeSize), Mathf.FloorToInt(worldPos.z/ subCubeSize));
    }

    public Vector3 ChunkToWorld(Vector2Int chunk) 
    {
        //Ex 2, 3 --> 2*32 = 64, 0, 96
        return new Vector3(chunk.x * subCubeSize, 0, chunk.y * subCubeSize);
    }

/*    private bool IsInRenderDistance(Vector2Int chunk)
    {
        //Check if the chunk is within the render distance of the player chunk
        return Vector2Int.Distance(playerChunk, chunk) <= renderDistance;
    }

    private void PopulateFrontierWithChunks()
    {
        //Populate the frontier with chunks within the render distance of the player chunk
        for (int x = -renderDistance; x <= renderDistance; x++)
        {
            for (int y = -renderDistance; y <= renderDistance; y++)
            {
                Vector2Int chunk = new Vector2Int(playerChunk.x + x, playerChunk.y + y);
                if (!visited.Contains(chunk) && IsInRenderDistance(chunk))
                {
                    frontier.Add(chunk);
                }
            }
        }
    }*/


    //Accessors
    public uint GetChunkSize() => subCubeSize;
    public uint GetChunkHeight() => subCubesPerChunk * subCubeSize;
    public uint GetSubCubesPerChunk() => subCubesPerChunk;
    public Vector2Int GetPlayerChunk() => playerChunk;
    public void SetSeed(float newSeed) => seed = newSeed;
}
