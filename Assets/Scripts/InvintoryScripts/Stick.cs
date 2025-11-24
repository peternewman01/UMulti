using UnityEngine;


public class Stick : MonoBehaviour
{
    /*    public static int ObjectID = -1;
    public static string ObjectName = "";

    private void Start()
    {
        if (ObjectID == -1)
        {
            ObjectID = objectID;
            ObjectName = objectName;
        }
    }
    public override void Interact(PlayerManager interacter)
    {
        interacter.GetInventory().AddItem(id, 1);
        Destroy(gameObject);
    }*/

    [SerializeField] private int objectID;
    [SerializeField] private Item stickItem;

    private void Start()
    {
        objectID = ItemManager.GetID(stickItem);
    }
}