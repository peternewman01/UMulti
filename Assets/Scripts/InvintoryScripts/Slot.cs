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
    public TMP_Text itemText;
    [SerializeField] private bool filled;
    public Vector2Int pos;

    private float lastClickTime;
    private const float doubleClickThreshold = 0.25f;

    public static bool moving = false;
    private static Slot MovingSlot;

    private int objectID;

    public Slot SourceSlot;

    [SerializeField] private Sprite emptySprite;

    private void Start()
    {;
        itemImage = transform.GetChild(0).GetComponent<Image>();
        itemText = transform.GetChild(2).GetComponent<TMP_Text>();

        ResetItem();
    }

    public void ClickSlot()
    {

        if (SourceSlot == this)
        {
            Debug.Log("sourceSlot here");
        }
        else
        {
            Debug.Log("sourceSlot at " + SourceSlot.pos);
        }

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

    private void OnSingleClick()
    {
        if (moving && MovingSlot && !filled)
        {
            MovingSlot.filled = false;
            filled = true;

            ui.openSlots.Add(MovingSlot);
            ui.openSlots.Remove(this);
            ui.filledSlots.Remove(MovingSlot);
            ui.filledSlots.Add(this);

            this.SetItem(MovingSlot.itemImage.sprite, MovingSlot.itemText.text, objectID);
            MovingSlot.ResetItem();

            MovingSlot = null;
        }
        else if(filled)
        {
            moving = true;
            MovingSlot = this;
        }
    }

    private void OnDoubleClick()
    {
        Debug.Log("Double Click");
    }

    public void ResetItem()
    { 
        itemImage.sprite = emptySprite;
        //itemImage.color = new Color(1, 1, 1, 1);
        itemText.text = "";
        filled = false;
        SourceSlot = this;

        objectID = -1;

    }
    public void SetItem(Sprite image, string name, int id)
    {
        itemImage.sprite = image;
        //itemImage.color = new Color(1, 1, 1, 1);
        itemText.text = name;
        filled = true;
        SourceSlot = this;

        objectID = id;

        //set set to the SourceSlot Image
    }

    public void SetItem(Slot SourceSlot, Item obj)
    {
        SetItem(SourceSlot, obj.GetSprite(), obj.name, ItemManager.GetID(obj));
    }

    private void SetItem(Slot SourceSlot, Sprite image, string name, int id)
    {
        this.SourceSlot = SourceSlot;
        filled = true;
        objectID = id;

        if (this.SourceSlot == this)
        {
            itemImage.sprite = image;
            //itemImage.color = new Color(1, 1, 1, 1);
            itemText.text = name;
        }
        else
        {
            itemImage.sprite = null;
            itemText.text = "";
        }
    }

    public bool isFilled() {  return filled; } 

    public int getObjectID() { return objectID; }
}
