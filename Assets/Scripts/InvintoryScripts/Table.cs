using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Unity.Netcode;
using System.Collections;


[RequireComponent(typeof(PlayerInput))]
public class Table : Interactable
{
    CraftingInventory craftingRef;
    public int chosenRecipeIndex = 0;

    private void Awake()
    {
        craftingRef = GetComponent<CraftingInventory>();
    }
    public override void Interact(PlayerManager interacter)
    {
        RecipeData recipe = craftingRef.GetRecipe(chosenRecipeIndex);

        recipe.TryCraftItemFromInventory(craftingRef);
    }

    public void addTotemHolding(Entity obj)
    {
/*        if (holding.ContainsKey(obj.getID()))
        {
            holding[obj.getID()]++;
        }
        else
        {
            holding.Add(obj.getID(), 1);
        }*/
    }

    public void removeTotemHolding(Entity obj)
    {
/*        if (holding.ContainsKey(obj.getID()))
        {
            if(holding[obj.getID()] > 1)
            {
                holding[obj.getID()]--;
            }
            else
            {
                holding.Remove(obj.getID());
            }
        }*/
    }

    private void DelaySpawn()
    {
/*        RequestSpawnServerRpc(spawnPos.position);*/
    }


    [ServerRpc()]
    private void RequestSpawnServerRpc(Vector3 spawnPosition)
    {
/*        Transform spawnedObj = Instantiate(tempTargetPrefab);
        spawnedObj.transform.position = spawnPosition;
    
        var netObj = spawnedObj.GetComponent<NetworkObject>();
        netObj.Spawn(true);*/
    }
}
