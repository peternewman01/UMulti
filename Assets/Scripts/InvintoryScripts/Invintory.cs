using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class Invintory : MonoBehaviour
{
    public Dictionary<int, Unit> Stuff = new Dictionary<int, Unit>();
    public GameObject InvintorySlotPrefab;

    [SerializeField] private RectTransform startPos;
    [SerializeField] private Vector2 offset = Vector2.one;  
    public void AddObject(Object obj, int count)
    {
        if (Stuff.ContainsKey(obj.ObjectID))
        {
            Stuff[obj.ObjectID].count += count;
        }
        else
        {
            Stuff.Add(obj.ObjectID, new Unit(obj, count));

            GameObject invintoryButtonGameObject = Instantiate(InvintorySlotPrefab, startPos.gameObject.transform);
            Debug.Log(startPos.rect.width);
            invintoryButtonGameObject.transform.position = new Vector3(startPos.position.x + (offset.x * startPos.rect.width) + (offset.x * 5), startPos.position.y + (offset.y * -startPos.rect.height) + (offset.y * 5), startPos.position.z);

            offset.x++;
            if(offset.x > 5)
            {
                offset.x = 0;
                offset.y++;
            }

            InvintorySlotButton invintoryButton = invintoryButtonGameObject.GetComponent<InvintorySlotButton>();
            invintoryButton.Target = obj.ObjectID;
            invintoryButton.Invintory = this;
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

                if(offset.x == 0)
                {
                    offset.x = 5;
                    offset.y -= 1;
                }
                else
                {
                    offset.x -= 1;
                }

                if(offset.x < 0 || offset.y <0)
                {
                    offset = Vector2.zero;
                }
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
