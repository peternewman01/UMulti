using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;

public class Invintory : MonoBehaviour
{
    public Dictionary<int, int> Stuff = new Dictionary<int, int>();

    public bool AddItem(int id, int count)
    {
        if(Stuff.TryAdd(id, count)) return true;

        if (Stuff.ContainsKey(id))
        {
            Stuff[id] += count;
            return true;
        }

        return false;
    }

    public virtual bool AddItem(ItemData data)
    {
        return AddItem(ItemManager.GetID(data.item), data.count);
    }

    public bool RemoveItem(int id, int count)
    {
        if(Stuff.ContainsKey(id))
        {
            if (Stuff[id] < count)
                return false;

            Stuff[id] -= count;

            if (Stuff[id] == 0)
                Stuff.Remove(id);

            return true;
        }

        return false;
    }

    public virtual bool RemoveItem(ItemData data)
    {
        return RemoveItem(ItemManager.GetID(data.item), data.count);
    }
    public int RemoveAllOfItem(int id)
    {
        Stuff.Remove(id, out int value);
        return value;
    }

    public virtual int RemoveAllOfItem(Item item)
    {
        return RemoveAllOfItem(ItemManager.GetID(item));
    }

    public virtual bool Has(ItemData data)
    {
        return Has(ItemManager.GetID(data.item), data.count);
    }

    public bool Has(int id, int count)
    {
        if(Stuff.ContainsKey(id) && Stuff[id] >= count)
            return true;

        return false;
    }
}
