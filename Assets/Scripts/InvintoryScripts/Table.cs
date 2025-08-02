using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;


[RequireComponent(typeof(PlayerInput))]
public class Table : Object
{
    public Recipe TempRecipe;
    public GameObject tempTargetPrefab;

    public Dictionary<int, int> holding = new Dictionary<int, int>();
    [SerializeField] private List<Totem> totems = new List<Totem>();
    [SerializeField] private Transform spawnPos;

    private void Start()
    {
        //temp version, will need to figure out recipies with ui
        Recipe stick = new Recipe();
        stick.recipe.Add(((int)Objects.WOOD, 3));

        stick.target = tempTargetPrefab.GetComponent<Stick>();
        TempRecipe = stick;
        TempRecipe.table = this;
    }

    protected override void Interact()
    {
        if (TempRecipe.TryCraftTotems())
        {
            Instantiate(tempTargetPrefab, spawnPos.position, spawnPos.rotation);
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
        if (holding.ContainsKey(obj.ObjectID))
        {
            holding[obj.ObjectID]++;
        }
        else
        {
            holding.Add(obj.ObjectID, 1);
        }
    }

    public void removeTotemHolding(Object obj)
    {
        if (holding.ContainsKey(obj.ObjectID))
        {
            if(holding[obj.ObjectID] > 1)
            {
                holding[obj.ObjectID]--;
            }
            else
            {
                holding.Remove(obj.ObjectID);
            }
        }
    }
}
