using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipeShow : MonoBehaviour
{
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text list;
    [SerializeField] private Image image;
    [SerializeField] private RecipeData recipe;
    [SerializeField] private Image background;
    public ShowAllRecipies sar;
    public bool selected;

    [Header("Button Colors")]
    [SerializeField] private Color basicColor;
    [SerializeField] private Color selectedColor;

    private void Awake()
    {
        //Show();
    }

    public void Show()
    {
        title.text = recipe.GetOutputItem().item.name;

        if(recipe != null)
        {
            image.sprite = recipe.GetOutputItem().item.GetSprite();
            image.SetNativeSize();

            // Get current size
            Vector2 size = image.rectTransform.sizeDelta;

            // Find how much we need to scale
            float maxSize = 100f;
            float scale = Mathf.Min(maxSize / image.rectTransform.sizeDelta.x, maxSize / image.rectTransform.sizeDelta.y);

            // If scaling would shrink it
            if (scale < 1f)
            {
                image.rectTransform.sizeDelta = size * scale;
            }

            string temp = "";
            List<ItemData> allItemData = new List<ItemData>();
            int currentItemCount = 0;
            ItemData[] requiredItems = recipe.GetRequiredItems();
            for (int i = 0; i < requiredItems.Count(); i++)
            {
                if(allItemData.Count() <= 0)
                {
                    allItemData.Add(requiredItems[i]);
                }
                if(allItemData.Last().item == requiredItems[i].item)
                {
                    currentItemCount += allItemData.Last().count;
                }
                if(i == requiredItems.Count() -1 || allItemData.Last().item != requiredItems[i].item)
                {
                    if (temp == "")
                    {
                        temp = currentItemCount + " " + allItemData.Last().item.name;
                    }
                    else
                    {
                        temp += ", " + currentItemCount + " " + allItemData.Last().item.name;
                    }
                    currentItemCount = requiredItems[i].count;
                }
                allItemData.Add(requiredItems[i]);
            }

            list.text = temp;
        }
    }

    public void SelectRecipe()
    {
        sar.SelectRecipe(this);
    }


    public void SetRecipe(RecipeData recipe)
    {
        this.recipe = recipe;
    }

    public RecipeData GetRecipe() => this.recipe;
}
