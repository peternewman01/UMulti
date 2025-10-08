using Unity.Netcode;
using UnityEngine;

public class hurtEntities : Entity
{
    [SerializeField] private int damage = 1;

    private void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.tag != "Player")
        {
            if (col.gameObject.GetComponent<HurtableEntity>() is HurtableEntity entity)
            {
                entity.Health -= damage;
            }
            else if(col.gameObject.GetComponentInParent<HurtableEntity>() is HurtableEntity parentEntity)
            {
                parentEntity.Health -= damage;
            }
        }
    }

    public void setDamage(int damage)
    {
        if(damage >= 0)
        {
            this.damage = damage;
        }
    }
}
