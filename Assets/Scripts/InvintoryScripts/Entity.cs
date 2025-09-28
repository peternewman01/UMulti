using System;
using System.Linq;
using System.Reflection;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(NetworkObject))]
public abstract class Entity : NetworkBehaviour
{
    private int objectID;
    private string objectName;
    [SerializeField] protected bool playerInArea = false;

    public Sprite objectSprite;

    public Entity() { }

    public void pickup(Invintory invintory)
    {
        invintory.AddItem(objectID, 1);
        Destroy(gameObject);
    }

    public abstract void Interact();

    private void Awake()
    {
        Type baseType = typeof(Entity);
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