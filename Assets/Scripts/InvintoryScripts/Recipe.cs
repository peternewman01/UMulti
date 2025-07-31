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


    public void TryCraft()
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

                Debug.Log("you crafted a " +  target.ObjectName);
            }

        }
        else
        {
            Debug.Log("can't craft a " + target.ObjectName);
        }
    }
}
