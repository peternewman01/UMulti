using UnityEngine;

public class OutputCraft : MonoBehaviour
{
    [SerializeField] private ShowAllRecipies sar;

    public void Craft()
    {
        if (sar != null)
        {
            if (sar.getSelectedRecipe() != null)
            {
                Debug.Log("Craft!");
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
