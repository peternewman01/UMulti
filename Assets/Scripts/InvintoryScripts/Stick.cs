using UnityEngine;


public class Stick : Object
{
    public override void Initialize()
    {
        objectID = (int)Objects.STICK;
        objectName = "Stick";
    }

    protected override void Interact()
    {
    }

    private void Update()
    {
    }
}
