using System;
using System.Collections.Generic;
using System.Globalization;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "IslandManager", menuName = "MarchingVoxelCubes/TerrainGeneration/IslandManager")]
public class IslandGeneratorManager : TerrainGeneration
{
    //[SerializeField] protected List<PerlinMultipliers> bottomPerlinMultipliers = new();

    /*    [SerializeField] protected Material material;
        [SerializeField] private float islandRadius = 64f;
        [Tooltip("Currently not implemented")]
        [Range(0, 1)][SerializeField] private float edgeCrispness = 0.95f;*/
    [SerializeField] private List<IslandGenerator> possibleGenerators = new();
    private List<IslandGenerator> islandGenerators = new();

    private void OnValidate()
    {
        if (possibleGenerators == null || possibleGenerators.Count == 0)
        {
            Debug.LogError("IslandGeneratorManager: No possible generators set. Please add at least one generator to the list.");
            return;
        }
        foreach (IslandGenerator generator in possibleGenerators)
        {
            if (generator == null)
            {
                Debug.LogError("IslandGeneratorManager: One of the possible generators is null. Please ensure all generators are assigned.");
                return;
            }
        }

        islandGenerators.Clear();
    }

    private IslandGenerator GetNearestIsland(Vector3 pos)
    {
        if (islandGenerators.Count == 0)
        {
            CreateNewGenerator();
        }

        float distanceToNearest = float.MaxValue;
        IslandGenerator islandGenerator = null;
        foreach (IslandGenerator island in islandGenerators)
        {
            if (island == null) continue;
            Vector3 center = island.GetCenter();
            float distance = Vector3.Distance(center, pos);
            if (distance < distanceToNearest)
            {
                distanceToNearest = distance;
                islandGenerator = island;
            }
        }

        return islandGenerator;
    }

    public override float[] CustomNoise(Vector2Int chunk, Vector2Int subPos)
    {
        return GetNearestIsland(ChunkMananger.Instance.ChunkToWorld(chunk) + new Vector3(subPos.x, 0, subPos.y)).CustomNoise(chunk, subPos);
    }

    public void CreateNewGenerator()
    {
        IslandGenerator newGenerator = Instantiate(possibleGenerators[0]);
        newGenerator.SetCenter(FindViableCenter(newGenerator));
        islandGenerators.Add(newGenerator);
    }

    private Vector3 FindViableCenter(IslandGenerator newGenerator)
    {
        Vector3 center = Vector3.zero;
        float newGeneratorMaxDistance = newGenerator.GetIslandMaxDistanceFromCenter();
        foreach (IslandGenerator island in islandGenerators)
        {
            if (island == null) continue;
            if (island.IsInsideIsland(center))
            {
                center = island.GetCenter();
                Vector2 newPos = UnityEngine.Random.insideUnitCircle.normalized * 
                    (island.GetIslandMaxDistanceFromCenter() + newGeneratorMaxDistance);
                center.x += newPos.x;
                center.z += newPos.y;
            }
        }
        Debug.Log($"IslandGeneratorManager: New generator center set to {center} with max distance {newGeneratorMaxDistance}");
        return center;
    }

    /*private float CalcPerlin(PerlinMultipliers multi, int x, int z, float distance)
    {
        float multiplier = 1f;
        float perlin = Mathf.PerlinNoise((x * multi.values.x)*//* + seed*//*,
                    (z * multi.values.z) *//* + seed*//*) * multi.values.y;

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
    }*/

    /*    public float GetChunkRadiusPercent(Vector2Int chunk)
        {
            if (edgeCrispness == 1) return 1;
            float perlinEffect = Mathf.PerlinNoise(chunk.x, chunk.y);
            float inversCrisp = 1 - edgeCrispness;
            return 1 - (inversCrisp * perlinEffect);
        }*/
}
