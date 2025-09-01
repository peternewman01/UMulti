using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class Entity : NetworkBehaviour
{
    //[SerializeField] private int hp;
    [SerializeField] private NetworkVariable<int> hp = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public event Action OnHurt;
    public event Action OnHeal;
    public event Action OnDeath;

    public bool activate = true;
    protected Entity pointEntity;
    protected List<Entity> owned = new List<Entity>();

    public int Health
    {
        get => hp.Value;
        set
        {
            if (hp.Value != value)
            {
                if (hp.Value > value)
                {
                    OnHurt?.Invoke();
                }
                else if (hp.Value < value)
                {
                    OnHeal?.Invoke();
                }

                if (value  <= 0)
                {
                    OnDeath?.Invoke();
                }

                if (activate)
                {
                    hp.Value = value;
                    foreach (Entity e in owned)
                    {
                        e.Health = value;
                    }

                }
                else if (pointEntity != null)
                {
                    hp.Value = value;
                    pointEntity.Health = value;
                }
            }

        }
    }

    void Start()
    {
        foreach (var entity in GetComponents<Entity>())
        {
            if (entity != this && activate)
            {
                entity.Health = this.hp.Value;

                entity.activate = false;
                entity.pointEntity = this;

                owned.Add(entity);
            }
        }
    }
}
