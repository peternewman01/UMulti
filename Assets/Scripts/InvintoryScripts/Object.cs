using UnityEngine;

enum Objects
{
    TABLE = 0,
    WOOD,
    COUNT
}

[RequireComponent(typeof(SphereCollider))]
public abstract class Object : MonoBehaviour
{
    [SerializeField] protected int objectID;
    [SerializeField] protected string objectName;
    [SerializeField] protected PlayerControler pc = null;
    [SerializeField] protected Invintory invintory;
    [SerializeField] protected bool playerInArea = false;

    private float pickupDist = 3f;

    public int ObjectID => objectID;
    public string ObjectName => objectName;

    public virtual void Initialize() { }
    protected virtual void Interact() { }

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
        PlayerControler pcTemp = col.gameObject.GetComponent<PlayerControler>();
        if (pcTemp != null)
        {
            pc = pcTemp;
            invintory = pc.GetComponent<Invintory>();
            playerInArea = true;
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if(pc != null)
        {
            pc = null;
            invintory = null;
            playerInArea = false;
        }
    }
}