using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Ability", menuName = "ScriptableObject/Ability/BaseAbility")]
public class Ability : ScriptableObject
{
    [SerializeField] private List<Ability> upgrades = new();
    [SerializeField] private GameObject abilityPrefab;
    [SerializeField] private TargettingType targettingType;
    private List<TargetSelecter> targetter;

    private void Awake()
    {
        if (targettingType.HasFlag(TargettingType.SELF)) targetter.Add(new Self());
        //if (targettingType.HasFlag(TargettingType.ON_COLLISION)) targetter.Add()
    }
}

[Flags]
public enum TargettingType 
{
    SELF = 1 << 0,
    ON_COLLISION = 1 << 1,
    RADIUS = 1 << 2,
    DIRECTED = 1 << 3,
}

[Serializable]
public abstract class TargetSelecter
{
    public abstract List<GameObject> GetTargets(GameObject caller);
}

public class Self : TargetSelecter
{
    public override List<GameObject> GetTargets(GameObject caller) => new() { caller };
}

public interface Activate
{
    public abstract void Activate();
}

public class ActiveAbility : Ability, Activate
{
    public void Activate()
    {
        throw new System.NotImplementedException();
    }
}


public static class FlagsHelper
{
    public static bool IsSet<T>(T flags, T flag) where T : struct
    {
        int flagsValue = (int)(object)flags;
        int flagValue = (int)(object)flag;

        return (flagsValue & flagValue) != 0;
    }

    public static void Set<T>(ref T flags, T flag) where T : struct
    {
        int flagsValue = (int)(object)flags;
        int flagValue = (int)(object)flag;

        flags = (T)(object)(flagsValue | flagValue);
    }

    public static void Unset<T>(ref T flags, T flag) where T : struct
    {
        int flagsValue = (int)(object)flags;
        int flagValue = (int)(object)flag;

        flags = (T)(object)(flagsValue & (~flagValue));
    }
}