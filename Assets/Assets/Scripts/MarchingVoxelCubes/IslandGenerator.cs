using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class IslandGenerator : TerrainGeneration
{
    [SerializeField] protected List<PerlinMultipliers> bottomPerlinMultipliers = new();

    [SerializeField] protected Material material;
    [SerializeField] private Vector3 islandCenter = Vector3.zero;
    [SerializeField] private float islandRadius = 64f;
    [Tooltip("Currently not implemented")]
    [UnityEngine.Range(0, 1)][SerializeField] private float edgeCrispness = 0.95f;

    

    public override void CustomNoise(MarchingAlgorithm algorithm, Vector2Int pos)
    {
        float radiusPercent = GetChunkRadiusPercent(algorithm.GetChunk());
        Vector3 offset = ChunkMananger.Instance.ChunkToWorld(algorithm.GetChunk());
        float distance = Vector3.Distance(center, new Vector2(offset.x, offset.z) + pos);
        float ratio = distance / islandRadius;
        if (ratio > radiusPercent) return;

        float value = 0;

        {
            foreach (PerlinMultipliers perlinMultiplier in bottomPerlinMultipliers)
            {
                value += CalcPerlin(perlinMultiplier, pos.x + (int)offset.x, pos.y + (int)offset.z, ratio);
            }
            terrainAirGaps.Add(ChunkMananger.Instance.GetChunkHeight() - value);
        }
        value = 0;
        {
            foreach (PerlinMultipliers perlinMultiplier in perlinMultipliers)
            {
                value += CalcPerlin(perlinMultiplier, pos.x + (int)offset.x, pos.y + (int)offset.z, ratio);
            }

            terrainAirGaps.Add(ChunkMananger.Instance.GetChunkHeight() + value);
        }
    }

    private float CalcPerlin(PerlinMultipliers multi, int x, int z, float distance)
    {
        float multiplier = 1f;
        float perlin = (Mathf.PerlinNoise((x + seed) * multi.values.x,
                    (z + seed) * multi.values.z) * multi.values.y);

        switch (multi.mathType)
        {
            case PerlinMath.NONE:
                break;
            case PerlinMath.LINEAR:
                multiplier = distance;
                break;
            case PerlinMath.SIN:
                multiplier = Mathf.Sin(Mathf.Clamp(distance, 0f, 1f) + 90f);
                break;
            case PerlinMath.COS:
                multiplier = Mathf.Cos(Mathf.Clamp(distance, 0f, 1f));
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
