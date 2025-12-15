using NUnit.Framework;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WeaponData : MonoBehaviour
{
    public const int WEIGHT_CLAMP = 10;

    [SerializeField] private int lightDamage = 1;
    [SerializeField] private int heavyDamage = 3;
    [SerializeField, UnityEngine.Range(0, 5)] private float weaponReach = 0.5f;
    [SerializeField, UnityEngine.Range(-WEIGHT_CLAMP, WEIGHT_CLAMP)] private float weaponWeight;
    [SerializeField] private float weaponPreslashTimerLight = 0.1f;
    [SerializeField] private float weaponPreslashTimerHeavy = 0.2f;


    [SerializeField] private GameObject slashLightVFX;
    [SerializeField] private GameObject slashDashLightVFX;
    [SerializeField] private GameObject slashHeavyVFX;
    [SerializeField] private GameObject slashDashHeavyVFX;

    [SerializeField] private List<float> lightAttackTiming = new List<float>();
    [SerializeField] private int lightIndex = 0;
    [SerializeField] private List<float> heavyAttackTiming = new List<float>();
    [SerializeField] private int heavyIndex = 0;



    public int GetLightDamage() => lightDamage;
    public int GetHeavyDamage() => heavyDamage;
    public float GetReach() => weaponReach;
    public float GetWeight() => weaponWeight;
    public float GetPreslash(bool isLightAttack)
    {
        if(isLightAttack)
            return weaponPreslashTimerLight;
        else
            return weaponPreslashTimerHeavy;
    }
    public GameObject GetLightVFX() => slashLightVFX;
    public GameObject GetDashingLightVFX() => slashDashLightVFX;
    public GameObject GetHeavyVFX() => slashHeavyVFX;
    public GameObject GetDashingHeavyVFX() => slashHeavyVFX;

    public void SetParent(Transform p) {transform.parent = p;}

    public float GetNextLightAttackDelay()
    {
        float delay = lightAttackTiming[lightIndex];
        lightIndex++;
        if(lightIndex >= lightAttackTiming.Count)
        {
            lightIndex = 0;
        }

        return delay;
    }

    public float GetNextHeavyAttackDelay()
    {
        float delay = heavyAttackTiming[heavyIndex];
        heavyIndex++;
        if (heavyIndex >= heavyAttackTiming.Count)
        {
            heavyIndex = 0;
        }

        return delay;
    }

    public void ResetAttackIndex()
    {
        lightIndex = 0;
        heavyIndex = 0;
    }

}
