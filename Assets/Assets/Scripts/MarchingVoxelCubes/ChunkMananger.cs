using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkMananger : MonoBehaviour
{
    public static ChunkMananger Instance;
    [SerializeField] private MarchingAlgorithm marchingAlgorithmPrefab;
    [SerializeField] private uint chunkSize = 32;
    [Tooltip("How many cubes tall a chunk is")]
    [SerializeField] private uint subChunks = 16;
    [Tooltip("How many subchunks to generate per frame")]
    [SerializeField] private uint subChunksPerFrame = 4;    
    //[SerializeField] private MarchingAlgorithm algorithmPrefab;
    [SerializeField] private TerrainGeneration generator;
    private List<Vector2Int> frontier = new();
    private List<Vector2Int> visited = new();
    private Vector2Int playerChunk = Vector2Int.zero;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            frontier.Add(new Vector2Int(0, 0));
            ChunkRecursion();
        }
        else if (Instance != this)
        {
            Debug.LogError("Chunk Manager already exists in scene");
            Destroy(this);
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

    void ChunkRecursion()
    {
        Vector2Int current = frontier[0];
        visited.Add(current);

        if (true)
        {
            frontier.RemoveAt(0);
            StartCoroutine(GenerateChunk(current));
        }

        if (!visited.Contains(current + Vector2Int.left))
        {
            frontier.Add(current + Vector2Int.left);
            visited.Add(current + Vector2Int.left);
        }

        if (!visited.Contains(current + Vector2Int.right))
        {
            frontier.Add(current + Vector2Int.right);
            visited.Add(current + Vector2Int.right);
        }

        if (!visited.Contains(current + Vector2Int.up))
        {
            frontier.Add(current + Vector2Int.up);
            visited.Add(current + Vector2Int.up);
        }

        if (!visited.Contains(current + Vector2Int.down))
        {
            frontier.Add(current + Vector2Int.down);
            visited.Add(current + Vector2Int.down);
        }

        frontier.Sort(new DistanceCompare());
    }


    public IEnumerator GenerateChunk(Vector2Int chunk)
    {
        GameObject spawnedChunk = new GameObject();
        spawnedChunk.transform.parent = transform;
        for (int i = 0; i < subChunks; i++)
        {
            MarchingAlgorithm subChunk = Instantiate(marchingAlgorithmPrefab);
            subChunk.transform.parent = spawnedChunk.transform;
            subChunk.name = "SubChunk " + i;
            subChunk.transform.position = ChunkToWorld(chunk) + new Vector3(0, i * chunkSize, 0);
            subChunk.Init(chunk, i);
            subChunk.GenerateIsland();
            
            if(i % subChunksPerFrame == 0)
                yield return new WaitForEndOfFrame();
        }

        ChunkRecursion();
    }

    public Vector2Int WorldToChunk(Vector3 worldPos)
    {
        //Ex 67.39, 12.49, 123.69 --> 67/32 = >2 & <3 = 2, 123/32 = <4 & >3 = 3
        return new Vector2Int(Mathf.FloorToInt(worldPos.x / chunkSize), Mathf.FloorToInt(worldPos.z/ chunkSize));
    }

    public Vector3 ChunkToWorld(Vector2Int chunk) 
    {
        //Ex 2, 3 --> 2*32 = 64, 0, 96
        return new Vector3(chunk.x * chunkSize, 0, chunk.y * chunkSize);
    }

    //Accessors
    public uint GetChunkSize() => chunkSize;
    public uint GetChunkHeight() => subChunks * chunkSize;
    public Vector2Int GetPlayerChunk() => playerChunk;
    public TerrainGeneration GetGeneration() => generator;
}
