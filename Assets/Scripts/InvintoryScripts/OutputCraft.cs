using UnityEngine;
using UseEntity;

public class OutputCraft : MonoBehaviour
{
    [SerializeField] private ShowAllRecipies sar;
    [SerializeField] private Table ritualArea;

    public void Craft()
    {
        if (sar != null)
        {
            RecipeShow selected = sar.getSelectedRecipe();
            if (selected != null)
            {
                selected.GetRecipe().TryCraftItemFromInventory(sar.getControlPanel().invintory);
            }
            else
            {
                Debug.Log("Couldn't find recipe");
            }
        }
        else
        {
            Debug.Log("Couldn't find show all recipes");
        }
    }
}
