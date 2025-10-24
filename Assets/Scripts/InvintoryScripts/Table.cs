using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Unity.Netcode;
using System.Collections;

namespace UseEntity
{
    [RequireComponent(typeof(PlayerInput))]
    public class Table : Interactable
    {
        [SerializeField] private Transform craftedItemSpawnPoint;
        private List<ItemData> heldItems = new();
        private Totem[] totems;

        private void Start()
        {
            totems = GetComponentsInChildren<Totem>();

            //TEMP
            ItemData[] requiredItemsForCraft = RecipeManager.Instance.GetRecipe(0).GetRequiredItems();
            for (int i = 0; i < requiredItemsForCraft.Length; i++)
            {
                ItemData item = requiredItemsForCraft[i];
                totems[i].SetRequestedItem(item);
            }
        }
        public override void Interact(PlayerManager interacter)
        {
            if(RecipeManager.Instance.CraftRecipe(heldItems, out RecipeData recipe))
            {
                //BUG -- extra items on totems will be destroyed
                NetcodeConnector.SpawnObjectServerRpc(recipe.GetOutputItem().item.GetWorldPrefab(), craftedItemSpawnPoint.position, craftedItemSpawnPoint.rotation);
                foreach(Totem totem in totems)
                {
                    totem.killHolding();
                }
            }
            else
            {
                //TODO: indicate to player crafting failed
            }
        }

        public void addTotemHolding(ItemData data)
        {
            heldItems.Add(data);
        }

        public void removeTotemHolding(ItemData data)
        {
            heldItems.Remove(data);
        }
    }
}

