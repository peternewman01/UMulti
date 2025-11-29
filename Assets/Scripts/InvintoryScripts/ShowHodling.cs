using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.UI;
using UnityEngineInternal;
using UseEntity;

public class ShowHodling : MonoBehaviour
{
    [SerializeField] private Slot holdingSlot;
    [SerializeField] private PlayerManager playerManager;
    private GameObject holdingObject;

    [SerializeField] private bool showsHolding = false;
    public Transform parent;
    private void Start()
    {
        playerManager = GetComponentInParent<PlayerManager>();
        holdingSlot = playerManager.controlPanel.slotHolding;

        NetworkObject hold = GetComponent<NetworkObject>();
        hold.Spawn();
        hold.transform.parent = parent.parent;
    }

    private void Update()
    {
        transform.position = parent.transform.position;
        transform.rotation = parent.transform.rotation;

        if (holdingSlot != null)
        {
            if (holdingSlot.isFilled() && !showsHolding)
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
            else if (!holdingSlot.isFilled() && showsHolding)
            {
                showsHolding = false;
                Destroy(holdingObject);
                holdingObject = null;
            }

            showsHolding = holdingObject;
        }
    }

    public bool getShowsHolding() => showsHolding;

    public GameObject getHoldingObject() => holdingObject; 
}
