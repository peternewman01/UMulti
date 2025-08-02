using System;
using System.Linq;
using System.Reflection;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

enum Objects
{
    TABLE = 0,
    WOOD,
    STICK,
    COUNT
}

[RequireComponent(typeof(SphereCollider))]
public abstract class Object : NetworkBehaviour
{
    [SerializeField] protected int objectID;
    [SerializeField] protected string objectName;
    [SerializeField] protected Invintory targetInvintory;
    [SerializeField] protected bool playerInArea = false;

    private float pickupDist = 3f;

    public int ObjectID => objectID;
    public string ObjectName => objectName;

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

    public Object() { }

    private void Reset()
    {
        SphereCollider sc = GetComponent<SphereCollider>();
        sc.isTrigger = true;
        float scaledDist = pickupDist/ ((transform.localScale.x + transform.localScale.y + transform.localScale.z)/3);
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
}