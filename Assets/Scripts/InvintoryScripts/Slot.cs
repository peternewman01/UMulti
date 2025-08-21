using Mono.Cecil.Cil;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour
{
    public Image itemImage;
    public TMP_Text itemText;

    private float lastClickTime;
    private const float doubleClickThreshold = 0.25f;

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
        Debug.Log("Click");
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

        objectID = -1;
    }
    public void SetItem(Sprite image, string name, int id)
    {
        itemImage.sprite = image;
        itemImage.color = new Color(1, 1, 1, 1);
        itemText.text = name;

        objectID = id;
    }


}
