using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
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

    public bool run = false;

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

    private void Update()
    {
        if(run)
        {
            Debug.Log(CheckSlotAreaOnGrid(new Vector2Int(0, 0), Vector2Int.down) ? "area T" : "area F");
            run = false;
        }
    }

    public bool AddObjectOfSize(Item obj)
    {
        Vector2Int slotArea = obj.GetInventorySize();

        if(slotArea == Vector2Int.zero) return false;

        Slot sourceSlot = null;
        foreach (var item in openSlots)
        {
            if(CheckSlotAreaOnGrid(item.pos, slotArea))
            {
                sourceSlot = item;

                //size should stay pretty small, checking somwhere between 1 and 9 slots
                for (int x = 0; x < slotArea.x; x++)
                {
                    for (int y = 0; y < slotArea.y; y++)
                    {
                        if (allSlots.TryGetValue(new Vector2Int(sourceSlot.pos.x + x, sourceSlot.pos.y + y), out var slot))
                        {
                            slot.SetItem(sourceSlot, obj);
                        }
                    }
                }

                return true;
            }
        }

        return false;
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

    public bool RemoveObjects(Item obj, int count)
    {
        int currentlyFound = 0;
        foreach (Slot slot in filledSlots)
        {
            if (slot.getObjectID() == ItemManager.GetID(obj))
            {
                slot.ResetItem();
                currentlyFound++;

                if (currentlyFound == count)
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
            if (slot.getObjectID() == id)
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

    public bool CheckSlotOnGrid(Vector2Int pos)
    {
        if(allSlots.TryGetValue(pos, out var slot))
        {
            if(openSlots.Contains(slot))
            {
                return true;
            }
        }
        return false;
    }

    public bool CheckSlotAreaOnGrid(Vector2Int pos, Vector2Int slotArea)
    {
        for (int x = 0; x < slotArea.x; x++)
        {
            for (int y = 0; y < slotArea.y; y++)
            {
                Vector2Int checkPos = new Vector2Int(pos.x + x, pos.y + y);

                if (!CheckSlotOnGrid(checkPos))
                {
                    return false;
                }
            }
        }

        return true;
    }
}
