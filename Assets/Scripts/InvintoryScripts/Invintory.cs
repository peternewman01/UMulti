using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class Invintory : MonoBehaviour
{
    [SerializeField] public Dictionary<int, Unit> Stuff = new Dictionary<int, Unit>();

    public void AddObject<T>(T obj, int count) where T : Object
    {
        if (Stuff.ContainsKey(obj.ObjectID))
        {
            Stuff[obj.ObjectID].count += count;
        }
        else
        {
            Stuff.Add(obj.ObjectID, new Unit(obj, count));
        }

        Debug.Log("Player has " + Stuff[obj.ObjectID].count + " " + Stuff[obj.ObjectID].obj.ObjectName);
    }

    public void RemoveObject<T>(T obj, int count) where T : Object
    {
        RemoveObject(obj.ObjectID, count);
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
        return Has(obj.ObjectID, count);
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
