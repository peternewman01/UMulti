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
    private Vector3 startSpawnPos;

    private void Start()
    {
        startSpawnPos = transform.position;
    }

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
        Vector3 spawnPoint = transform.position;// + Vector3.up * radius;
        //Debug.Log(startSpawnPos);
        bool hasSpawned = false;
        for(int i = 0; i < count; i++)
        {
            float chance = Random.Range(0f, 1f);
            if(chance < spawnChance || (guaranteed && !hasSpawned))
            {
                Vector3 randomSphere = Random.insideUnitSphere * radius;
                randomSphere.y = 0f;
                Vector3 pos = spawnPoint + randomSphere;
                pos.y = spawnPoint.y + 0.8f;
                RequestSpawnServerRpc(pos);
                hasSpawned = true;
            }
        }
    }

    [ServerRpc]
    public void RequestSpawnServerRpc(Vector3 spawnPosition)
    {
        Transform spawnedObj = Instantiate(obj.transform);
        spawnedObj.transform.position = spawnPosition;

        var netObj = spawnedObj.GetComponent<NetworkObject>();
        netObj.Spawn(true);
    }
}
