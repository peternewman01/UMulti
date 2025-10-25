using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

//[CreateAssetMenu(fileName = "NewIslandGeneration", menuName = "MarchingVoxelCubes/TerrainGeneration/CircleIsland")]
public abstract class IslandGenerator : TerrainGeneration
{
    [Serializable]
    protected struct HeightMaterials
    {
        public Material material;
        public float startHeight;
    }

    [SerializeField] protected List<PerlinMultipliers> perlinMultipliers = new();
    [SerializeField] protected List<PerlinMultipliers> bottomPerlinMultipliers = new();
    [SerializeField] protected float yFluxuation = 0f; // How much the center can fluctuate in the y-axis, 0 means no fluctuation

    [SerializeField] protected float islandSpacing = 0f; // min space between islands
    [SerializeField] protected HeightMaterials[] possibleMaterials;
    protected Vector3 center;

    public virtual void Init()
    {
        yFluxuation = UnityEngine.Random.Range(-yFluxuation, yFluxuation);


    }
    public abstract bool IsInsideIsland(Vector3 pos);

    public abstract float GetIslandMaxDistanceFromCenter();

    public void SetCenter(Vector3 newCenter)
    {
        center = newCenter;
        center.y = yFluxuation;
    }

    public Material GetMaterial(float height) 
    {
        Material bestMaterial = possibleMaterials[0].material;
        foreach (var material in possibleMaterials)
        {
            if (height + yFluxuation > material.startHeight) bestMaterial = material.material;
            else break;
        }
        return bestMaterial;
    }

    public Vector3 GetCenter() => center;
    public float GetSpacing() => islandSpacing;

    public abstract bool IsInsideIsland(Vector2Int chunk);

    public float GetFluxuation() => yFluxuation;
}
