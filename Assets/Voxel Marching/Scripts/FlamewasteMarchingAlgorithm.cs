using Palmmedia.ReportGenerator.Core;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class FlamewasteMarchingAlgorithm : TerrainGenerationMarching
{
    protected override void ProcessTerrainData(int x, int z, float[] yData, float[,,] terrainData, int subChunk, int voxelArea, int yOffset)
    {
        SortedDictionary<int, float> pointData = new();
        float min = 0f;
        float max = yData[0];

        for (int y = 0; y < voxelArea + 1; y++)
        {
            int yValue = y + yOffset;
            float point = algorithm.GetPointValue(yValue, min, max);
            algorithm.SetHasData(point > 0f);
            terrainData[x, y, z] = point;
        }

        return;
    }
}
