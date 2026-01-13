using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TerrainGen;

[CreateAssetMenu(fileName = "FlamewasteGenerationManager", menuName = "MarchingVoxelCubes/TerrainGeneration/FlamewasteGenManager")]
public class FlamewasteGeneratorManager : ITerrainGeneration
{
    [SerializeField] private uint baseHeight = 32;
    [SerializeField] private float powerScaling = 1.2f;
    [SerializeField] private List<PerlinMultipliers> terrainMultipliers;
    [SerializeField] private uint chunksPerLoadingZone = 4; //practically a chunk radius
    static private List<Vector2Int> superChunksAdded = new();
    private bool needsToInitilize = true;

    private void Awake()
    {

    }

    private void OnDisable()
    {
        ChunkMananger.Instance.PlayerChunkUpdated -= GenerateChunksCloseToPlayer;
    }

    private void OnValidate()
    {
        superChunksAdded.Clear();
        needsToInitilize = true;
        if (ChunkMananger.Instance == null) return;
        ChunkMananger.Instance.PlayerChunkUpdated -= GenerateChunksCloseToPlayer;
    }

    private void Init()
    {
        needsToInitilize = false;
        ChunkMananger.Instance.PlayerChunkUpdated += GenerateChunksCloseToPlayer;
    }

    public void GenerateChunksCloseToPlayer(Vector2Int playerChunk)
    {
        Vector2Int superChunk = ChunkToSuperChunk(playerChunk);
        if (!superChunksAdded.Contains(superChunk)) QueueChunksFromSuperchunk(superChunk);

        //Check Neighbors
        if (!superChunksAdded.Contains(superChunk + Vector2Int.down)) QueueChunksFromSuperchunk(superChunk + Vector2Int.down);
        if (!superChunksAdded.Contains(superChunk + Vector2Int.down + Vector2Int.left)) QueueChunksFromSuperchunk(superChunk + Vector2Int.down + Vector2Int.left);
        if (!superChunksAdded.Contains(superChunk + Vector2Int.left)) QueueChunksFromSuperchunk(superChunk + Vector2Int.left);
        if (!superChunksAdded.Contains(superChunk + Vector2Int.left + Vector2Int.up)) QueueChunksFromSuperchunk(superChunk + Vector2Int.left + Vector2Int.up);
        if (!superChunksAdded.Contains(superChunk + Vector2Int.up)) QueueChunksFromSuperchunk(superChunk + Vector2Int.up);
        if (!superChunksAdded.Contains(superChunk + Vector2Int.up + Vector2Int.right)) QueueChunksFromSuperchunk(superChunk + Vector2Int.up + Vector2Int.right);
        if (!superChunksAdded.Contains(superChunk + Vector2Int.right)) QueueChunksFromSuperchunk(superChunk + Vector2Int.right);
        if (!superChunksAdded.Contains(superChunk + Vector2Int.right + Vector2Int.down)) QueueChunksFromSuperchunk(superChunk + Vector2Int.right + Vector2Int.down);
    }

    private Vector2Int ChunkToSuperChunk(Vector2Int chunk)
    {
        return new Vector2Int(chunk.x / (int)chunksPerLoadingZone, chunk.y / (int)chunksPerLoadingZone);
    }

    private void QueueChunksFromSuperchunk(Vector2Int superChunk)
    {
        superChunksAdded.Add(superChunk);

        List<Vector2Int> chunks = new();
        Vector2Int firstChunk = superChunk * (int)chunksPerLoadingZone;
        for (int x = firstChunk.x; x < firstChunk.x + chunksPerLoadingZone; x++)
        {
            for (int z = firstChunk.y; z < firstChunk.y + chunksPerLoadingZone; z++)
            {
                chunks.Add(new(x, z));
            }
        }

        ChunkMananger.Instance.AddChunks(chunks.ToArray());
    }

    public float[] CustomNoise(Vector3 pos, MarchingAlgorithm algorithm)
    {
        if(needsToInitilize) Init();

        List<float> values = new() { 0f };
        foreach(PerlinMultipliers multiplier in terrainMultipliers)
        {
            values[0] += CalcPerlin(multiplier, pos.x, pos.z);
        }
        values[0] = Mathf.Pow(values[0], powerScaling);
        values[0] += baseHeight;
        return values.ToArray();
    }

    private float CalcPerlin(PerlinMultipliers multi, float x, float z)
    {
        float seed = ChunkMananger.Instance.Seed;
        float multiplier = 1f;
        float perlin = Mathf.PerlinNoise((x * multi.values.x) + seed,
                    (z * multi.values.z) + seed) * multi.values.y;

        switch (multi.mathType)
        {
            case PerlinMath.NONE:
                break;
            case PerlinMath.LINEAR:
                multiplier = x / (z + float.Epsilon);
                break;
            case PerlinMath.SIN:
                multiplier = Mathf.Sin(x + z);
                break;
            case PerlinMath.COS:
                multiplier = Mathf.Cos(x + z);
                break;
            case PerlinMath.TAN:
                multiplier = Mathf.Tan(x + z);
                break;
            case PerlinMath.SQRT:
                multiplier = Mathf.Sqrt(x + z);
                break;
            case PerlinMath.LOG:
                multiplier = Mathf.Log(x + z);
                break;
            case PerlinMath.INVERSE_LINEAR:
                multiplier = 1 / (x / (z + float.Epsilon));
                break;
            default:
                break;
        }

        return perlin * multiplier;
    }
}
