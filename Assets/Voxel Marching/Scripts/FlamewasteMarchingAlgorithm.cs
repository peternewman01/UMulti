using System.Collections.Generic;
using UnityEngine;

public class FlamewasteMarchingAlgorithm : MarchingAlgorithm
{
    protected override void CreateTerrainMapDataAt(int x, int z)
    {
        float[] airGaps = generation.CustomNoise(ChunkMananger.Instance.ChunkToWorld(chunk) + new Vector3(x, 0, z), this);
        float min = 0f;
        float max = airGaps[0];
        uint yOffset = subChunk * voxelArea;

        for (uint y = 0; y < voxelArea + 1; y++)
        {
            uint yValue = y + yOffset;
            float point = GetPointValue(yValue, min, max);
            if (point > 0f) hasData = true;
            terrainMap[x, y, z] = point;
        }

    }
}
