using Unity.Netcode;
using UnityEngine;
using static UnityEngine.UI.Image;

public class DeathSpawner : Entity
{
    [SerializeField] private GameObject obj;
    [SerializeField] private int count;
    [SerializeField] private float radius = 1;

    public override void onHeal() { }
    public override void onHurt() { }
    public override void onDeath()
    {
        SpawnObjects();
        Destroy(gameObject);
    }
    private void SpawnObjects()
    {
        Vector3 spawnPoint = this.transform.position + Vector3.up * radius;
        for(int i = 0; i < count; i++)
        {
            RequestSpawnServerRpc(spawnPoint + Random.insideUnitSphere * radius);
        }
    }

    [Rpc(SendTo.Server)]
    private void RequestSpawnServerRpc(Vector3 spawnPosition)
    {
        Transform spawnedObj = Instantiate(obj.transform);
        spawnedObj.transform.position = spawnPosition;

        var netObj = spawnedObj.GetComponent<NetworkObject>();
        netObj.Spawn(true);
    }
}
