using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class WeaponController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private WeaponData weaponData;

    [Header("Inputs")]
    [SerializeField] private InputActionReference aimAction;
    [SerializeField] private InputActionReference attackAction;
    [SerializeField] private InputActionReference reloadAction;

    [Header("Transforms")]
    [SerializeField] private Transform muzzleTransform;
    [SerializeField] private Transform shellEjectPoint;

    [Header("References")]
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private Image crosshair;

    [Header("Collision Check")]
    [SerializeField] private LayerMask shootBlockMask;

    [Header("Fire Mode Settings")]
    [SerializeField] private int currentAmmo;

    private CinemachineImpulseSource impulseSource;
    private AudioSource audioSource;
    private float nextFireTime;
    private bool wasAttackPressed;
    private Vector3 currentRecoilPos;
    private bool isReloading;
    private float reloadTimer;
    private CinemachinePanTilt panTilt;
    private float currentPanRecoil;
    private float currentTiltRecoil;
    private float originalFOV;
    private float currentFOV;
    private float targetFOV;
    private PlayerWeaponsManager playerWeaponsManager;
    private Energy energyManager;

    private Vector3 initialWeaponLocalPos;
    private Quaternion initialWeaponLocalRot;

    private float aimProgress;
    private float aimProgressVelocity;

    public float AimSwayScale => Mathf.Lerp(1f, weaponData.aimSwayScale, aimProgress);
    public float AimRotationScale => Mathf.Lerp(1f, weaponData.aimRotationScale, aimProgress);
    public float AimBobScale => Mathf.Lerp(1f, weaponData.aimBobScale, aimProgress);
    public Vector3 CurrentAimPositionOffset => Vector3.Lerp(Vector3.zero, weaponData.aimPositionOffset, aimProgress);
    public Vector3 CurrentAimRotationOffset => Vector3.Lerp(Vector3.zero, weaponData.aimRotationOffset, aimProgress);
    public float CurrentAimRecoilScale => Mathf.Lerp(1f, weaponData.aimRecoilScale, aimProgress);
    public int CurrentAmmo => currentAmmo;
    public int MagazineSize => weaponData.magazineSize;
    public bool IsReloading => isReloading;

    public bool IsAiming => aimProgress > 0.5f;

    private void Start()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();
        audioSource = GetComponent<AudioSource>();
        if (cinemachineCamera == null)
            cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
        if (cinemachineCamera != null && panTilt == null)
            panTilt = cinemachineCamera.GetComponent<CinemachinePanTilt>();
        if (crosshair == null)
        {
            var crosshairObj = GameObject.FindGameObjectWithTag("Crosshair");
            if (crosshairObj != null) crosshair = crosshairObj.GetComponent<Image>();
        }
        playerWeaponsManager = FindFirstObjectByType<PlayerWeaponsManager>();
        energyManager = FindFirstObjectByType<Energy>();

        currentAmmo = weaponData.magazineSize;
        originalFOV = cinemachineCamera != null ? cinemachineCamera.Lens.FieldOfView : 60f;
        currentFOV = originalFOV;
        targetFOV = originalFOV;

        initialWeaponLocalPos = transform.localPosition;
        initialWeaponLocalRot = transform.localRotation;
    }

    private void Update()
    {
        if (Mathf.Approximately(Time.timeScale, 0f)) return;

        bool aimPressed = aimAction != null && aimAction.action.IsPressed();
        float targetAim = aimPressed ? 1f : 0f;
        aimProgress = Mathf.SmoothDamp(aimProgress, targetAim, ref aimProgressVelocity, 0.1f);

        targetFOV = aimPressed ? originalFOV * weaponData.aimFOVMultiplier : originalFOV;
        currentFOV = Mathf.Lerp(currentFOV, targetFOV, weaponData.aimFOVSpeed * Time.deltaTime);
        if (cinemachineCamera != null) cinemachineCamera.Lens.FieldOfView = currentFOV;

        if (crosshair != null) crosshair.enabled = !aimPressed;

        if (reloadAction != null && reloadAction.action.WasPressedThisFrame() && !isReloading && currentAmmo < weaponData.magazineSize)
            StartReload();

        if (isReloading)
        {
            reloadTimer -= Time.deltaTime;
            if (reloadTimer <= 0f) FinishReload();
        }

        if (!isReloading)
        {
            bool attack = attackAction != null && attackAction.action.IsPressed();
            Shooting(attack, aimPressed);
            wasAttackPressed = attack;
        }

        UpdateWeaponRecoil();

        ApplyWeaponTransform();
    }

    private void LateUpdate()
    {
        if (panTilt == null) return;

        panTilt.PanAxis.Value += currentPanRecoil * Time.deltaTime;
        panTilt.TiltAxis.Value += currentTiltRecoil * Time.deltaTime;

        currentPanRecoil = Mathf.Lerp(currentPanRecoil, 0f, weaponData.recoilReturnSpeed * Time.deltaTime);
        currentTiltRecoil = Mathf.Lerp(currentTiltRecoil, 0f, weaponData.recoilReturnSpeed * Time.deltaTime);
    }

    private void Shooting(bool isAttack, bool isAiming)
    {
        if (playerWeaponsManager != null && playerWeaponsManager.PreventShooting)
            return;
        if (Time.time < nextFireTime) return;
        if (currentAmmo <= 0)
        {
            if (!isReloading) StartReload();
            return;
        }
        if (!CanShoot()) return;

        bool fired = false;
        switch (weaponData.fireMode)
        {
            case WeaponData.FireMode.Single:
                if (isAttack && !wasAttackPressed)
                {
                    CreateProjectile(muzzleTransform.rotation);
                    fired = true;
                }
                break;
            case WeaponData.FireMode.Auto:
                if (isAttack)
                {
                    CreateProjectile(muzzleTransform.rotation);
                    fired = true;
                }
                break;
            case WeaponData.FireMode.Shotgun:
                if (isAttack && !wasAttackPressed)
                {
                    for (int i = 0; i < weaponData.shotgunPellets; i++)
                        CreateProjectile(CalculateShotgunSpread());
                    fired = true;
                }
                break;
        }

        if (fired)
        {
            currentAmmo--;
            CameraShake(isAiming);
            if (weaponData.shotSound != null) audioSource.PlayOneShot(weaponData.shotSound);
            if (weaponData.muzzleFlash != null && UnityEngine.Random.value < weaponData.flashEffectChance)
                Destroy(Instantiate(weaponData.muzzleFlash, muzzleTransform.position, muzzleTransform.rotation), 1f);
            if (weaponData.muzzleSmoke != null)
                Destroy(Instantiate(weaponData.muzzleSmoke, muzzleTransform.position, muzzleTransform.rotation), 1f);
            nextFireTime = Time.time + weaponData.fireRate;
            EjectShell();
            ApplyRecoil(isAiming);
        }
    }

    private bool CanShoot()
    {
        if (cinemachineCamera == null || muzzleTransform == null) return true;
        Vector3 camPos = cinemachineCamera.transform.position;
        Vector3 muzzlePos = muzzleTransform.position;
        Vector3 dir = muzzlePos - camPos;
        return !Physics.Raycast(camPos, dir, out _, dir.magnitude, shootBlockMask);
    }

    private void CreateProjectile(Quaternion rotation)
    {
        if (weaponData.projectilePrefab == null || muzzleTransform == null) return;
        var proj = Instantiate(weaponData.projectilePrefab, muzzleTransform.position, rotation);
        var projScript = proj.GetComponent<Projectile>();
        if (projScript != null)
        {
            var playerUpgrades = GetComponentInParent<PlayerUpgrades>();
            projScript.SetPlayerUpgrades(playerUpgrades, playerUpgrades != null ? playerUpgrades.gameObject : gameObject);
        }
    }

    private Quaternion CalculateShotgunSpread()
    {
        float h = UnityEngine.Random.Range(-weaponData.shotgunSpread, weaponData.shotgunSpread);
        float v = UnityEngine.Random.Range(-weaponData.shotgunSpread, weaponData.shotgunSpread);
        return muzzleTransform.rotation * Quaternion.Euler(v, h, 0);
    }

    private void CameraShake(bool isAiming)
    {
        if (impulseSource == null) return;
        float force = weaponData.shakeForceMultiplier * (isAiming ? weaponData.aimShakeMultiplier : 1f);
        impulseSource.GenerateImpulseWithForce(force);
    }

    private void ApplyRecoil(bool isAiming)
    {
        if (panTilt != null)
        {
            float mult = isAiming ? weaponData.aimKickRecoilScale : 1f;
            currentPanRecoil += mult * UnityEngine.Random.Range(-weaponData.weaponKickForceHorizontal, weaponData.weaponKickForceHorizontal);
            currentTiltRecoil -= mult * weaponData.weaponKickForce;
        }

        currentRecoilPos += weaponData.recoilPositionKick * (isAiming ? weaponData.aimRecoilScale : 1f);
    }

    private void UpdateWeaponRecoil()
    {
        currentRecoilPos = Vector3.Lerp(currentRecoilPos, Vector3.zero, weaponData.recoilPositionReturnSpeed * Time.deltaTime);
    }

    private void ApplyWeaponTransform()
    {
        Vector3 pos = initialWeaponLocalPos;
        pos += Vector3.Lerp(Vector3.zero, weaponData.aimPositionOffset, aimProgress);
        pos += currentRecoilPos * CurrentAimRecoilScale;

        Quaternion rot = initialWeaponLocalRot;
        rot *= Quaternion.Euler(Vector3.Lerp(Vector3.zero, weaponData.aimRotationOffset, aimProgress));

        transform.localPosition = pos;
    }

    private void StartReload()
    {
        if (energyManager == null) return;
        int energyMagazine = (weaponData.energyPerShot * weaponData.magazineSize) - (currentAmmo * weaponData.energyPerShot);
        if (!energyManager.TryConsumeEnergy(energyMagazine))
            return;
        energyManager.ConsumeEnergy(energyMagazine);
        isReloading = true;
        reloadTimer = weaponData.reloadTime;
        if (weaponData.reloadSound != null) audioSource.PlayOneShot(weaponData.reloadSound);
        if (playerWeaponsManager != null)
            playerWeaponsManager.HideWeaponForDuration(weaponData.reloadTime);
    }

    private void FinishReload()
    {
        isReloading = false;
        currentAmmo = weaponData.magazineSize;
    }

    private void EjectShell()
    {
        if (weaponData.shellPrefab == null || shellEjectPoint == null) return;
        GameObject shell = Instantiate(weaponData.shellPrefab, shellEjectPoint.position, shellEjectPoint.rotation);
        var rb = shell.GetComponent<Rigidbody>();
        if (rb == null) { Destroy(shell); return; }
        Vector3 rnd = new Vector3(UnityEngine.Random.Range(-0.2f, 0.2f), UnityEngine.Random.Range(0.8f, 1.2f), UnityEngine.Random.Range(-0.2f, 0.2f));
        Vector3 ejectDir = (shellEjectPoint.right + rnd).normalized;
        rb.AddForce(ejectDir * weaponData.shellEjectForce, ForceMode.Impulse);
        rb.AddTorque(UnityEngine.Random.Range(-weaponData.shellEjectTorque, weaponData.shellEjectTorque),
                     UnityEngine.Random.Range(-weaponData.shellEjectTorque, weaponData.shellEjectTorque),
                     UnityEngine.Random.Range(-weaponData.shellEjectTorque, weaponData.shellEjectTorque), ForceMode.Impulse);
        Destroy(shell, weaponData.shellLifetime);
    }
}