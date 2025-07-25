using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class Invintory : MonoBehaviour
{
    [SerializeField] public Dictionary<int, Unit> Stuff = new Dictionary<int, Unit>();

    public void AddObject(Object obj, int count)
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

    public void RemoveObject(Object obj, int count)
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

    public bool Has(Object obj, int count)
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
