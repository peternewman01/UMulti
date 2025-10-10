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
        private List<ItemData> heldItems;
        private Totem[] totems;

        private void Awake()
        {
            totems = GetComponentsInChildren<Totem>();
        }
        public override void Interact(PlayerManager interacter)
        {
            if(RecipeManager.Instance.CraftRecipe(heldItems))
            {
                //BUG -- extra items on totems will be destroyed
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

