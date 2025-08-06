using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class Invintory : MonoBehaviour
{
    [SerializeField] public Dictionary<int, Unit> Stuff = new Dictionary<int, Unit>();

    public void AddObject<T>(T obj, int count) where T : Object
    {
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
        RemoveObject(obj.getID(), count);
    }
    public void RemoveObject(int id, int count)
    {
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
