using System;
using UnityEngine;
using TerrainGen;

public interface ITerrainGeneration
{

    /// <summary>
    /// Generates a list of points where the terrain should change from air to solid.
    /// O(n^2) complexity, where n is perlinMultipliers.Length
    /// </summary>
    /// <param name="algorithm"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
    public float[] CustomNoise(Vector3 pos, MarchingAlgorithm algorithm);
}

namespace TerrainGen
{
    [Serializable]
    public enum PerlinMath
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
    public struct PerlinMultipliers
    {
        //[Range(0, 1)] public float startRadiusPercent;
        //[Range(1, 0)] public float endRadiusPercent;
        public Vector3 values;
        public PerlinMath mathType;
    }
}
