using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingShow : MonoBehaviour
{
    public ShowAllRecipies sar;
    [SerializeField] private TMP_Text currentCraft;
    [SerializeField] private Image mainImage;
    [SerializeField] private List<Image> slotImages = new List<Image>();

    private void Start()
    {
        sar = GetComponent<ShowAllRecipies>();
    }

    public void UpdateSelected(RecipeData r)
    {
        currentCraft.text = r.GetOutputItem().item.name;

        ScaleSetImage(mainImage, r.GetOutputItem().item.GetSprite());
        ItemData[] rqItems = r.GetRequiredItems();
        int savei = 4;
        for (int i = 0; i < rqItems.Length; i++)
        {
            if(i < 4)
            {
                ScaleSetImage(slotImages[i], rqItems[i].item.GetSprite());
            }
            savei = i;
        }
        if(savei < 3)
        {

            for (int i = savei+1; i < slotImages.Count; i++)
            {
                slotImages[i].gameObject.SetActive(false);
            }
        }
    }

    private void ScaleSetImage(Image target, Sprite newImage)
    {
        if (!target.gameObject.active)
        {
            target.gameObject.SetActive(true);
        }
        target.sprite = newImage;
        target.SetNativeSize();
        Vector2 size = target.rectTransform.sizeDelta;

        // Find how much we need to scale
        float maxSize = 100f;
        float scale = Mathf.Min(maxSize / target.rectTransform.sizeDelta.x, maxSize / target.rectTransform.sizeDelta.y);

        // If scaling would shrink it
        if (scale != 1f)
        {
            target.rectTransform.sizeDelta = size * scale;
        }
    }

}
