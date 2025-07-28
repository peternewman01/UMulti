using JetBrains.Annotations;
using System.ComponentModel;
using UnityEngine;

public class IslandMarching : MarchingAlgorithm
{
    [Tooltip("Ensure Prefab has proper index")]


    private void Awake()
    {
        
    }


    protected override void CreateTerrainMapDataAt(int x, int z)
    {
        float[] airGaps = generation.CustomNoise(this, new Vector2Int(x, z));
        if(airGaps == null || airGaps.Length != 2)
        {
            Debug.Log($"IslandMarching: CustomNoise returned null or insufficient data for chunk ({x}, {z}), subchunk ({subChunk})");
            for (int y = 0; y < voxelArea + 1; y++)
            {
                terrainMap[x, y, z] = 0f; // Default to air if no data is available
            }
            return;
        }

        int airIndex = 0;
        int yOffset = subChunk * voxelArea;
        float minHeight = airGaps[airIndex];
        float maxHeight = airGaps[airIndex + 1];

        for (int y = 0; y < voxelArea + 1; y++)
        {
            int yValue = y + yOffset;
            float point = yValue >= minHeight - 0.5f && yValue <= maxHeight + 0.5f /*Q1*/ ? /*Q1-T*/ yValue <= maxHeight - 0.5f /*Q2*/ ? /*Q2-T*/ yValue >= minHeight + 0.5f /*Q3*/ ?
                     /*Q3-T*/ 1f : /*Q3-F*/ yValue > minHeight /*Q4*/ ? /*Q4-T*/ yValue - minHeight : /*Q4-F*/ minHeight - yValue : 
                     /*Q2-F*/ yValue < maxHeight /*Q5*/ ? /*Q5-T*/ maxHeight - yValue : /*Q5-F*/ yValue - maxHeight : /*Q1-F*/ 0f; //Black magic  :~}

            if (point > 0f) hasData = true;
            terrainMap[x, y, z] = point;
            // Get a terrain height using regular old Perlin noise.
        }
    }
}
