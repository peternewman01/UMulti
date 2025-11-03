using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace UnityEngine
{
    [CreateAssetMenu(fileName = "New Resource", menuName = "Resource-Generation/Resource")]
    public class ResourceGenerator : ScriptableObject
    {
        [SerializeField] private List<WeightedPrefab> weightedPrefabs;
        [SerializeField] private bool isNetworkObject;
        [SerializeField] private FitnessData spawningData;
        private int totalWeight = 0;

        private void OnValidate()
        {
            totalWeight = 0;
            foreach (var weight in weightedPrefabs)
            {
                totalWeight += weight.weight;
            }
        }

        public void SpawnResource(Vector3 pos)
        {
            pos.y = ChunkMananger.Instance.GetChunkHeight() * ChunkMananger.Instance.GetSpacing();
            pos.x += UnityEngine.Random.Range(-spawningData.posOffset, spawningData.posOffset);
            pos.z += Random.Range(-spawningData.posOffset, spawningData.posOffset);
            if (Physics.Raycast(new Ray(pos, Vector3.down), out RaycastHit hitData, 10000, ~LayerMask.NameToLayer("Ground")))
            {
                if (ResourceFitness(pos.x, pos.z, hitData) > (1 - spawningData.density))
                {
                    pos.y = hitData.point.y;
                    GameObject spawned = null;
                    GameObject prefab = GetRandomPrefab();
                    if (isNetworkObject)
                    {
                        NetcodeConnector.SpawnObjectServerRpc(prefab.GetComponent<NetworkObject>(), out spawned);
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
            if (fitness < spawningData.floorValue) return 0f;
            //Debug.Log($"Perlin at ({x}, {z}) = {fitness}");
            fitness += Random.Range(-spawningData.randomness, spawningData.randomness);
            if (Mathf.Acos(Vector3.Dot(hitData.normal, Vector3.up)) > spawningData.maxSteepness) return 0f; ;
            if (Mathf.Acos(Vector3.Dot(hitData.normal, Vector3.up)) < spawningData.minSteepness) return 0f; ;
            if (hitData.point.y > spawningData.maxHeight) return 0f;
            if (hitData.point.y < spawningData.minHeight) return 0f;

            return Mathf.Pow(fitness, spawningData.powScaling);
        }

        public GameObject GetResourcePrefabByWeight(int weight)
        {
            if (weight < 0) return null;
            int curWeight = 0;

            foreach (var weightedResource in weightedPrefabs)
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
            [Range(0, 1)] public float floorValue;
            public float powScaling;
            [Range(0, 1)] public float density;
            [Range(0, 1)] public float posOffset;
            [Range(0, 1)] public float randomness;
            [Range(0, 90)] public float maxSteepness;
            [Range(0, 90)] public float minSteepness;
            public float maxHeight;
            public float minHeight;
        }
    }
}