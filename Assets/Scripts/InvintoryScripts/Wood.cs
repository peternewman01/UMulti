using UnityEngine;


public class Wood : Object
{
    public override void Interact()
    {
        targetInvintory.AddObject(this, 1);
        Destroy(gameObject);
    }
}
