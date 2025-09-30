using UnityEngine;
using UnityEngine.UI;

public class HotbarSelector : MonoBehaviour
{
    [Header("Hotbar Setup")]
    [SerializeField] private RectTransform[] slots = new RectTransform[10];
    [SerializeField] private float unselectedScale = 0.75f;
    [SerializeField] private float selectedScale = 1.2f;
    [SerializeField] private float selectedYOffset = 24f;
    [SerializeField] private float unselectedAlpha = 0.5f;
    [SerializeField] private float selectedAlpha = 1f;
    [SerializeField] private float xShift = 40f;

    private Vector2[] originalPositions;
    private int currentIndex = -1;

    void Start()
    {
        CacheOriginalPositions();
        ResetAllSlots();
    }

    void Update()
    {
        CheckInput();
    }

    private void CacheOriginalPositions()
    {
        originalPositions = new Vector2[slots.Length];
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
                originalPositions[i] = slots[i].anchoredPosition;
        }
    }

    private void CheckInput() //note: programmers please fix my horrible designer code please
    {
        int slotIndex = -1;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            slotIndex = 0;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            slotIndex = 1;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            slotIndex = 2;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            slotIndex = 3;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            slotIndex = 4;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            slotIndex = 5;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            slotIndex = 6;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            slotIndex = 7;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            slotIndex = 8;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            slotIndex = 9;
        }

        // Only call SelectSlot if we found something
        if (slotIndex != -1)
        {
            SelectSlot(slotIndex);
        }
    }

    private void SelectSlot(int index)
    {
        if (index == currentIndex) return;

        currentIndex = index;
        ResetAllSlots();
        ApplySlotState(index, true);
        ShiftOtherSlots(index);
    }

    private void ResetAllSlots()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;

            slots[i].localScale = Vector3.one * unselectedScale;
            slots[i].anchoredPosition = originalPositions[i];

            Image img = slots[i].GetComponent<Image>();
            if (img)
            {
                Color c = img.color;
                c.a = unselectedAlpha;
                img.color = c;
            }
        }
    }

    private void ApplySlotState(int index, bool selected)
    {
        if (slots[index] == null) return;

        slots[index].localScale = Vector3.one * (selected ? selectedScale : unselectedScale);
        slots[index].anchoredPosition = originalPositions[index] + new Vector2(0f, selected ? selectedYOffset : 0f);

        Image img = slots[index].GetComponent<Image>();
        if (img)
        {
            Color c = img.color;
            c.a = selected ? selectedAlpha : unselectedAlpha;
            img.color = c;
        }
    }

    private void ShiftOtherSlots(int selectedIndex)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null || i == selectedIndex) continue;

            Vector2 offset = Vector2.zero;
            if (i < selectedIndex) offset.x = -xShift;
            else if (i > selectedIndex) offset.x = xShift;

            slots[i].anchoredPosition += offset;
        }
    }
}
