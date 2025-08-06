using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Recipe
{
    public Invintory targetInvintory;

    public List<(int, int)> recipe = new List<(int, int)>() { };
    public Object target;
    public Table table;
    private Dictionary<int, int> holding = new Dictionary<int, int>() { };


    public void TryCraftInvintory()
    {
        bool canCraft = true;

        foreach (var item in recipe)
        {
            if (!targetInvintory.Has(item.Item1, item.Item2))
            {
                canCraft = false;
            }
        }

        if(canCraft)
        {
            foreach (var item in recipe)
            {
                targetInvintory.RemoveObject(item.Item1, item.Item2);
            }

            if (target != null)
            {
                targetInvintory.AddObject(target, 1);

                Debug.Log("you crafted a " +  target.getName());
            }

        }
        else
        {
            Debug.Log("can't craft a " + target.getName());
        }
    }

    public bool TryCraftTotems()
    {
        foreach (KeyValuePair<int, int> item in table.holding)
        {
            if(holding.ContainsKey(item.Key))
            {
                holding[item.Key] += item.Value;
            }
            else
            {
                holding.Add(item.Key, item.Value);
            }
        }

        bool canCraft = true;

        foreach (var item in recipe)
        {
            if (holding.ContainsKey(item.Item1))
            {
                if (holding[item.Item1] < item.Item2)
                {
                    canCraft = false;
                }
            }
            else
            {
                canCraft = false;
            }
        }

        return canCraft;
    }
}
