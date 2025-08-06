using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Unity.Netcode;


[RequireComponent(typeof(PlayerInput))]
public class Table : Object
{
    public Recipe TempRecipe;
    public Transform tempTargetPrefab;

    public Dictionary<int, int> holding = new Dictionary<int, int>();
    [SerializeField] private List<Totem> totems = new List<Totem>();
    [SerializeField] private Transform spawnPos;

    private void Start()
    {
        //temp version, will need to figure out recipies with ui
        Recipe stick = new Recipe();
        stick.recipe.Add((Wood.ObjectID, 3));

        stick.target = tempTargetPrefab.GetComponent<Stick>();
        TempRecipe = stick;
        TempRecipe.table = this;
    }

    protected override void Interact()
    {
        if (TempRecipe.TryCraftTotems())
        {
            RequestSpawnServerRpc(spawnPos.position);
            foreach(Totem totem in totems)
            {
                if(totem.holding)
                {
                    Destroy(totem.holding);
                }
            }
        }
    }

    private void Update()
    {
        if (playerInArea)
        {
            if(Input.GetKeyDown(KeyCode.E))
            {
                TempRecipe.targetInvintory = targetInvintory;
                Interact();

            }
        }
    }

    public void addTotemHolding(Object obj)
    {
        if (holding.ContainsKey(obj.getID()))
        {
            holding[obj.getID()]++;
        }
        else
        {
            holding.Add(obj.getID(), 1);
        }
    }

    public void removeTotemHolding(Object obj)
    {
        if (holding.ContainsKey(obj.getID()))
        {
            if(holding[obj.getID()] > 1)
            {
                holding[obj.getID()]--;
            }
            else
            {
                holding.Remove(obj.getID());
            }
        }
    }


    [Rpc(SendTo.Server)]
    private void RequestSpawnServerRpc(Vector3 spawnPosition)
    {
        Transform spawnedObj = Instantiate(tempTargetPrefab);
        spawnedObj.transform.position = spawnPosition;
    
        var netObj = spawnedObj.GetComponent<NetworkObject>();
        netObj.Spawn(true);
    }
}
