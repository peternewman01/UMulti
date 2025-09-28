using UnityEngine;


public class Stick : Entity
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
