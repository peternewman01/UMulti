using Unity.VisualScripting;
using UnityEngine;

public class StickHolding : MonoBehaviour
{
    public Slot holdingSlot;
    public bool canHit = false;
    public bool wasHolding = false;
    public GameObject stick;

    private void Update()
    {
        if(holdingSlot.isFilled() != wasHolding)
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
        }
    }
}
