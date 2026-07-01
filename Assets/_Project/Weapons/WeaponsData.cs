using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Weapons/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    public string weaponName = "Rifle";

    [Header("Damage")]
    public int damage = 25;

    [Header("Ammo")]
    public int magazineSize = 30;
    public int reserveAmmo = 120;
    public float reloadTime = 2f;

    [Header("Fire Rate")]
    [Tooltip("Rounds per minute")]
    public float fireRateRPM = 600f;

    [Header("Recoil Pattern")]
    [Tooltip("Each entry = one shot's recoil. X = horizontal, Y = vertical (up).")]
    public Vector2[] recoilPattern = new Vector2[]
    {
        new Vector2( 0.0f, 0.5f),
        new Vector2( 0.0f, 0.5f),
        new Vector2( 0.0f, 0.6f),
        new Vector2( 0.1f, 0.5f),
        new Vector2(-0.1f, 0.5f),
        new Vector2( 0.3f, 0.4f),
        new Vector2( 0.4f, 0.3f),
        new Vector2( 0.3f, 0.3f),
        new Vector2( 0.0f, 0.3f),
        new Vector2( 0.0f, 0.3f),
    };
}