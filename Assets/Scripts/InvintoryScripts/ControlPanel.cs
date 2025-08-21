using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ControlPanel : MonoBehaviour
{
    public GameObject slot;
    public Transform startPosition;
    [SerializeField] private List<Slot> openSlots = new List<Slot>();
    [SerializeField] private List<Slot> filledSlots = new List<Slot>();

    [SerializeField] private int slotSpawnCount = 63;
    [SerializeField] private int targetSlot = 0;

    [Header("Player Stuff")]
    public PlayerManager playerManager;
    public Invintory invintory;

    private void Start()
    {
        for(int i = 0; i <  slotSpawnCount; i++)
        {
            openSlots.Add(Instantiate(slot, startPosition).GetComponent<Slot>());
        }
    }

    public bool AddObjects(Object obj, int count)
    {
        if(slotSpawnCount - targetSlot < count)
        {
            return false;
        }

        for(int i = 0; i < count; i++)
        {
            openSlots.First().SetItem(obj.objectSprite, obj.objectName, obj.objectID);
            filledSlots.Add(openSlots.First());
            openSlots.Remove(openSlots.First());
            targetSlot++;
        }

        return true;
    }

    public bool RemoveObjects(Object obj, int count)
    {
        int currentlyFound = 0;
        foreach(Slot slot in filledSlots)
        {
            if(slot.itemText.text == obj.objectName)
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
