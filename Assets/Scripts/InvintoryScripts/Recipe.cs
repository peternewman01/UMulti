using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Xml;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "new recipe", menuName = "Item/New Recipe")]
public class RecipeData : ScriptableObject
{
    [SerializeField] private ItemData[] requiredItems;
    [SerializeField] private ItemData outputItemData;

    public bool CanInventoryCraft(Invintory inventory)
    {
        int currentCount = 0;
        int currentID = -1;
        foreach (ItemData item in requiredItems)
        {
            int hold = ItemManager.GetID(item.item);
            if (currentID == hold || currentID == -1)
            {
                currentID = hold;
                currentCount += item.count;
                continue;
            }
            else if(currentID != hold)
            {
                if (inventory.Has(currentID, currentCount))
                {
                    currentID = hold;
                    currentCount += item.count;
                    continue;
                }
            }
            return false;
        }
        
        if(currentID != -1)
        {
            if (!inventory.Has(currentID, currentCount))
            {
                return false;
            }
        }
        return true;
    }

    public bool TryCraftItemFromInventory(Invintory inventory)
    {
        if (!CanInventoryCraft(inventory)) return false;

        CraftItemFromInventory(inventory);
        return true;
    }

    //TODO Give Back Items if there isnt enough room in invintory?
    public void CraftItemFromInventory(Invintory inventory)
    {
        int currentCount = 0;
        int currentID = -1;
        List<Slot> removedSlots = new List<Slot>();
        foreach (ItemData item in requiredItems)
        {
            int hold = ItemManager.GetID(item.item);
            if (currentID == hold || currentID == -1)
            {
                currentID = hold;
                currentCount += item.count;
                continue;
            }
            else if (currentID != hold)
            {
                inventory.RemoveItem(currentID, currentCount);
            }
        }

        if (currentID != -1)
        {
            if (inventory.Has(currentID, currentCount))
            {
                inventory.RemoveItem(currentID, currentCount);
            }
        }

        if (inventory.GetControlPanel().AddObject(outputItemData.item))
        {
            inventory.AddItem(outputItemData);
        }
    }

    public bool CanCraftFromDataList(List<ItemData> dataList)
    {
        if (dataList == null) return false;
        foreach (ItemData item in requiredItems)
        {
            if (dataList.Contains(item)) continue;
            return false;
        }

        return true;
    }

    //Accessors
    public ItemData[] GetRequiredItems() => requiredItems;
    public ItemData GetOutputItem() => outputItemData;
}
