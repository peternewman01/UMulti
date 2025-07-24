using System;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;

public abstract class TerrainGeneration : MonoBehaviour
{
    [Serializable]
    protected enum PerlinMath
    {
        NONE,
        LINEAR,
        INVERSE_LINEAR,
        SIN,
        COS,
        TAN,
        SQRT,
        LOG,
    }

    [Serializable]
    protected struct PerlinMultipliers
    {
        //[Range(0, 1)] public float startRadiusPercent;
        //[Range(1, 0)] public float endRadiusPercent;
        public Vector3 values;
        public PerlinMath mathType;
    }
    
    protected Vector3 center;
    [SerializeField] protected List<PerlinMultipliers> perlinMultipliers = new();
    
    protected List<float> terrainAirGaps;
    protected int seed;

    private void Start()
    {
        seed = UnityEngine.Random.Range(-50000, 50000);
    }



    public abstract void CustomNoise(MarchingAlgorithm algorithm, Vector2Int pos);
    public float[] GetAirGaps() => terrainAirGaps.ToArray();
}

