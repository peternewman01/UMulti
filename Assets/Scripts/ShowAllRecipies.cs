using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ShowAllRecipies : MonoBehaviour
{
    public List<RecipeData> recipes = new List<RecipeData>();
    [SerializeField] private RecipeManager recipeManager;
    [SerializeField] private GameObject showParent;
    [SerializeField] private ControlPanel cp;
    [SerializeField] private CraftingShow show;

    [SerializeField] private GameObject showRecipePrefab;
    [SerializeField] private RecipeShow selectedRecipe;

    private void Start()
    {
        recipeManager = FindFirstObjectByType<RecipeManager>();
        cp = GetComponentInParent<ControlPanel>();
        show = GetComponentInParent<CraftingShow>();

        int count = 0;
        foreach(RecipeData recipe in recipeManager.GetAllRecipes())
        {
            GameObject save = Instantiate(showRecipePrefab, showParent.transform);
            RecipeShow rs = save.GetComponent<RecipeShow>();
            rs.SetRecipe(recipe);
            rs.Show();
            rs.sar = this;

            save.transform.position = showParent.transform.position + Vector3.down * ((rs.GetComponent<RectTransform>().sizeDelta.y * count) + 5);
            count++;

            recipes.Add(recipe);
        }
    }

    public void SelectRecipe(RecipeShow recipe)
    {
        if(selectedRecipe)
            selectedRecipe.selected = false;
        selectedRecipe = recipe;
        selectedRecipe.selected = true;

        show.UpdateSelected(selectedRecipe.GetRecipe());
    }

    public RecipeShow getSelectedRecipe() => selectedRecipe;
}
