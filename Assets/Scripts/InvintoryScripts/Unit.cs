using UnityEngine;

[System.Serializable]
public class Unit
{
    public Object obj;
    public int count;

    public Unit() { }
    public Unit(Object obj, int count)
    {
        this.obj = obj;
        this.count = count;
    }
}