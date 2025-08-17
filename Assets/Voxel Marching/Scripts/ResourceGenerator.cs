using System;
using System.Collections.Generic;
using System.Globalization;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "NewResourceGenerator", menuName = "MarchingVoxelCubes/TerrainGeneration/ResourceGenerator")]
public class ResourceGenerator : TerrainGeneration
{
    [SerializeField] private GameObject resourcePrefab;
    [SerializeField] private FitnessData spawningData;

    public override float[] CustomNoise(Vector3 pos)
    {
        return null;
    }

    public void GenerateResource(Vector3 pos)
    {
        pos.y = 1000000;
        //pos.x += Random.Range(-spawningData.posOffset, spawningData.posOffset);
        //pos.z += Random.Range(-spawningData.posOffset, spawningData.posOffset);
        if (Physics.Raycast(new Ray(pos, Vector3.down), out RaycastHit hitData, float.MaxValue))
        {
            if (ResourceFitness(pos.x, pos.z, hitData, spawningData) < spawningData.density)
            {
                pos.y = hitData.point.y;
                GameObject obj = Instantiate(resourcePrefab, pos, Quaternion.identity, ChunkMananger.Instance.transform);
            }
        }
    }

    private float ResourceFitness(float x, float z, RaycastHit hitData, FitnessData data)
    {
        float fitness = Mathf.PerlinNoise(x, z);
        fitness += Random.Range(-data.randomness, data.randomness);
        if (Mathf.Acos(Vector3.Dot(hitData.normal, Vector3.up)) > data.maxSteepness) fitness += 0.9f;
        if (Mathf.Acos(Vector3.Dot(hitData.normal, Vector3.up)) < data.minSteepness) fitness += 0.9f;
        if (hitData.point.y > data.maxHeight) fitness += 0.7f;
        if (hitData.point.y < data.minHeight) fitness += 0.7f;

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
