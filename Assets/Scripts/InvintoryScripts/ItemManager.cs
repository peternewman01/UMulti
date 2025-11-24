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

    public static Item GetItem(int id)
    {
        if(id != -1) return ids[id];
        else return null;
    }
    public static int GetID(Item item)
    {
        for (int index = 0; index < ids.Length; index++)
    {
            if (ids[index] == item) return index;
        }

        return -1;
    }
}
