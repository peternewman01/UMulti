using System;
using System.Linq;
using System.Reflection;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Meat : Object
{
    public static int ObjectID = -1;
    public static string ObjectName = "";

    private void Start()
    {
        if (ObjectID == -1)
        {
            ObjectID = objectID;
            ObjectName = objectName;
        }
    }

    public override void Interact()
    {
        Invintory.AddObject(this, 1);
        Destroy(gameObject);
    }
}