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
    [SerializeField] private List<ResourceGenerator> resourceGenerators = new();

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
                foreach (ResourceGenerator resource in resourceGenerators)
                {
                    resource.SpawnResource(ChunkMananger.Instance.ChunkToWorld(chunk) + new Vector3(x, 0, z));
                }
            }
        }
    }
}

[Serializable]
public struct WeightedPrefab
{
    public GameObject prefab;
    public int weight;
}