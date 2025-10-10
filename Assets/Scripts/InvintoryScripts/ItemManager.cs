using System;
using Unity.Netcode;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    [SerializeField] private Item[] itemID;
    public static ItemManager Instance;
    private static Item[] ids;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            ids = itemID;
        }
        else if(Instance != this)
        {
            Destroy(this);
        }
    }

    public static Item GetItem(int id) => ids[id];
    public static int GetID(Item item)
    {
        for (int index = 0; index < ids.Length; index++)
        {
            if (ids[index] == item) return index;
        }

        return -1;
    }
}

[CreateAssetMenu(fileName = "new item", menuName = "Item/New Item")]
public class Item : ScriptableObject
{
    [SerializeField] private Sprite sprite;
    [SerializeField] private NetworkObject worldPrefab;

    private void Awake()
    {
        if(sprite == null)  
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