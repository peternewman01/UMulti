using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;

namespace UseEntity
{
    public class Health : Entity
    {
        //[SerializeField] private int hp;
        [SerializeField] private NetworkVariable<float> hp = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public event Action OnHurt;
        public event Action OnHeal;
        public event Action OnDeath;



        public float HP
        {
            get => hp.Value;
            set
            {
            }
        }

        public void Hurt(float damage)
        {
            hp.Value -= damage;
            OnHurt?.Invoke();
            if (hp.Value < 0) OnDeath?.Invoke();
        }

        public void Heal(float heal)
        {
            hp.Value += heal;
            OnHeal?.Invoke();
        }

        public void SetHP(float newHP)
        {
            if (hp.Value >= newHP) OnHurt?.Invoke();
            else if (hp.Value < newHP) OnHeal?.Invoke();

            hp.Value = newHP;
        }
    }

}
