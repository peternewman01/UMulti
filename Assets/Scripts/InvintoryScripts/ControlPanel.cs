using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public class ControlPanel : MonoBehaviour
{
    public GameObject slot;
    public Transform startPosition;
    public List<Slot> openSlots = new List<Slot>();
    public List<Slot> filledSlots = new List<Slot>();

    public Dictionary<Vector2Int, Slot> allSlots = new Dictionary<Vector2Int, Slot>();

    [SerializeField] private int slotSpawnCount = 63;
    [SerializeField] private int columns = 9;

    [SerializeField] private int targetSlot = 0;

    [Header("Player Stuff")]
    public PlayerManager playerManager;
    public Invintory invintory;

    public Slot slotHolding;

    private void Start()
    {
        for (int i = 0; i <  slotSpawnCount; i++)
        {
            Slot s = Instantiate(slot, startPosition).GetComponent<Slot>();
            s.ui = this;

            int x = i % columns;
            int y = i / columns;
            Vector2Int pos = new Vector2Int(x, y);
            s.pos = pos;

            allSlots.Add(pos, s);
            openSlots.Add(s);
        }
        allSlots.Add(-Vector2Int.one, slotHolding);
        allSlots[-Vector2Int.one].ui = this;
        allSlots[-Vector2Int.one].pos = -Vector2Int.one;
        openSlots.Add(allSlots[-Vector2Int.one]);
    }

    public bool AddObjects(Item obj, int count)
    {
        if(slotSpawnCount - targetSlot < count)
        {
            return false;
        }

        for(int i = 0; i < count; i++)
        {
            openSlots.First().SetItem(obj.GetSprite(), obj.name, ItemManager.GetID(obj));
            filledSlots.Add(openSlots.First());
            openSlots.Remove(openSlots.First());    
            targetSlot++;
        }

        return true;
    }

    public bool RemoveObjects(Entity obj, int count)
    {
        int currentlyFound = 0;
        foreach(Slot slot in filledSlots)
        {
            if(slot.itemText.text == obj.name)
            {
                slot.ResetItem();
                currentlyFound++;

                if(currentlyFound == count)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool RemoveObjects(int id, int count)
    {
        Slot[] removeSlots = new Slot[count];
        int currentSlot = 0;

        int currentlyFound = 0;
        foreach (Slot slot in filledSlots)
        {
            if (slot.objectID == id)
            {
                slot.ResetItem();
                currentlyFound++;

                openSlots.Insert(0, slot);
                removeSlots[currentSlot] = slot;
                currentSlot++;

                if (currentlyFound == count) break;
            }
        }

        foreach (Slot slot in removeSlots)
        {
            filledSlots.Remove(slot);
        }

        if (currentlyFound == count)
        {
            return true;
        }
        return false;
    }
}
