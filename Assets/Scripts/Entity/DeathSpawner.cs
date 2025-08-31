using Unity.Netcode;
using UnityEngine;
using static UnityEngine.UI.Image;

public class DeathSpawner : Entity
{
    [SerializeField] private GameObject obj;
    [SerializeField] private int count;
    [SerializeField] private float radius = 1;
    [Range(0f, 1f)] [SerializeField] private float spawnChance = 1;
    [SerializeField] private bool guaranteed = false;
    private void OnEnable()
    {
        OnDeath += deathSpawn;
    }

    private void OnDisable()
    {
        OnDeath -= deathSpawn;
    }
    public void deathSpawn()
    {
        SpawnObjects();
        Invoke("delayDestroy", 0.05f);
    }

    public void delayDestroy()
    {
        Destroy(gameObject);
    }
    private void SpawnObjects()
    {
        //TODO: spawn multiple objects
            //Chance? hardset? BOTH??
        Vector3 spawnPoint = this.transform.position + Vector3.up * radius;
        bool hasSpawned = false;
        for(int i = 0; i < count; i++)
        {
            float chance = Random.Range(0, 1);
            if(chance < spawnChance || (guaranteed && !hasSpawned))
            {
                RequestSpawnServerRpc(spawnPoint + Random.insideUnitSphere * radius);
                hasSpawned = true;
            }
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
