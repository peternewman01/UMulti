using System;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "new item", menuName = "Item/New Item")]
public class Item : ScriptableObject
{
    [SerializeField] private Sprite inventorySprite;
    //[SerializeField] private float tileSize = 32;
    [SerializeField] private GameObject worldPrefab;

    public Sprite GetSprite() => inventorySprite;
    public NetworkObject GetWorldPrefab() => worldPrefab.GetComponent<NetworkObject>();

    public Vector2Int GetInventorySize()
    {
        if (inventorySprite == null) return Vector2Int.zero;

        return new Vector2Int(Mathf.FloorToInt(inventorySprite.rect.width / inventorySprite.pixelsPerUnit), Mathf.FloorToInt(inventorySprite.rect.height / inventorySprite.pixelsPerUnit));
    }
}

[Serializable]
public struct ItemData
{
    public Item item;
    public int count;

    public ItemData(Item item, int count)
    {
        this.item = item;
        this.count = count;
    }
}