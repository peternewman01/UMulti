using Unity.Netcode;
using UnityEngine;

namespace UseEntity
{
    public class Totem : Interactable
    {
        public static string ObjectName = "";
        private Table table;
        public NetworkObject holding;
        [SerializeField] private Transform spawnPos;
        public Transform woodPrefab;
        private ItemData requestItem;

        private bool isHolding = false;

        //TODO: how do we wanna handle interacting with crafting interfaces and the totems?
        public override void Interact(PlayerManager interacter)
        {
            if (interacter.GetInventory().Has(requestItem)) 
            {
                NetcodeConnector.SpawnObjectServerRpc(requestItem.item.GetWorldPrefab(), out holding, spawnPos.position);
                holding.GetComponent<Rigidbody>().useGravity = false; //TEMP
                interacter.GetInventory().RemoveItem(requestItem);
                table.addTotemHolding(requestItem); 
            }
        }

        public void SetRequestedItem(ItemData data)
        {
            requestItem = data;
        }

        private void Start()
        {
            table = GetComponentInParent<Table>();
        }

/*        [ServerRpc(RequireOwnership = false)]
        public void RequestSpawnServerRpc(Vector3 spawnPosition)
        {
            Transform spawnedObj = Instantiate(woodPrefab);
            spawnedObj.transform.position = spawnPosition;

            var netObj = spawnedObj.GetComponent<NetworkObject>();
            netObj.Spawn(true);

            table.addTotemHolding(new ItemData(heldItem, 1));
            holding = spawnedObj.gameObject;
        }*/



        public void killHolding()
        {
            if (holding == null) return;

            NetcodeConnector.RequestKillServerRpc(holding);
            table.removeTotemHolding(requestItem);
        }
    }
}


