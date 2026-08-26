using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Fire Mode Settings")]
    public float fireRate = 0.1f;
    public int magazineSize = 30;
    public int energyPerShot = 1;
    public float reloadTime = 2f;
    public FireMode fireMode = FireMode.Single;
    public int shotgunPellets = 8;
    public float shotgunSpread = 10f;
    [Range(0, 1)] public float flashEffectChance = 0.25f;

    [Header("Aiming Settings")]
    public float aimSwayScale = 0.5f;
    public float aimRotationScale = 0.5f;
    public float aimBobScale = 0.3f;
    public Vector3 aimPositionOffset = new Vector3(0.1f, -0.1f, 0.2f);
    public Vector3 aimRotationOffset = Vector3.zero;
    public float aimRecoilScale = 0.1f;
    public float aimKickRecoilScale = 0.025f;
    public float aimShakeMultiplier = 1.5f;
    public float aimFOVSpeed = 8f;
    public float aimFOVMultiplier = 0.8f;

    [Header("Camera Effects")]
    public float shakeForceMultiplier = 1f;
    public float weaponKickForce = 0.1f;
    public float weaponKickForceHorizontal = 0.1f;
    public float recoilReturnSpeed = 5f;

    [Header("Recoil (Position)")]
    public Vector3 recoilPositionKick = new Vector3(0, 0, -0.05f);
    public float recoilPositionReturnSpeed = 8f;

    [Header("Shell Ejection")]
    public float shellEjectForce = 5f;
    public float shellEjectTorque = 100f;
    public float shellLifetime = 5f;

    [Header("References")]
    public GameObject projectilePrefab;
    public GameObject muzzleFlash;
    public GameObject muzzleSmoke;
    public AudioClip shotSound;
    public AudioClip reloadSound;
    public GameObject shellPrefab;

    public enum FireMode { Single, Auto, Shotgun }
}
