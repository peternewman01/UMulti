using UnityEngine;

public class Totem : MonoBehaviour
{
    private Table table;
    public GameObject holding;

    private void Start()
    {
        table = GetComponentInParent<Table>();
    }
    private void OnTriggerEnter(Collider obj)
    {
        if(obj.GetComponent<Object>() is Object o)
        {
            table.addTotemHolding(o);
            holding = o.gameObject;
        }
    }

    private void OnTriggerExit(Collider obj)
    {
        if(obj.GetComponent<Object>() is Object o)
        {
            table.removeTotemHolding(o);
            holding = null;
        }
    }
}
