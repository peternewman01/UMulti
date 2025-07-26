using JetBrains.Annotations;
using System.ComponentModel;
using UnityEngine;

public class IslandMarching : MarchingAlgorithm
{
    [Tooltip("Ensure Prefab has proper index")]


    private void Awake()
    {
        generation.SetSeed();
    }

    protected override void CreateTerrainMapDataAt(int x, int z)
    {
        float[] airGaps = generation.CustomNoise(this, new Vector2Int(x, z));
        if(airGaps == null || airGaps.Length != 2)
        {
            Debug.Log($"IslandMarching: CustomNoise returned null or insufficient data for chunk ({x}, {z}), subchunk ({subChunk})");
            for (int y = 0; y < voxelArea; y++)
            {
                terrainMap[x, y, z] = 1f; // Default to air if no data is available
            }
            return;
        }

        int airIndex = 0;
        int yOffset = subChunk * voxelArea;
        float minHeight = airGaps[airIndex];
        float maxHeight = airGaps[airIndex + 1];

        for (int y = 0; y < voxelArea; y++)
        {
            int yValue = y + yOffset;
            float point = yValue > minHeight - 0.5f && yValue < maxHeight + 0.5f ? yValue < maxHeight - 0.5f ? yValue > minHeight + 0.5f ?
                    0f : yValue > minHeight ? yValue - minHeight : minHeight - yValue : yValue < maxHeight ? maxHeight - yValue : yValue - maxHeight : 1f; //Black magic

            if (point != 1f) hasData = true;
            terrainMap[x, y, z] = point;
            // Get a terrain height using regular old Perlin noise.
        }
    }
}
