using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "IslandManager", menuName = "MarchingVoxelCubes/TerrainGeneration/IslandManager")]
public class IslandGeneratorManager : TerrainGeneration
{
    //[SerializeField] private float islandSpacing = 64f;
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

    public override float[] CustomNoise(Vector3 pos)
    {
        return GetNearestIsland(pos).CustomNoise(pos);
    }

    public void CreateNewGenerator(int index = -1)
    {
        if (index == -1) { index = UnityEngine.Random.Range(0, possibleGenerators.Count); }
        IslandGenerator newGenerator = Instantiate(possibleGenerators[index]);
        newGenerator.Init();
        newGenerator.SetCenter(FindViableCenter(newGenerator));
        islandGenerators.Add(newGenerator);
    }

    private Vector3 FindViableCenter(IslandGenerator newGenerator)
    {
        Vector3 center = Vector3.zero;
        float newGeneratorMaxDistance = newGenerator.GetIslandMaxDistanceFromCenter();
        float spacing = newGenerator.GetSpacing();
        IslandGenerator nearestIsland = GetNearestIsland(center);
        if(nearestIsland == null) return center;

        while (Vector3.Distance(nearestIsland.GetCenter(), center) < (nearestIsland.GetIslandMaxDistanceFromCenter() + newGeneratorMaxDistance + spacing))
        {
            center = nearestIsland.GetCenter();
            Vector2 newPos = UnityEngine.Random.insideUnitCircle.normalized *
                (nearestIsland.GetIslandMaxDistanceFromCenter() + newGeneratorMaxDistance + spacing);
            center.x += newPos.x;
            center.z += newPos.y;
            nearestIsland = GetNearestIsland(center);

            //if (nearestIsland == newGenerator) break;
        }

        Debug.Log($"IslandGeneratorManager: New generator center set to {center} with max distance {newGeneratorMaxDistance}");
        return center;
    }
}
