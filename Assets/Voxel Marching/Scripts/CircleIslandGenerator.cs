using System.Collections.Generic;
using System.Globalization;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "NewIslandGeneration", menuName = "MarchingVoxelCubes/TerrainGeneration/CircleIsland")]
public class CircleIslandGenerator : IslandGenerator
{
    [SerializeField] private float islandRadius = 64f;
    [SerializeField] private float radiusFluxuation = 0f; // How much the radius can fluctuate, 0 means no fluctuation
    [Tooltip("Currently not implemented")]
    //[Range(0, 1)][SerializeField] private float edgeCrispness = 0.95f;

    public override void Init()
    {
        base.Init();
        islandRadius += Random.Range(-radiusFluxuation, radiusFluxuation);
    }

    public override float[] CustomNoise(Vector3 pos, MarchingAlgorithm algorithm)
    {   
        //float radiusPercent = GetChunkRadiusPercent(chunk);
        //Vector3 offset = ChunkMananger.Instance.ChunkToWorld(chunk);
        Vector3 worldPos = pos;
        if (!IsInsideIsland(worldPos))
        {
            return null;
        }

        float distance = Vector3.Distance(center, worldPos);
        float halfHeight = ChunkMananger.Instance.GetChunkHeight() /2;
        halfHeight += yFluxuation; // Add random fluctuation to the height

        float[] value = { halfHeight, halfHeight };

        foreach (PerlinMultipliers perlinMultiplier in bottomPerlinMultipliers)
        {
            value[0] -= CalcPerlin(perlinMultiplier, worldPos.x, worldPos.z, distance);
        }

        foreach (PerlinMultipliers perlinMultiplier in perlinMultipliers)
        {
            value[1] += CalcPerlin(perlinMultiplier, worldPos.x, worldPos.z, distance);
        }
        //Debug.Log($"values for chunk({chunk.x}, {chunk.y})-subPos({subPos.x}, {subPos.y}): (min:{value[0]}, max:{value[1]})");
        return value;
    }

    private float CalcPerlin(PerlinMultipliers multi, float x, float z, float distance)
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
                multiplier = 1 / (distance + float.Epsilon);
                break;
            default:
                break;
        }

        return perlin * multiplier;
    }

/*    public float GetChunkRadiusPercent(Vector2Int chunk)
    {
        if (edgeCrispness == 1) return 1;
        float perlinEffect = Mathf.PerlinNoise(chunk.x, chunk.y);


        float inversCrisp = 1 - edgeCrispness;
        return 1 - (inversCrisp * perlinEffect);
    }*/

    private float GetRadiusPercent(Vector3 pos)
    {
        float distance = Vector3.Distance(center, pos);
        return distance / islandRadius;
    }

    public override float GetIslandMaxDistanceFromCenter() => islandRadius;

    public override bool IsInsideIsland(Vector3 pos)
    {
        return GetRadiusPercent(pos) <= 1;
    }

    public override bool IsInsideIsland(Vector2Int chunk)
    {
        float distance = Vector2Int.Distance(chunk, ChunkMananger.Instance.WorldToChunk(center));
        return distance < (islandRadius / ChunkMananger.Instance.GetChunkSize());
    }
}
