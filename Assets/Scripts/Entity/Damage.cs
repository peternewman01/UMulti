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

                if (col.gameObject.GetComponent<Health> () is Health entity)
                {
                    entity.Hurt(damage);
                }
                else if (col.gameObject.GetComponentInParent<Health> () is Health parentEntity)
                {
                    parentEntity.Hurt(damage);
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

        public float getDamage() => damage;
    }
}