using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class CraftButton : MonoBehaviour
{
    public CraftingPanel CraftingPanel;
    public List<(int, int)> recipe = new List<(int, int)>() { ((int)Objects.WOOD, 5) };

    public Object Target = null;

    public void TryCraft()
    {
        bool canCraft = true;

        foreach (var item in recipe)
        {
            if (!CraftingPanel.PlayerInvintory.Has(item.Item1, item.Item2))
            {
                canCraft = false;
            }
        }

        if(canCraft)
        {
            Debug.Log("can craft");

            foreach (var item in recipe)
            {
                CraftingPanel.PlayerInvintory.RemoveObject(item.Item1, item.Item2);
            }

            if (Target != null)
            {
                CraftingPanel.PlayerInvintory.AddObject(Target, 1);
            }

        }
        else
        {
            Debug.Log("can't craft");
        }
    }
}
