using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public class ControlPanel : MonoBehaviour
{
    [SerializeField] private GameObject slot;
    [SerializeField] private Transform startPosition;
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

    public Slot MovingSlot;

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

    public bool AddObject(Item obj)
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
                            filledSlots.Add(slot);
                            openSlots.Remove(slot);
                        }
                    }
                }

                RectTransform sourceRect = sourceSlot.gameObject.GetComponent<RectTransform>();
                sourceRect.localScale = new Vector2(slotArea.x, slotArea.y);

                return true;
            }
        }

        return false;
    }

    public bool RemoveObject(Item obj)
    {
        foreach (Slot slot in filledSlots)
        {
            if (slot.getObjectID() == ItemManager.GetID(obj))
            {
                slot.ResetItem();
                RectTransform removeSlot = MovingSlot.gameObject.GetComponent<RectTransform>();
                removeSlot.localScale = new Vector2(MovingSlot.getSize().x, MovingSlot.getSize().y);
            }
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

    public void SetMovingSlot(Slot slot)
    {
        MovingSlot = slot;
    }

    public bool MoveMovingSlotTo(Slot slot)
    {
        if(MovingSlot)
        {
            if (CheckSlotAreaOnGrid(slot.pos, MovingSlot.getSize()))
            {
                Slot sourceSlot = slot;
                //size should stay pretty small, checking somwhere between 1 and 9 slots
                for (int x = 0; x < MovingSlot.getSize().x; x++)
                {
                    for (int y = 0; y < MovingSlot.getSize().y; y++)
                    {
                        if (allSlots.TryGetValue(new Vector2Int(sourceSlot.pos.x + x, sourceSlot.pos.y + y), out var tempSlot))
                        {
                            tempSlot.SetItem(sourceSlot, ItemManager.GetItem(MovingSlot.getObjectID()));
                            filledSlots.Add(tempSlot);
                            openSlots.Remove(tempSlot);
                        }
                    }
                }

                RectTransform sourceRect = sourceSlot.gameObject.GetComponent<RectTransform>();
                sourceRect.localScale = new Vector2(MovingSlot.getSize().x, MovingSlot.getSize().y);

                MovingSlot.ResetItem();
                sourceRect = MovingSlot.gameObject.GetComponent<RectTransform>();
                sourceRect.localScale = new Vector2(MovingSlot.getSize().x, MovingSlot.getSize().y);




                return true;
            }
        }
        
        return false;
    }
}
