using System.Collections.Generic;
using System.Globalization;
using Unity.VisualScripting;
using UnityEngine;

//[CreateAssetMenu(fileName = "NewIslandGeneration", menuName = "MarchingVoxelCubes/TerrainGeneration/CircleIsland")]
public abstract class IslandGenerator : TerrainGeneration
{
    [SerializeField] protected List<PerlinMultipliers> perlinMultipliers = new();
    [SerializeField] protected List<PerlinMultipliers> bottomPerlinMultipliers = new();

    [SerializeField] protected Material material;
    protected Vector3 center;

    public abstract bool IsInsideIsland(Vector3 pos);

    public abstract float GetIslandMaxDistanceFromCenter();

    public Material GetMaterial() => material;
    public void SetCenter(Vector3 newCenter) => center = newCenter; 
    public Vector3 GetCenter() => center;
}
