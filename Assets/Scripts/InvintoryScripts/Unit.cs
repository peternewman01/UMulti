using System;
using UnityEngine;

[Serializable]
public class Unit
{
    public UseEntity.Entity obj;
    public int count;

    public Unit() { }
    public Unit(UseEntity.Entity obj, int count)
    {
        this.obj = obj;
        this.count = count;
    }
}