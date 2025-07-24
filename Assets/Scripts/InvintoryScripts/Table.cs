using NUnit.Framework;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class Table : Object
{
    public List<Unit> currentRecipe = null;
    private CraftingPanel panelRef;
    private bool open;
    public override void Initialize()
    {
        objectID = (int)Objects.TABLE;
        objectName = "Table";
    }

    private void Update()
    {
        if (playerInArea && pc.Interacted && !open)
        {
            Interact();
            open = true;
        }
        else if (!playerInArea || (open && pc.Interacted))
        {
            try
            {
                panelRef.HidePanel();
                open = false;
            }
            catch { }
        }
    }

    protected override void Interact()
    {
        panelRef = pc.CraftingPanel;
        pc.CraftingPanel.ShowPanel();
    }
}
