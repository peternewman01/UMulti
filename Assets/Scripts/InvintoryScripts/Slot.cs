using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using UnityEngine.Rendering.HighDefinition;

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

    private void Start()
    {;
        itemImage = GetComponent<Image>();

        ResetItem();
    }

    private void Update()
    {
        if(SourceSlot.getSize() == Vector2Int.one)
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
        /*if (moving && MovingSlot && !filled)
        {
            MovingSlot.filled = false;
            filled = true;

            ui.openSlots.Add(MovingSlot);
            ui.openSlots.Remove(this);
            ui.filledSlots.Remove(MovingSlot);
            ui.filledSlots.Add(this);

            this.SetItem(MovingSlot, MovingSlot.itemImage.sprite, objectID);
            MovingSlot.ResetItem();

            MovingSlot = null;
        }
        else if(filled)
        {
            moving = true;
            MovingSlot = this;
        }*/

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

        ui.openSlots.Add(this);
        ui.filledSlots.Remove(this);
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
            itemImage.sprite = image;
            itemImage.SetNativeSize();
            //itemImage.color = new Color(1, 1, 1, 1);
        }
        else
        {
            itemImage.enabled = false;
        }
    }

    public Vector2Int getSize() { return slotSize; }

    public bool isFilled() {  return filled; } 

    public int getObjectID() { return objectID; }
}
