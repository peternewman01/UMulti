using System;
using System.Linq;
using System.Reflection;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public abstract class Object : NetworkBehaviour
{
    [SerializeField] protected static int objectID;
    [SerializeField] protected static string objectName;
    [SerializeField] protected Invintory targetInvintory;
    [SerializeField] protected bool playerInArea = false;

    private float dist = 3f;

    public static int ObjectID => objectID;
    public static string ObjectName => objectName;

    public Object() { }

    public void pickup(Invintory invintory)
    {
        invintory.AddObject(this, 1);
        Destroy(gameObject);
    }

    protected virtual void Interact() { }

    private void Awake()
    {
        Type baseType = typeof(Object);
        var subTypes = Assembly.GetAssembly(baseType).GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(baseType));

        int id = 0;
        foreach (Type type in subTypes)
        {
            var field = type.GetField("objectID", BindingFlags.NonPublic);
            field?.SetValue(null, id);
            id++;

            field = type.GetField("objectName", BindingFlags.NonPublic);
            field?.SetValue(null, type.Name);

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

    public string getName() { return ObjectName; }
    public int getID() { return ObjectID; }
}