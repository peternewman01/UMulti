using UnityEngine;

public class IslandMarching : MarchingAlgorithm
{
    [Tooltip("Ensure Prefab has proper index")]
    public uint topGenIndex;
    public uint bottomGenIndex;

    protected override void CreateTerrainMapDataAt(int x, int z)
    {
        int yOffset = subChunk * voxelArea;
        ChunkMananger chunkMananger = ChunkMananger.Instance;
        int chunkHeight = (int)chunkMananger.GetChunkHeight();
        ChunkMananger.Instance.GetGeneration().CustomNoise(this, new Vector2Int(x, z));
        float[] airGaps = chunkMananger.GetGeneration().GetAirGaps();

        int airIndex = 0;

        for(int y = 0; y < voxelArea; y++)
        {
            float point = 1;

            int yValue = y + yOffset;
            // We're only interested when point is within 0.5f of terrain surface. More than 0.5f less and it is just considered
            // solid terrain, more than 0.5f above and it is just air. Within that range, however, we want the exact value.
            if (yValue > airGaps[airIndex] - 0.5f && yValue < airGaps[airIndex + 1] + 0.5f)
            {
                point = 0;
                float minHeight = airGaps[airIndex];
                float maxHeight = airGaps[airIndex+1];
                if (minHeight < 0) minHeight = 0;
                if (maxHeight > chunkHeight) maxHeight = chunkHeight;

                if (yValue > (chunkHeight / 2))
                {
                    if (yValue <= maxHeight - 0.5f)
                        point = 0f;
                    else if (yValue > maxHeight)
                        point = yValue - maxHeight;
                    else
                        point = maxHeight - yValue;
                }
                else
                {
                    if (yValue > minHeight + 0.5f)
                        point = 0f;
                    else if (yValue > minHeight)
                        point = yValue - minHeight;
                    else
                        point = minHeight - yValue;
                }
            }

            terrainMap[x,y, z] = point; //Air == 1, solid terrain == 0, decimals == transition between air and terrain.
            // Get a terrain height using regular old Perlin noise.
        }
    }

}
