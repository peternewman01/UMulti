using UnityEngine;


public class Wood : Object
{
    private void Start()
    {
        Debug.Log(objectName);
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
            //TODO: I need to make this so the player chooses to pick up the wood
            //maybe create an order? like ticket system from a script on the player?
            //Pick up closest object? instead of moving from the perspective of the wood?
        }
    }
}
