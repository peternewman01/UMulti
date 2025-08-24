using JetBrains.Annotations;
using System.ComponentModel;
using UnityEngine;

public class IslandMarching : MarchingAlgorithm
{
    protected override void CreateTerrainMapDataAt(int x, int z)
    {
        float[] airGaps = generation.CustomNoise(ChunkMananger.Instance.ChunkToWorld(chunk) + new Vector3(x, 0, z), this);
        if(airGaps.Length == 0 || airGaps == null)
        {
            for (int y = 0; y < voxelArea + 1; y++)
            {
                terrainMap[x, y, z] = 0f; // Default to air if no data is available
            }
            return;
        }

        int airIndex = 0;
        uint yOffset = subChunk * voxelArea;
        float minHeight = airGaps[airIndex];
        float maxHeight = airGaps[airIndex + 1];

        for (uint y = 0; y < voxelArea + 1; y++)
        {

            uint yValue = y + yOffset;
            float point = GetPointValue(yValue, minHeight, maxHeight);

            if (point > 0f) hasData = true;
            terrainMap[x, y, z] = point;

            if (airIndex + 2 < airGaps.Length && yValue > maxHeight + 0.5f)
            {
                airIndex += 2;
                maxHeight = airGaps[airIndex + 1];
                minHeight = airGaps[airIndex];
                y = (uint)minHeight;
            }
        }
    }
}
