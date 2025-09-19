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
    [Serializable]
    private struct Resource
    {
        public List<GameObject> resourcePrefab;
        public bool isNetworkObject;
        public FitnessData spawningData;
    }

    [SerializeField] private List<Resource> resourceGenerators = new();

    private void Start()
    {
        ChunkMananger.Instance.ChunkFinishedGenerating += OnChunkFinishUpdate;
    }

    private void OnDisable()
    {
        ChunkMananger.Instance.ChunkFinishedGenerating -= OnChunkFinishUpdate;
    }

    private void SpawnResource(Resource resource, Vector3 pos)
    {
        pos.y = ChunkMananger.Instance.GetChunkHeight() * ChunkMananger.Instance.GetSpacing();
        pos.x += Random.Range(-resource.spawningData.posOffset, resource.spawningData.posOffset);
        pos.z += Random.Range(-resource.spawningData.posOffset, resource.spawningData.posOffset);
        if (Physics.Raycast(new Ray(pos, Vector3.down), out RaycastHit hitData, 10000, ~LayerMask.NameToLayer("Ground")))
        {
            if (Mathf.Clamp01(ResourceFitness(pos.x, pos.z, hitData, resource.spawningData)) < resource.spawningData.density)
            {
                pos.y = hitData.point.y;
                GameObject spawned = null;
                int prefabIndex = Random.Range(0, resource.resourcePrefab.Count);
                if (resource.isNetworkObject)
                {
                    ServerFunctions.SpawnObjectServerRpc(resource.resourcePrefab[prefabIndex], out spawned);
                }
                else
                {
                    spawned = Instantiate(resource.resourcePrefab[prefabIndex], ChunkMananger.Instance.ResourceParent.transform);
                }

                spawned.transform.position = pos;
                spawned.transform.parent = ChunkMananger.Instance.ResourceParent.transform;
            }
        }
    }

    public void OnChunkFinishUpdate(Vector2Int chunk)
    {
        for (int x = 0; x < ChunkMananger.Instance.GetChunkSize(); x++)
        {
            for (int z = 0; z < ChunkMananger.Instance.GetChunkSize(); z++)
            {
                foreach (Resource resource in resourceGenerators)
                {
                    SpawnResource(resource, ChunkMananger.Instance.ChunkToWorld(chunk) + new Vector3(x, 0, z));
                }
            }
        }
    }

    private float ResourceFitness(float x, float z, RaycastHit hitData, FitnessData data)
    {
        float fitness = Mathf.PerlinNoise(x + ChunkMananger.Instance.Seed, z + ChunkMananger.Instance.Seed);
        //Debug.Log($"Perlin at ({x}, {z}) = {fitness}");
        fitness += Random.Range(-data.randomness, data.randomness);
        if (Mathf.Acos(Vector3.Dot(hitData.normal, Vector3.up)) > data.maxSteepness) fitness += 0.9f;
        if (Mathf.Acos(Vector3.Dot(hitData.normal, Vector3.up)) < data.minSteepness) fitness += 0.9f;
        if (hitData.point.y > data.maxHeight) fitness += 0.7f;
        if (hitData.point.y < data.minHeight) fitness += 0.7f;

        //Debug.Log($"ResourceFitness at ({x}, {z}) = {fitness}");
        return fitness;
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
