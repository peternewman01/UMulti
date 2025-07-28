using System.Collections.Generic;
using System.Globalization;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "NewIslandGeneration", menuName = "MarchingVoxelCubes/TerrainGeneration/Island")]
public class IslandGenerator : TerrainGeneration
{
    [SerializeField] protected List<PerlinMultipliers> bottomPerlinMultipliers = new();

    [SerializeField] protected Material material;
    [SerializeField] private float islandRadius = 64f;
    [Tooltip("Currently not implemented")]
    [Range(0, 1)][SerializeField] private float edgeCrispness = 0.95f;

    public override float[] CustomNoise(MarchingAlgorithm algorithm, Vector2Int subPos)
    {
        TrySetSeed(Random.Range(float.MinValue / 2, float.MaxValue / 2));
        
        float radiusPercent = GetChunkRadiusPercent(algorithm.GetChunk());
        Vector3 offset = ChunkMananger.Instance.ChunkToWorld(algorithm.GetChunk());
        Vector2Int worldPos = new Vector2Int((int)offset.x, (int)offset.z) + subPos;
        float distance = Vector2.Distance(center, worldPos);
        float ratio = distance / islandRadius;
        if (ratio > radiusPercent) return null;

        float halfHeight = ChunkMananger.Instance.GetChunkHeight() /2;

        float[] value = { halfHeight, halfHeight };

        foreach (PerlinMultipliers perlinMultiplier in bottomPerlinMultipliers)
        {
            value[0] -= CalcPerlin(perlinMultiplier, worldPos.x, worldPos.y, distance);
        }

        foreach (PerlinMultipliers perlinMultiplier in perlinMultipliers)
        {
            value[1] += CalcPerlin(perlinMultiplier, worldPos.x, worldPos.y, distance);
        }
        Debug.Log($"values for chunk({algorithm.GetChunk().x}, {algorithm.GetChunk().y})-subPos({subPos.x}, {subPos.y}): (min:{value[0]}, max:{value[1]})");
        return value;
    }

    private float CalcPerlin(PerlinMultipliers multi, int x, int z, float distance)
    {
        float multiplier = 1f;
        float perlin = Mathf.PerlinNoise((x + seed) * multi.values.x,
                    (z + seed) * multi.values.z) * multi.values.y;

        switch (multi.mathType)
        {
            case PerlinMath.NONE:
                break;
            case PerlinMath.LINEAR:
                multiplier = distance;
                break;
            case PerlinMath.SIN:
                multiplier = Mathf.Sin(Mathf.Clamp(distance / islandRadius, 0f, 1f) + 90f);
                break;
            case PerlinMath.COS:
                multiplier = Mathf.Cos(Mathf.Clamp(distance / islandRadius, 0f, 1f));
                break;
            case PerlinMath.TAN:
                multiplier = Mathf.Tan(Mathf.Clamp(distance, 0f, 1f));
                break;
            case PerlinMath.SQRT:
                multiplier = Mathf.Sqrt(distance);
                break;
            case PerlinMath.LOG:
                multiplier = Mathf.Log(distance);
                break;
            case PerlinMath.INVERSE_LINEAR:
                multiplier = 1 / distance;
                break;
            default:
                break;
        }

        return perlin * multiplier;
    }

    public float GetChunkRadiusPercent(Vector2Int chunk)
    {
        if (edgeCrispness == 1) return 1;
        float perlinEffect = Mathf.PerlinNoise(chunk.x, chunk.y);
        float inversCrisp = 1 - edgeCrispness;
        return 1 - (inversCrisp * perlinEffect);
    }

    public Material GetMaterial() => material;

}
