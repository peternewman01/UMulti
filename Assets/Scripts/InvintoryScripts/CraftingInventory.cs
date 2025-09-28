using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class CraftingInventory : Invintory
{
    [SerializeField] protected RecipeData[] availableRecipes;
    protected List<Invintory> connectedInventories = new();

    public void OpenInventory(Invintory interacter)
    {
        connectedInventories.Add(interacter);
    }

    public void CloseInventory(Invintory interacter)
    {
        connectedInventories.Remove(interacter);
    }

    public RecipeData GetRecipe(int index) => availableRecipes[index];
}
