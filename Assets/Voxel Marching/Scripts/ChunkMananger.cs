using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ChunkMananger : NetworkBehaviour
{
    public static ChunkMananger Instance;
    [SerializeField] private MarchingAlgorithm marchingAlgorithmPrefab;

    [Tooltip("How large a sub-cube is for chunk generation")]
    [SerializeField] private uint subCubeSize = 32;
    [Tooltip("How many sub-cubes tall a chunk is")]
    [SerializeField] private uint subCubesPerChunk = 16;
    [Tooltip("How many sub-cubes to generate per frame")]
    [SerializeField] private uint chunksPerFrame = 4;
    [SerializeField][UnityEngine.Min(0.01f)] private float cubeSpacing = 0.5f;
    [SerializeField][UnityEngine.Range(0f, 90f)] private float stepHeightAngle = 45f;
    private NetworkObject resourceParent;
    private NetworkObject chunkParent;
    //[SerializeField] private MarchingAlgorithm algorithmPrefab;
    private List<Vector2Int> frontier = new();
    private Vector2Int playerChunk = Vector2Int.zero;
    private NetworkVariable<float> seed = new(0);


    public Action<Vector2Int> ChunkFinishedGenerating;

    private Vector2Int[] INITIAL_CHUNKS = { Vector2Int.zero, Vector2Int.down, Vector2Int.left, Vector2Int.right, Vector2Int.up, 
        Vector2Int.down + Vector2Int.left, Vector2Int.down + Vector2Int.right, Vector2Int.up + Vector2Int.left, Vector2Int.up + Vector2Int.right};

    public float Seed
    {
        get => seed.Value;
        set { }
    }

    public NetworkObject ResourceParent
    {
        get => resourceParent;
        set { }
    }

    public NetworkObject ChunkParent
    {
        get => chunkParent;
        set { }
    }

    //Events
    public Action<Vector2Int> PlayerChunkUpdated;

    private void OnValidate()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            DestroyImmediate(gameObject);
        }
    }

    private void Start()
    {
        stepHeightAngle = Mathf.Tan(Mathf.Deg2Rad * stepHeightAngle);
        
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        foreach (Transform child in transform)
            Destroy(child.gameObject);

        chunkParent = new GameObject("Chunks").AddComponent<NetworkObject>();
        chunkParent.Spawn();
        chunkParent.transform.parent = transform;

        resourceParent = new GameObject("Resources").AddComponent<NetworkObject>();
        resourceParent.Spawn();
        resourceParent.transform.parent = transform;

        frontier.AddRange(INITIAL_CHUNKS);
        SetSeed(UnityEngine.Random.Range(-10000, 10000));
        NetworkManager.Singleton.OnServerStarted += StartChunkLoadingServerRpc;
    }


    //TEMP
    private void Update()
    {
        if(FindFirstObjectByType<PlayerManager>())
        {
            Vector2Int player = WorldToChunk(FindFirstObjectByType<PlayerManager>().transform.position);
            if (playerChunk != player)
            {
                playerChunk = WorldToChunk(FindFirstObjectByType<PlayerManager>().transform.position);
                PlayerChunkUpdated?.Invoke(playerChunk);
            }

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

    [ServerRpc()]
    private void StartChunkLoadingServerRpc()
    {
        StartCoroutine(ChunkRecursion());
        PlayerChunkUpdated?.Invoke(playerChunk);
    }

    private IEnumerator ChunkRecursion()
    {
        yield return new WaitForEndOfFrame();
        StartCoroutine(ChunkRecursion());
        frontier.Sort(new DistanceCompare());
        uint cubeCount = 0;
        List<Vector2Int> generatedChunks = new();
        while (frontier.Count > 0 && cubeCount < chunksPerFrame * subCubesPerChunk)
        {
            Vector2Int current = frontier[0];
            cubeCount += GenerateChunk(current);
            frontier.RemoveAt(0);
            generatedChunks.Add(current);
        }
        StartCoroutine(FrameDelayFinishGeneration(generatedChunks));
        float fps = 1.0f / Time.deltaTime;



        if (chunksPerFrame > 1 && fps < 30)
            chunksPerFrame--;
        else if (fps > 90)
            chunksPerFrame++;
    }

    private IEnumerator FrameDelayFinishGeneration(List<Vector2Int> chunks)
    {
        yield return new WaitForEndOfFrame();
        foreach (var chunk in chunks)
        {
            ChunkFinishedGenerating?.Invoke(chunk);
        }
    }

    public uint GenerateChunk(Vector2Int chunk)
    {
        Destroy(GameObject.Find(GetChunkName(chunk)));
        GameObject spawnedChunk = new GameObject();
        spawnedChunk.AddComponent<NetworkObject>().Spawn();
        spawnedChunk.transform.parent = chunkParent.transform;
        spawnedChunk.name = GetChunkName(chunk);

        for (uint i = 0; i < subCubesPerChunk; i++)
        {
            MarchingAlgorithm subChunk = Instantiate(marchingAlgorithmPrefab);
            subChunk.name = GetChunkName(chunk) + "--SubChunk " + i;
            subChunk.InitChunkData(chunk, i);
            subChunk.GetComponent<NetworkObject>().Spawn();
            subChunk.transform.parent = spawnedChunk.transform;
            subChunk.transform.position = (ChunkToWorld(chunk) + new Vector3(0, i * subCubeSize, 0) * stepHeightAngle) * cubeSpacing;
/*            if(IsOwner && subChunk.IsSpawned)
                subChunk.GenerateIslandRpc();*/
        }

        return subCubesPerChunk;
    }

    public string GetChunkName(Vector2Int chunk)
    {
        return $"Chunk ({ chunk.x}, { chunk.y})";
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
        return new Vector2Int(Mathf.FloorToInt(worldPos.x / (subCubeSize)), Mathf.FloorToInt(worldPos.z / (subCubeSize)));
    }

    public Vector3 ChunkToWorld(Vector2Int chunk) 
    {
        //Ex 2, 3 --> 2*32 = 64, 0, 96
        return new Vector3(chunk.x * subCubeSize, 0, chunk.y * subCubeSize);
    }

    //Accessors
    public uint GetChunkSize() => subCubeSize;
    public uint GetChunkHeight() => subCubesPerChunk * subCubeSize;
    public uint GetSubCubesPerChunk() => subCubesPerChunk;
    public Vector2Int GetPlayerChunk() => playerChunk;
    public float GetSpacing() => cubeSpacing;
    public float GetStepHeight() => stepHeightAngle;
    private void SetSeed(float newSeed) => seed.Value = newSeed;

    public MarchingAlgorithm GetMarchingAlgorithm(Vector2Int chunk, int subChunk)
    {
        string name = GetChunkName(chunk);
        GameObject chunkObj = GameObject.Find(name);
        return chunkObj.transform.GetChild(subChunk).GetComponent<MarchingAlgorithm>();
    }

}

public class ServerFunctions : NetworkManager
{
    [ServerRpc(RequireOwnership = false)]
    public static void SpawnObjectServerRpc(MonoBehaviour prefab, out GameObject spawned)
    {
        if (prefab.GetComponent<NetworkObject>() == null) spawned = null;

        spawned = Instantiate(prefab.gameObject);
        spawned.GetComponent<NetworkObject>().Spawn();
    }

    [ServerRpc(RequireOwnership = false)]
    public static void SpawnObjectServerRpc(GameObject prefab, out GameObject spawned)
    {
        if (prefab.GetComponent<NetworkObject>() == null) spawned = null;

        spawned = Instantiate(prefab);
        spawned.GetComponent<NetworkObject>().Spawn();
    }

    [ServerRpc(RequireOwnership = false)]
    public static void SpawnObjectServerRpc(GameObject prefab)
    {
        if (prefab.GetComponent<NetworkObject>() == null) return;

        GameObject spawned = Instantiate(prefab);
        spawned.GetComponent<NetworkObject>().Spawn();
    }
}