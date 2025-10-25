using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "IslandManager", menuName = "MarchingVoxelCubes/TerrainGeneration/IslandManager")]
public class IslandGeneratorManager : TerrainGeneration
{
    [Serializable]
    private struct WeightedGenerator
    {
        [Tooltip("Higher == More likely")]
        public int weight;
        public IslandGenerator generator;
    }

    //[SerializeField] private float islandSpacing = 64f;
    [SerializeField] private List<WeightedGenerator> possibleGenerators = new();
    //[SerializeField] private HeightMaterials[] possibleMaterials;
    [SerializeField] private uint islandsToGenerate = 16;
    private List<IslandGenerator> instancedGenerators = new();
    private int totalWeight;

    private void OnValidate()
    {
        totalWeight = 0;
        if (possibleGenerators == null || possibleGenerators.Count == 0)
        {
            Debug.LogError("IslandGeneratorManager: No possible generators set. Please add at least one generator to the list.");
            return;
        }
        foreach (WeightedGenerator generator in possibleGenerators)
        {
            if (generator.generator == null)
            {
                Debug.LogError("IslandGeneratorManager: One of the possible generators is null. Please ensure all generators are assigned.");
                return;
            }
            totalWeight += generator.weight;
        }

        instancedGenerators.Clear();
    }

    public IslandGenerator[] GetAllIslandsAtPosition(Vector3 pos)
    {
        List<IslandGenerator> generators = new();
        if (instancedGenerators.Count <= 0)
        {
            CreateInitialGenerators();
        }

        foreach (IslandGenerator island in instancedGenerators)
        {
            if (island.IsInsideIsland(pos)) generators.Add(island);
        }
        return generators.ToArray();
    }

    private IslandGenerator GetNearestIsland(Vector3 pos)
    {
        float distanceToNearest = float.MaxValue;
        IslandGenerator islandGenerator = null;
        foreach (IslandGenerator island in instancedGenerators)
        {
            if (island == null) continue;
            if (island.IsInsideIsland(pos)) return island; //Being inside an island is more important than being "close"

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

    public override float[] CustomNoise(Vector3 pos, MarchingAlgorithm algorithm)
    {
        List<float> values = new();

        foreach(IslandGenerator generator in GetAllIslandsAtPosition(pos))
        {
            values.AddRange( generator.CustomNoise(pos, algorithm));
        }
        return /*values.Count > 0 ? */values.ToArray()/* : GetNearestIsland(pos).CustomNoise(pos, algorithm)*/;
    }

    public void CreateInitialGenerators()
    {
        for(int i = 0; i < islandsToGenerate; i++)
        {
            if (i == 0) CreateNewGenerator(0); //first generator should be of the first type in the weighted list
            else CreateNewGenerator();
        }
    }

    public void CreateNewGenerator(int index = -1)
    {
        IslandGenerator newGenerator;

        if (index <= -1 || index >= possibleGenerators.Count) { 
            int weight = UnityEngine.Random.Range(1, totalWeight);
            newGenerator = Instantiate(GetGeneratorByWeight(weight));
        } else
        {
            newGenerator = Instantiate(possibleGenerators[index].generator);
        }

        newGenerator.Init();
        newGenerator.SetCenter(FindViableCenter(newGenerator));
        instancedGenerators.Add(newGenerator);

        //Populates frontier in chunk manager to only contain chunks that matter to
        ChunkMananger.Instance.AddChunksInRadiusAtPosition(newGenerator.GetCenter(), newGenerator.GetIslandMaxDistanceFromCenter() * 1.5f);
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

        //Debug.Log($"IslandGeneratorManager: New generator center set to {center} with max distance {newGeneratorMaxDistance}");
        return center;
    }

    private IslandGenerator GetGeneratorByWeight(int weight)
    {
        int index = -1;
        for (int total = 0; total < weight; total += possibleGenerators[index].weight)
        {
            index++;
        }

        return possibleGenerators[index].generator;
    }
}
