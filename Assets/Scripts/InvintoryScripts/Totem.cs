using Unity.Netcode;
using UnityEngine;

namespace UseEntity
{
    public class Totem : Interactable
    {
        public Item heldItem;
        public static string ObjectName = "";
        private Table table;
        public GameObject holding;
        [SerializeField] private Transform spawnPos;
        public Transform woodPrefab;

        private bool isHolding = false;

        //TODO: grab players held item instead of trying to grab a specific item
        public override void Interact(PlayerManager interacter)
        {
            if (interacter.GetInventory().Has(ItemManager.GetID(heldItem), 1))
            {
                NetcodeConnector.SpawnObjectServerRpc(heldItem.GetWorldPrefab(), spawnPos.position);
                interacter.GetInventory().RemoveItem(new ItemData(heldItem, 1));
            }
        }

        private void Start()
        {
            table = GetComponentInParent<Table>();
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestSpawnServerRpc(Vector3 spawnPosition)
        {
            Transform spawnedObj = Instantiate(woodPrefab);
            spawnedObj.transform.position = spawnPosition;

            var netObj = spawnedObj.GetComponent<NetworkObject>();
            netObj.Spawn(true);

            table.addTotemHolding(new ItemData(heldItem, 1));
            holding = spawnedObj.gameObject;
        }



        public void killHolding()
        {
            holding.SetActive(false);
            var netObj = holding.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                NetcodeConnector.RequestKillServerRpc(netObj);
            }
            else
            {
                Destroy(holding);
            }

            table.removeTotemHolding(new ItemData(heldItem, 1));
        }
    }
}


