using UnityEngine;

enum Objects
{
    TABLE = 0,
    WOOD,
    STICK,
    COUNT
}

[RequireComponent(typeof(SphereCollider))]
public abstract class Object : MonoBehaviour
{
    [SerializeField] protected int objectID;
    [SerializeField] protected string objectName;
    [SerializeField] protected Invintory targetInvintory;
    [SerializeField] protected bool playerInArea = false;

    private float pickupDist = 3f;

    public int ObjectID => objectID;
    public string ObjectName => objectName;

    public virtual void Initialize() { }
    protected virtual void Interact() { }

    public Object() { }

    private void Reset()
    {
        Initialize();

        SphereCollider sc = GetComponent<SphereCollider>();
        sc.isTrigger = true;
        float scaledDist = pickupDist/ ((transform.localScale.x + transform.localScale.y + transform.localScale.z)/3);
        sc.radius = scaledDist;
    }

    private void OnTriggerEnter(Collider col)
    {
        Invintory invTemp = col.gameObject.GetComponentInParent<Invintory>();
        if (invTemp != null)
        {
            targetInvintory = invTemp;
            playerInArea = true;
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if(targetInvintory != null)
        {
            targetInvintory = null;
            playerInArea = false;
        }
    }
}