using Unity.Netcode;
using UnityEngine;

public class hurtEntities : NetworkBehaviour
{
    [SerializeField] private int damage = 1;

    private void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.tag != "Player")
        {
            if (col.gameObject.GetComponent<Entity>() is Entity entity)
            {
                entity.Health -= damage;
            }
            else if(col.gameObject.GetComponentInParent<Entity>() is Entity parentEntity)
            {
                parentEntity.Health -= damage;
            }
        }
    }
}
