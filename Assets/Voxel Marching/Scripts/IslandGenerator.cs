using System.Collections.Generic;
using System.Globalization;
using Unity.VisualScripting;
using UnityEngine;

//[CreateAssetMenu(fileName = "NewIslandGeneration", menuName = "MarchingVoxelCubes/TerrainGeneration/CircleIsland")]
public abstract class IslandGenerator : TerrainGeneration
{
    [SerializeField] protected List<PerlinMultipliers> perlinMultipliers = new();
    [SerializeField] protected List<PerlinMultipliers> bottomPerlinMultipliers = new();
    [SerializeField] protected float yFluxuation = 0f; // How much the center can fluctuate in the y-axis, 0 means no fluctuation

    [SerializeField] protected float islandSpacing = 0f; // min space between islands
    [SerializeField] protected Material material;
    protected Vector3 center;

    public virtual void Init()
    {
        yFluxuation = Random.Range(-yFluxuation, yFluxuation);
    }
    public abstract bool IsInsideIsland(Vector3 pos);

    public abstract float GetIslandMaxDistanceFromCenter();

    public Material GetMaterial() => material;
    public void SetCenter(Vector3 newCenter)
    {
        center = newCenter;
        center.y = yFluxuation;
    }

    public Vector3 GetCenter() => center;
    public float GetSpacing() => islandSpacing;

    public abstract bool IsInsideIsland(Vector2Int chunk);

}
