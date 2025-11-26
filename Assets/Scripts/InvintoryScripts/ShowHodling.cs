using UnityEngine;
using UnityEngine.UI;
using UnityEngineInternal;
using UseEntity;

public class ShowHodling : MonoBehaviour
{
    [SerializeField] private Slot holdingSlot;
    [SerializeField] private PlayerManager playerManager;
    private GameObject holdingObject;

    private bool showsHolding = false;
    private void Start()
    {
        playerManager = GetComponentInParent<PlayerManager>();
        holdingSlot = playerManager.controlPanel.slotHolding;
    }

    private void Update()
    {
        if (holdingSlot != null)
        {
            if(holdingSlot.isFilled() && !showsHolding)
            {
                holdingObject = Instantiate(ItemManager.GetItem(holdingSlot.getObjectID()).GetWorldPrefab().gameObject, this.gameObject.transform);
                showsHolding = true;
                holdingObject.GetComponent<Grabbable>().enabled = false;
                Rigidbody rb = holdingObject.GetComponent<Rigidbody>();
                rb.useGravity = false;
                rb.isKinematic = false;
                rb.constraints = RigidbodyConstraints.FreezeAll; 
                
                Collider col = holdingObject.GetComponent<CapsuleCollider>();
                if (col != null)
                    col.enabled = false;

            }
            else if( !holdingSlot.isFilled() && showsHolding)
            {
                showsHolding = false;
                Destroy(holdingObject);
            }
        }
    }
}
