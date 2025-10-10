using System;
using System.Linq;
using System.Reflection;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace UseEntity {
    [RequireComponent(typeof(NetworkObject))]
    public class Entity : NetworkBehaviour
    {
        //TODO: Spawn / despawn entity (networking)
    }

    public abstract class Interactable : Entity
    {
        public abstract void Interact(PlayerManager interacter);
    }

    public class Grabbable : Interactable
    {
        public Item item;

        public override void Interact(PlayerManager interacter)
        {
            interacter.GetInventory().AddItem(new ItemData(item, 1));
        }
    }
}


