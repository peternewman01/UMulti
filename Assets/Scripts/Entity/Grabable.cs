using Unity.Netcode;
using UnityEngine;
using UseEntity;

namespace UseEntity
{
    public class Grabbable : Interactable
    {
        public Item item;

        public override void Interact(PlayerManager interacter)
        {
            if (interacter.controlPanel.AddObject(item))
            {
                interacter.GetInventory().AddItem(new ItemData(item, 1));
                NetworkObject no = GetComponent<NetworkObject>();
                NetcodeConnector.RequestKillServerRpc(new NetworkObjectReference(no));
            }
        }
    }
}