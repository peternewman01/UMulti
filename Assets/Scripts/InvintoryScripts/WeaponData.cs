using UnityEngine;

public class WeaponData : MonoBehaviour
{
    [SerializeField] private int damage;
    [SerializeField, Range(0, 20)] private int weaponReach;
    [SerializeField, Range(0, 200)] private int weaponWeight;
    [SerializeField] private int weaponPreslashTimer;
    [SerializeField] private GameObject slashLightVFX;
    [SerializeField] private GameObject slashDashLightVFX;
    [SerializeField] private GameObject slashHeavyVFX;
    [SerializeField] private GameObject slashDashHeavyVFX;


    public int GetDamage() => damage;
    public int GetReach() => weaponReach;
    public int GetWeight() => weaponWeight;
    public int GetPreslash() => weaponPreslashTimer;
    public GameObject GetLightVFX() => slashLightVFX;
    public GameObject GetDashingLightVFX() => slashDashLightVFX;
    public GameObject GetHeavyVFX() => slashHeavyVFX;
    public GameObject GetDashingHeavyVFX() => slashHeavyVFX;

}
