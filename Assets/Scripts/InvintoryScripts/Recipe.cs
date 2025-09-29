using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new recipe", menuName = "Item/New Recipe")]
public class RecipeData : ScriptableObject
{
    [SerializeField] private ItemData[] requiredItems;
    [SerializeField] private ItemData outputItemData;

    public bool CanInventoryCraft(Invintory inventory)
    {
        foreach (ItemData item in requiredItems)
        {
            if (inventory.Has(item)) continue;
            return false;
        }

        return true;
    }

    public bool TryCraftItemFromInventory(Invintory inventory)
    {
        if (!CanInventoryCraft(inventory)) return false;

        CraftItemFromInventory(inventory);
        return true;
    }

    public void CraftItemFromInventory(Invintory inventory)
    {
        foreach (ItemData item in requiredItems)
        {
            inventory.RemoveItem(item);
        }

        inventory.AddItem(outputItemData);
    }
}
