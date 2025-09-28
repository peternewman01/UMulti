using System;
using UnityEngine;

[Serializable]
public class Unit
{
    public Entity obj;
    public int count;

    public Unit() { }
    public Unit(Entity obj, int count)
    {
        this.obj = obj;
        this.count = count;
    }
}