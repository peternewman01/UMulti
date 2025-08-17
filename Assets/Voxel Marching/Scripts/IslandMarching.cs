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
        float[] airGaps = generation.CustomNoise(ChunkMananger.Instance.ChunkToWorld(chunk) + new Vector3(x, 0, z));
        if(airGaps == null)
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
            float point = yValue >= minHeight - 0.5f && yValue <= maxHeight + 0.5f /*Q1*/ ? /*Q1-T*/ yValue <= maxHeight - 0.5f /*Q2*/ ? /*Q2-T*/ yValue >= minHeight + 0.5f /*Q3*/ ?
                     /*Q3-T*/ 1f : /*Q3-F*/ yValue > minHeight /*Q4*/ ? /*Q4-T*/ yValue - minHeight : /*Q4-F*/ minHeight - yValue : 
                     /*Q2-F*/ yValue < maxHeight /*Q5*/ ? /*Q5-T*/ maxHeight - yValue : /*Q5-F*/ yValue - maxHeight : /*Q1-F*/ 0f; //Black magic  :~}

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
