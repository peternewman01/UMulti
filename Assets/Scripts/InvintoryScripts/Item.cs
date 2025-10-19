using System;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "new Item", menuName = "Item/New Item")]
public class Item : ScriptableObject
{
    [SerializeField] private Sprite sprite;
    [SerializeField] private NetworkObject worldPrefab;

    private void Awake()
    {
        if (sprite == null)
            sprite = (Sprite)Resources.Load("ObjectThumbnails/BasicThumbnail");
    }

    public Sprite GetSprite() => sprite;
    public NetworkObject GetWorldPrefab() => worldPrefab;
}

[Serializable]
public struct ItemData
{
    public Item item;
    public int count;

    public ItemData(Item item, int v) : this()
    {
        this.item = item;
        count = v;
    }
}