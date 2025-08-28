using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Entity : NetworkBehaviour
{
    [SerializeField] private int hp;
    public int Health
    {
        get => hp;
        set
        {
            if(hp > value)
            {
                onHurt();
                hp = value;
            }
            else if(hp < value)
            {
                onHeal();
                hp = value;
            }

            if(hp <= 0)
            {
                onDeath();
            }
        }
    }

    public abstract void onHurt();
    public abstract void onHeal();
    public abstract void onDeath();
}
