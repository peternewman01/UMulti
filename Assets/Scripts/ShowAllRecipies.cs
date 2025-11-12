using System.Threading;
using UnityEngine;

public class ShowAllRecipies : MonoBehaviour
{
    [SerializeField] private RecipeManager recipeManager;
    [SerializeField] private GameObject showParent;
    [SerializeField] private ControlPanel cp;

    [SerializeField] private GameObject showRecipePrefab;

    private void Start()
    {
        recipeManager = FindFirstObjectByType<RecipeManager>();

        int count = 0;
        foreach(RecipeData recipe in recipeManager.GetAllRecipes())
        {
            GameObject save = Instantiate(showRecipePrefab, showParent.transform);
            RecipeShow rs = save.GetComponent<RecipeShow>();
            rs.SetRecipe(recipe);
            rs.Show();

            save.transform.position = showParent.transform.position + Vector3.down * ((rs.GetComponent<RectTransform>().sizeDelta.y * count) + 5);
            count++;
        }
    }
}
