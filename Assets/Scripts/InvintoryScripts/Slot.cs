using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Splines;

[Serializable]
public class Slot : MonoBehaviour
{
    public ControlPanel ui;
    public Image itemImage;
    [SerializeField] private bool filled;
    public Vector2Int pos;
    [SerializeField] private Vector2Int slotSize;

    private float lastClickTime;
    private const float doubleClickThreshold = 0.25f;

    public static bool moving = false;

    private int objectID;

    public Slot SourceSlot;

    [SerializeField] private Sprite emptySprite;
    [SerializeField] private bool uiSlot = false;

    private void Start()
    {;
        itemImage = GetComponent<Image>();

        ResetItem();
    }

    private void Update()
    {
        if(SourceSlot.getSize() == Vector2Int.one || SourceSlot.SourceSlot != SourceSlot)
        {
            ResetItem();
        }
    }

    public void ClickSlot()
    {
        if (SourceSlot == this)
        {
            float timeSinceLastClick = Time.time - lastClickTime;

            if (timeSinceLastClick <= doubleClickThreshold)
            {
                OnDoubleClick();
            }
            else
            {
                OnSingleClick();
            }

            lastClickTime = Time.time;
        }
        else
        {
            SourceSlot.ClickSlot();
        }

        
    }

    //Wrong, this needs to be happening in the control pannel;
    private void OnSingleClick()
    {
        if (ui.MovingSlot && !filled)
        {
            ui.MoveMovingSlotTo(this);
        }
        else if (filled)
        {
            moving = true;
            ui.SetMovingSlot(this);
        }

    }

    private void OnDoubleClick()
    {
        Debug.Log("Double Click");
    }

    public void ResetItem()
    {
        itemImage.enabled = true;
        itemImage.sprite = emptySprite;
        //itemImage.color = new Color(1, 1, 1, 1);
        filled = false;
        SourceSlot = this;
        itemImage.SetNativeSize();
        slotSize = Vector2Int.one;

        objectID = -1;

        if(!ui.openSlots.Contains(this))
        {
            ui.openSlots.Add(this);
            ui.filledSlots.Remove(this);
        }


        RectTransform sourceRect = gameObject.GetComponent<RectTransform>();
        sourceRect.localScale = new Vector2(slotSize.x, slotSize.y);
    }
    public void SetItem(Slot SourceSlot, Item obj)
    {
        SetItem(SourceSlot, obj.GetSprite(), obj.GetInventorySize(), ItemManager.GetID(obj));
    }

    private void SetItem(Slot SourceSlot, Sprite image, Vector2Int size, int id)
    {
        this.SourceSlot = SourceSlot;
        filled = true;
        objectID = id;
        slotSize = size;

        if (this.SourceSlot == this)
        {
            if(uiSlot)
            {
                ScaleSetImage(itemImage, image);
            }
            else
            {
                itemImage.sprite = image;
                itemImage.SetNativeSize();
            }
            //itemImage.color = new Color(1, 1, 1, 1);
        }
        else
        {
            itemImage.enabled = false;
        }
    }
    public void ScaleSetImage(Image target, Sprite newImage)
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

    public Vector2Int getSize() { return slotSize; }

    public bool isFilled() {  return filled; } 

    public int getObjectID() { return objectID; }

    public bool isUiSlot() { return uiSlot; }
}
