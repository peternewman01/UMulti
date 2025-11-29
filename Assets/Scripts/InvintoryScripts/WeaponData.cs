using UnityEngine;

public class WeaponData : MonoBehaviour
{
    [SerializeField] private int damage;

    public int GetDamage() => damage;
}
