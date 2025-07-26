using System;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;


public abstract class TerrainGeneration : ScriptableObject
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
    
    protected Vector3 center = Vector3.zero;
    [SerializeField] protected List<PerlinMultipliers> perlinMultipliers = new();
    protected int seed;

    public void SetSeed(int newSeed = int.MinValue)
    {
        if (newSeed != int.MinValue)
        {
            seed = newSeed;
            return;
        }

        seed = UnityEngine.Random.Range(int.MinValue / 2, int.MaxValue / 2);
    }



    public abstract float[] CustomNoise(MarchingAlgorithm algorithm, Vector2Int pos);
}

