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
    /// <summary>
    /// Generates a list of points where the terrain should change from air to solid.
    /// O(n) complexity, where n is perlinMultipliers.Length
    /// </summary>
    /// <param name="algorithm"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
    public abstract float[] CustomNoise(Vector3 pos);
}

