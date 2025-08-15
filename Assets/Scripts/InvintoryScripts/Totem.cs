using Unity.Netcode;
using UnityEngine;

public class Totem : Object
{
    public static int ObjectID = -1;
    public static string ObjectName = "";
    private Table table;
    public GameObject holding;
    [SerializeField] private Transform spawnPos;
    public Transform woodPrefab;

    private bool isHolding = false;


    public override void Interact()
    {
        if (Invintory.Has(Wood.ObjectID, 1))
        {
            RequestSpawnServerRpc(spawnPos.position);
            Invintory.RemoveObject(Wood.ObjectID, 1);
        }
    }

    private void Start()
    {
        if (ObjectID == -1)
        {
            ObjectID = objectID;
            ObjectName = objectName;
        }

        table = GetComponentInParent<Table>();
    }

    [Rpc(SendTo.Server)]
    private void RequestSpawnServerRpc(Vector3 spawnPosition)
    {
        Transform spawnedObj = Instantiate(woodPrefab);
        spawnedObj.transform.position = spawnPosition;

        var netObj = spawnedObj.GetComponent<NetworkObject>();
        netObj.Spawn(true);

        table.addTotemHolding(spawnedObj.GetComponent<Object>());
        holding = spawnedObj.gameObject;
    }

    [Rpc(SendTo.Server)]
    private void RequestKillServerRpc(NetworkObjectReference objRef)
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
            if (IsServer)
            {
                netObj.Despawn(true);
            }
            else
            {
                RequestKillServerRpc(netObj);
            }
        }
        else
        {
            Destroy(holding);
        }

        table.removeTotemHolding(holding.GetComponent<Object>());
    }
}

