using System;
using System.Linq;
using System.Reflection;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public abstract class Object : NetworkBehaviour
{
    public int objectID;
    public string objectName;
    public Invintory Invintory;
    [SerializeField] protected bool playerInArea = false;

    public Sprite objectSprite;

    public Object() { }

    public void pickup(Invintory invintory)
    {
        invintory.AddObject(this, 1);
        Destroy(gameObject);
    }

    public virtual void Interact() { }

    private void Awake()
    {
        Type baseType = typeof(Object);
        var subTypes = Assembly.GetAssembly(baseType).GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(baseType));
        
        int id = 0;
        foreach (Type type in subTypes)
        {
            if(this.GetType() == type)
            {
                objectID = id;
                objectName = type.Name;
            }
            id++;
        }

        objectSprite = Resources.Load<Sprite>("ObjectThumbnails/BasicThumbnail");
    }

    public string getName() { return objectName; }
    public int getID() { return objectID; }
}