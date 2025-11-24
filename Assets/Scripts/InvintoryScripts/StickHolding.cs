using System.Runtime.Serialization;
using Unity.VisualScripting;
using UnityEngine;

public class StickHolding : MonoBehaviour
{
    public Slot holdingSlot;
    public bool canHit = false;
    public bool wasHolding = false;
    public GameObject stick;

    private void Start()
    {
        ControlPanel cp = GetComponent<ControlPanel>();
        holdingSlot = cp.slotHolding;

        canHit = false;
        stick.SetActive(false);
    }

    private void Update()
    {
        /*        if(holdingSlot.isFilled() != wasHolding)
                {
                    wasHolding = holdingSlot.isFilled();

                    if (holdingSlot.itemText.text == Stick.ObjectName)
                    {
                        canHit = true;
                        stick.SetActive(true);
                    }
                    else
                    {
                        canHit = false;
                        stick.SetActive(false);
                    }
                }*/

        if (ItemManager.GetItem(holdingSlot.getObjectID()).name == "Stick")
        {
            stick.SetActive(true);
        }
        else
        {
            stick.SetActive(false);   
        }
    }
}
