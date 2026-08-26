using UnityEngine;

public class WeaponPickUp : PickUp
{
    [SerializeField] private WeaponController weaponPrefab;

    protected override bool TryCollect(GameObject collector)
    {
        PlayerWeaponsManager weaponsManager = collector.GetComponent<PlayerWeaponsManager>();
        if (weaponsManager == null) return false;
        weaponsManager.AddWeapon(weaponPrefab);
        return true;
    }
}
