using Unity.Netcode;
using UnityEngine;

public class WeaponData : MonoBehaviour
{
    public const int WEIGHT_CLAMP = 10;

    [SerializeField] private int lightDamage = 1;
    [SerializeField] private int heavyDamage = 3;
    [SerializeField, Range(0, 5)] private float weaponReach = 0.5f;
    [SerializeField, Range(-WEIGHT_CLAMP, WEIGHT_CLAMP)] private float weaponWeight;
    [SerializeField] private float weaponPreslashTimer = 0.1f;
    [SerializeField] private GameObject slashLightVFX;
    [SerializeField] private GameObject slashDashLightVFX;
    [SerializeField] private GameObject slashHeavyVFX;
    [SerializeField] private GameObject slashDashHeavyVFX;


    public int GetLightDamage() => lightDamage;
    public int GetHeavyDamage() => heavyDamage;
    public float GetReach() => weaponReach;
    public float GetWeight() => weaponWeight;
    public float GetPreslash() => weaponPreslashTimer;
    public GameObject GetLightVFX() => slashLightVFX;
    public GameObject GetDashingLightVFX() => slashDashLightVFX;
    public GameObject GetHeavyVFX() => slashHeavyVFX;
    public GameObject GetDashingHeavyVFX() => slashHeavyVFX;

    public void SetParent(Transform p) {transform.parent = p;}

}
