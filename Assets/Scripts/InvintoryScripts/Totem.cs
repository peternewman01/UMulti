using Unity.Netcode;
using UnityEngine;

public class Totem : Entity
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

