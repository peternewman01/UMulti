using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Entity : NetworkBehaviour
{
    [SerializeField] private int hp;

    public event Action OnHurt;
    public event Action OnHeal;
    public event Action OnDeath;

    public bool activate = true;
    protected Entity pointEntity;
    protected List<Entity> owned = new List<Entity>();

    public int Health
    {
        get => hp;
        set
        {
            if (hp != value)
            {
                if (hp > value)
                {
                    OnHurt?.Invoke();
                }
                else if (hp < value)
                {
                    OnHeal?.Invoke();
                }

                if (value <= 0)
                {
                    OnDeath?.Invoke();
                }

                if (activate)
                {
                    hp = value;
                    foreach (Entity e in owned)
                    {
                        e.Health = value;
                    }

                }
                else if (pointEntity != null)
                {
                    hp = value;
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
                entity.Health = this.hp;

                entity.activate = false;
                entity.pointEntity = this;

                owned.Add(entity);
            }
        }
    }

}
