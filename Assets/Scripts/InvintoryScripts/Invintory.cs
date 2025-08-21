using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class Invintory : MonoBehaviour
{
    public Dictionary<int, Unit> Stuff = new Dictionary<int, Unit>();
    public ControlPanel ui;

    public void AddObject<T>(T obj, int count) where T : Object
    {
        if(!ui.AddObjects(obj, count))
        {
            return;
        }

        if (Stuff.ContainsKey(obj.getID()))
        {
            Stuff[obj.getID()].count += count;
        }
        else
        {
            Stuff.Add(obj.getID(), new Unit(obj, count));
        }

        Debug.Log("Player has " + Stuff[obj.getID()].count + " " + Stuff[obj.getID()].obj.getName());
    }

    public void RemoveObject<T>(T obj, int count) where T : Object
    {
        ui.RemoveObjects(obj, count);

        if (Stuff.ContainsKey(obj.objectID))
        {
            Stuff[obj.objectID].count -= count;
            if (Stuff[obj.objectID].count <= 0)
            {
                Stuff.Remove(obj.objectID);
            }
        }
    }

    public void RemoveObject(int id, int count)
    {
        ui.RemoveObjects(id, count);

        if (Stuff.ContainsKey(id))
        {
            Stuff[id].count -= count;
            if (Stuff[id].count <= 0)
            {
                Stuff.Remove(id);
            }
        }
    }

    public bool Has<T>(T obj, int count) where T : Object
    {
        return Has(obj.getID(), count);
    }

    public bool Has(int id, int count)
    {
        if(!Stuff.ContainsKey(id))
            return false;

        if(Stuff[id].count < count)
            return false;

        return true;
    }
}
