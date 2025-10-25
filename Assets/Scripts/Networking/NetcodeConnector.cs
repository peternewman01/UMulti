using Unity.Netcode;
using UnityEngine;

public class NetcodeConnector : NetworkManager
{

    [ServerRpc(RequireOwnership = false)]
    public static void SpawnObjectServerRpc(NetworkObject prefab, out NetworkObject spawned, Vector3? position = null, Quaternion? rotation = null)
    {
        if (position == null) position = Vector3.zero;
        if (rotation== null) rotation = Quaternion.identity;

        spawned = Instantiate(prefab, (Vector3)position, (Quaternion)rotation);
        spawned.Spawn();
    }

    [ServerRpc(RequireOwnership = false)]
    public static void SpawnObjectServerRpc(NetworkObject prefab, out GameObject spawned, Vector3? position = null, Quaternion? rotation = null)
    {
        SpawnObjectServerRpc(prefab, out NetworkObject netObj, position, rotation);
        spawned = netObj.gameObject;
    }

    public static void SpawnObjectServerRpc(NetworkObject prefab, Vector3? position = null, Quaternion? rotation = null)
    {
        SpawnObjectServerRpc(prefab, out NetworkObject netObj, position, rotation);
    }

    [ServerRpc(RequireOwnership = false)]
    public static void SpawnMarchingAlgorithmRpc(MarchingAlgorithm prefab, out MarchingAlgorithm spawned, Vector3 position, Vector2Int chunk, uint subCube)
    {
        spawned = Instantiate(prefab, position, Quaternion.identity);
        spawned.InitChunkData(chunk, subCube);
        spawned.GetComponent<NetworkObject>().Spawn();
    }

    [ServerRpc(RequireOwnership = false)]
    public static void RequestKillServerRpc(NetworkObjectReference objRef)
    {
        if (objRef.TryGet(out NetworkObject netObj))
        {
            netObj.Despawn(true);
        }
    }
}
