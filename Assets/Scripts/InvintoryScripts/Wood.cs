using UnityEngine;


public class Wood : Object
{
    public override void Initialize()
    {
        objectID = (int)Objects.WOOD;
        objectName = "Wood";
    }

    protected override void Interact()
    {
        targetInvintory.AddObject(this, 1);
        Destroy(gameObject);
    }

    private void Update()
    {
        if (playerInArea)
        {
            Interact();
        }
    }
}
