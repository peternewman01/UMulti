using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class RecipeManager : MonoBehaviour
{
    [SerializeField] private List<RecipeData> allRecipes = new();
    public static RecipeManager Instance;
    private void Awake()
    {
        Instance = this;
    }

    public RecipeData GetRecipe(int index) => allRecipes[index];

    public List<RecipeData> GetPossibleRecipes(Invintory invintory)
    {
        List<RecipeData> possibleRecipes = new();
        foreach (var recipe in allRecipes)
        {
            if (recipe.TryCraftItemFromInventory(invintory)) possibleRecipes.Add(recipe);
        }

        return possibleRecipes;
    }

    public bool CraftRecipe(Invintory invintory)
    {
        foreach(var recipe in allRecipes)
        {
            if (recipe.TryCraftItemFromInventory(invintory)) return true;
        }

        return false;
    }

    public bool CraftRecipe(List<ItemData> data)
    {
        foreach(var recipe in allRecipes)
        {
            if (recipe.CanCraftFromDataList(data)) return true;
        }

        return false;
    }
}
