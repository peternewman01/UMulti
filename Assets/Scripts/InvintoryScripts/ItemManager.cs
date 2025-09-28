using System;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    [SerializeField] private static Item[] itemID;
    public static ItemManager Instance;

    private void Awake()
    {
        if(Instance != this)
            Destroy(this);

        Instance = this;
    }

    public static Item GetItem(int id) => itemID[id];
    public static int GetID(Item item)
    {
        for (int index = 0; index < itemID.Length; index++)
        {
            if (itemID[index] == item) return index;
        }

        return -1;
    }
}

public class Item : ScriptableObject
{

}

[Serializable]
public struct ItemData
{
    public Item item;
    public int count;
}