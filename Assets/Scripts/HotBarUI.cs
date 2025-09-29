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
        SelectSlot(0);
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

    private void CheckInput()
    {
        for (int i = 0; i < 10; i++)
        {
            bool pressed = (i < 9) ? Input.GetKeyDown(KeyCode.Alpha1 + i) : Input.GetKeyDown(KeyCode.Alpha0);
            if (pressed)
            {
                SelectSlot(i);
                break;
            }
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
