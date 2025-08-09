using System;
using System.Linq;
using System.Reflection;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public abstract class Object : NetworkBehaviour
{
    public int objectID;
    public string objectName;
    [SerializeField] protected Invintory targetInvintory;
    [SerializeField] protected bool playerInArea = false;

    private float dist = 3f;

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
    }

    private void Reset()
    {
        SphereCollider sc = GetComponent<SphereCollider>();
        sc.isTrigger = true;
        float scaledDist = dist/ ((transform.localScale.x + transform.localScale.y + transform.localScale.z)/3);
        sc.radius = scaledDist;
    }

    private void OnTriggerEnter(Collider col)
    {
        Invintory invTemp = col.gameObject.GetComponentInParent<Invintory>();
        if (invTemp != null)
        {
            targetInvintory = invTemp;
            playerInArea = true;
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if(targetInvintory != null)
        {
            targetInvintory = null;
            playerInArea = false;
        }
    }

    public string getName() { return objectName; }
    public int getID() { return objectID; }
}