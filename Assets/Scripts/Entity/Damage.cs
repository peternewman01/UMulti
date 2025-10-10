using Unity.Netcode;
using UnityEngine;

namespace UseEntity
{
    public class Damage : Entity
    {
        [SerializeField] private float damage = 1f;

        private void OnTriggerEnter(Collider col)
        {
            if (col.gameObject.tag != "Player")
            {
                if (col.gameObject.GetComponent< UseEntity.Health> () is Health entity)
                {
                    entity.HP -= damage;
                }
                else if (col.gameObject.GetComponentInParent< UseEntity.Health> () is Health parentEntity)
                {
                    parentEntity.HP -= damage;
                }
            }
        }

        public void setDamage(int damage)
        {
            if (damage >= 0)
            {
                this.damage = damage;
            }
        }
    }
}