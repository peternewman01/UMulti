using System;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "new item", menuName = "Item/New Item")]
public class Item : ScriptableObject
{
    [SerializeField] private Sprite inventorySprite;
    [SerializeField] private GameObject worldPrefab;

    public Sprite GetSprite() => inventorySprite;
    public NetworkObject GetWorldPrefab() => worldPrefab.GetComponent<NetworkObject>();
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