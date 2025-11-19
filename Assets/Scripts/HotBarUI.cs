using UnityEngine;
using UnityEngine.InputSystem;
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

    [Header("Cooldown System")]
    [SerializeField] private float[] cooldownTimes = new float[10]; //replace with cooldowns from ability system
    private float[] cooldownRemaining = new float[10];
    private Slider[] cooldownSliders;

    [SerializeField] private float cooldownAlpha = 0.3f;
    [SerializeField] private float fadeSpeed = 6f;

    private InputAction one;
    private InputAction two;
    private InputAction three;
    private InputAction four;
    private InputAction five;
    private InputAction six;
    private InputAction seven;
    private InputAction eight;
    private InputAction nine;
    private InputAction zero;
    public PlayerManager player;

    private Vector2[] originalPositions;
    private int currentIndex = -1;

    void Start()
    {
        one = InputSystem.actions.FindAction("One");
        two = InputSystem.actions.FindAction("Two");
        three = InputSystem.actions.FindAction("Three");
        four = InputSystem.actions.FindAction("Four");
        five = InputSystem.actions.FindAction("Five");
        six = InputSystem.actions.FindAction("Six");
        seven = InputSystem.actions.FindAction("Seven");
        eight = InputSystem.actions.FindAction("Eight");
        nine = InputSystem.actions.FindAction("Nine");
        zero = InputSystem.actions.FindAction("Zero");
        CacheOriginalPositions();
        ResetAllSlots();

        cooldownSliders = new Slider[slots.Length];
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
            {
                cooldownSliders[i] = slots[i].GetComponentInChildren<Slider>(true);
                if (cooldownSliders[i] != null)
                {
                    cooldownSliders[i].minValue = 0f;
                    cooldownSliders[i].maxValue = 1f;
                    cooldownSliders[i].value = 0f;
                }
            }
        }

    }

    void Update()
    {
        CheckInput();
        UpdateCooldowns();
    }

    public void TriggerAbility()
    {
        if (cooldownRemaining[currentIndex] > 0f)
        {
            player.abilityIndex = -1;
            return;
        }

        //check ability to trigger here then pass back to playermanager?

        cooldownRemaining[currentIndex] = cooldownTimes[currentIndex];
        if (cooldownSliders[currentIndex] != null)
            cooldownSliders[currentIndex].value = 1f;

        player.abilityIndex = -1;
        currentIndex = -1;
        ResetAllSlots();
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

        if (one.WasPressedThisFrame())
        {
            slotIndex = 0;
        }
        else if (two.WasPressedThisFrame())
        {
            slotIndex = 1;
        }
        else if (three.WasPressedThisFrame())
        {
            slotIndex = 2;
        }
        else if (four.WasPressedThisFrame())
        {
            slotIndex = 3;
        }
        else if (five.WasPressedThisFrame())
        {
            slotIndex = 4;
        }
        else if (six.WasPressedThisFrame())
        {
            slotIndex = 5;
        }
        else if (seven.WasPressedThisFrame())
        {
            slotIndex = 6;
        }
        else if (eight.WasPressedThisFrame())
        {
            slotIndex = 7;
        }
        else if (nine.WasPressedThisFrame())
        {
            slotIndex = 8;
        }
        else if (zero.WasPressedThisFrame())
        {
            slotIndex = 9;
        }

        if (slotIndex != -1)
        {
            if (cooldownRemaining != null && slotIndex >= 0 && slotIndex < cooldownRemaining.Length && cooldownRemaining[slotIndex] > 0f)
                return;
            SelectSlot(slotIndex);
        }
    }

    private void UpdateCooldowns()
    {
        for (int i = 0; i < cooldownRemaining.Length; i++)
        {
            bool isCooling = cooldownRemaining[i] > 0f;

            if (isCooling)
            {
                cooldownRemaining[i] -= Time.deltaTime;
                float normalized = Mathf.Clamp01(cooldownRemaining[i] / cooldownTimes[i]);

                if (cooldownSliders[i] != null)
                    cooldownSliders[i].value = normalized;
            }

            if (slots[i] != null)
            {
                Image img = slots[i].GetComponent<Image>();
                if (img)
                {
                    float targetAlpha = isCooling ? cooldownAlpha : unselectedAlpha;
                    Color c = img.color;
                    c.a = Mathf.Lerp(c.a, targetAlpha, Time.deltaTime * fadeSpeed);
                    img.color = c;
                }
            }

            //hard reset
            if (cooldownRemaining[i] <= 0f)
            {
                cooldownRemaining[i] = 0f;
                if (cooldownSliders[i] != null)
                    cooldownSliders[i].value = 0f;
            }
        }
    }



    private void SelectSlot(int index)
    {
        if (index == currentIndex) return;

        currentIndex = index;
        player.abilityIndex = currentIndex;
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
