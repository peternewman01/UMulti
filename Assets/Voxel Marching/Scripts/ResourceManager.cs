using Palmmedia.ReportGenerator.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using Unity.Cinemachine;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class ResourceManager : MonoBehaviour
{ 
    [SerializeField] private List<Resource> resourceGenerators = new();

    private void Start()
    {
        ChunkMananger.Instance.ChunkFinishedGenerating += OnChunkFinishUpdate;
    }

    private void OnDisable()
    {
        ChunkMananger.Instance.ChunkFinishedGenerating -= OnChunkFinishUpdate;
    }

    public void OnChunkFinishUpdate(Vector2Int chunk)
    {
        for (int x = 0; x < ChunkMananger.Instance.GetChunkSize(); x++)
        {
            for (int z = 0; z < ChunkMananger.Instance.GetChunkSize(); z++)
            {
                foreach (Resource resource in resourceGenerators)
                {
                    resource.SpawnResource(ChunkMananger.Instance.ChunkToWorld(chunk) + new Vector3(x, 0, z));
                }
            }
        }
    }
}

[CreateAssetMenu(fileName = "New Resource", menuName = "Resource-Generation/Resource")]
public class Resource : ScriptableObject
{
    [SerializeField] private List<WeightedPrefab> weightedPrefabs;
    [SerializeField] private bool isNetworkObject;
    [SerializeField] private FitnessData spawningData;
    private int totalWeight = 0;

    private void OnValidate()
    {
        totalWeight = 0;
        foreach(var weight in weightedPrefabs)
        {
            totalWeight += weight.weight;
        }
    }

    public void SpawnResource(Vector3 pos)
    {
        pos.y = ChunkMananger.Instance.GetChunkHeight() * ChunkMananger.Instance.GetSpacing();
        pos.x += Random.Range(-spawningData.posOffset, spawningData.posOffset);
        pos.z += Random.Range(-spawningData.posOffset, spawningData.posOffset);
        if (Physics.Raycast(new Ray(pos, Vector3.down), out RaycastHit hitData, 10000, ~LayerMask.NameToLayer("Ground")))
        {
            if (Mathf.Clamp01(ResourceFitness(pos.x, pos.z, hitData)) < spawningData.density)
            {
                pos.y = hitData.point.y;
                GameObject spawned = null;
                GameObject prefab = GetRandomPrefab();
                if (isNetworkObject)
                {
                    ServerFunctions.SpawnObjectServerRpc(prefab, out spawned);
                }
                else
                {
                    spawned = Instantiate(prefab, ChunkMananger.Instance.ResourceParent.transform);
                }

                spawned.transform.position = pos;
                spawned.transform.parent = ChunkMananger.Instance.ResourceParent.transform;
            }
        }
    }

    private float ResourceFitness(float x, float z, RaycastHit hitData)
    {
        float fitness = Mathf.PerlinNoise(x + ChunkMananger.Instance.Seed, z + ChunkMananger.Instance.Seed);
        //Debug.Log($"Perlin at ({x}, {z}) = {fitness}");
        fitness += Random.Range(-spawningData.randomness, spawningData.randomness);
        if (Mathf.Acos(Vector3.Dot(hitData.normal, Vector3.up)) > spawningData.maxSteepness) fitness += 0.9f;
        if (Mathf.Acos(Vector3.Dot(hitData.normal, Vector3.up)) < spawningData.minSteepness) fitness += 0.9f;
        if (hitData.point.y > spawningData.maxHeight) fitness += 0.7f;
        if (hitData.point.y < spawningData.minHeight) fitness += 0.7f;

        //Debug.Log($"ResourceFitness at ({x}, {z}) = {fitness}");
        return fitness;
    }

    public GameObject GetResourcePrefabByWeight(int weight)
    {
        if (weight < 0) return null;
        int curWeight = 0;

        foreach(var weightedResource in weightedPrefabs)
        {
            curWeight += weightedResource.weight;
            if (curWeight > weight)
                return weightedResource.prefab;
        }

        return null;
    }

    public int GetWeightByPercent(float percent)
    {
        percent = Mathf.Clamp01(percent);
        return (int)(totalWeight * percent);
    }

    public GameObject GetRandomPrefab()
    {
        return GetResourcePrefabByWeight(Random.Range(0, totalWeight));
    }

    [Serializable]
    private struct FitnessData
    {
        [Range(0, 1)] public float density;
        [Range(0, 1)] public float posOffset;
        [Range(0, 1)] public float randomness;
        [Range(0, 90)] public float maxSteepness;
        [Range(0, 90)] public float minSteepness;
        public float maxHeight;
        public float minHeight;
    }
}

[Serializable]
public struct WeightedPrefab
{
    public GameObject prefab;
    public int weight;
}