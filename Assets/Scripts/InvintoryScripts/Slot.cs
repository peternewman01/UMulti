using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour
{
    public ControlPanel ui;
    public Image itemImage;
    public TMP_Text itemText;
    [SerializeField] private bool filled;

    private float lastClickTime;
    private const float doubleClickThreshold = 0.25f;

    public static bool moving = false;
    public static Slot MovingSlot;

    public int objectID;

    private void Start()
    {;
        itemImage = transform.GetChild(0).GetComponent<Image>();
        itemText = transform.GetChild(2).GetComponent<TMP_Text>();

        ResetItem();
    }

    public void ClickSlot()
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
        itemImage.sprite = null;
        itemImage.color = new Color(1, 1, 1, 0);
        itemText.text = "";
        filled = false;

        objectID = -1;
    }
    public void SetItem(Sprite image, string name, int id)
    {
        itemImage.sprite = image;
        itemImage.color = new Color(1, 1, 1, 1);
        itemText.text = name;
        filled = true;

        objectID = id;
    }

    public bool isFilled() {  return filled; } 
}
