using Unity.Netcode;
using UnityEngine;

public class Totem : Interactable
{
    public Item neededItem;
    public static int ObjectID = -1;
    public static string ObjectName = "";
    private Table table;
    public GameObject holding;
    [SerializeField] private Transform spawnPos;
    public Transform woodPrefab;

    private bool isHolding = false;

    //TODO: grab players held item instead of trying to grab a specific item
    public override void Interact(PlayerManager interacter)
    {
        if (interacter.GetInventory().Has(ObjectID, 1))
        {
            RequestSpawnServerRpc(spawnPos.position);
            interacter.GetInventory().RemoveItem(ObjectID, 1);
        }
    }

    private void Start()
    {
        if (ObjectID == -1)
        {
            ObjectID = ItemManager.GetID(neededItem);
        }

        table = GetComponentInParent<Table>();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestSpawnServerRpc(Vector3 spawnPosition)
    {
        Transform spawnedObj = Instantiate(woodPrefab);
        spawnedObj.transform.position = spawnPosition;

        var netObj = spawnedObj.GetComponent<NetworkObject>();
        netObj.Spawn(true);

        table.addTotemHolding(spawnedObj.GetComponent<Entity>());
        holding = spawnedObj.gameObject;
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestKillServerRpc(NetworkObjectReference objRef)
    {
        if (objRef.TryGet(out NetworkObject netObj))
        {
            netObj.Despawn(true);
        }
    }

    public void killHolding()
    {
        holding.SetActive(false);
        var netObj = holding.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            RequestKillServerRpc(netObj);
        }
        else
        {
            Destroy(holding);
        }

        table.removeTotemHolding(holding.GetComponent<Entity>());
    }
}

