using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUpgrades : MonoBehaviour
{
    public const int MaxSlots = 3;
    public float DotsMultiplier { get; private set; } = 1f;
    public Action OnUpgradesChanged;

    private Dictionary<UpgradeType, UpgradeData> activeUpgrades = new Dictionary<UpgradeType, UpgradeData>();
    private Health health;
    private Energy energy;
    private PlayerController controller;

    private void Awake()
    {
        health = GetComponent<Health>();
        energy = GetComponent<Energy>();
        controller = GetComponent<PlayerController>();
    }

    public bool TryApplyUpgrade(UpgradeData upgrade)
    {
        if (upgrade == null) return false;

        bool changed = false;
        if (activeUpgrades.ContainsKey(upgrade.upgradeType))
        {
            if (upgrade.level > activeUpgrades[upgrade.upgradeType].level)
            {
                activeUpgrades[upgrade.upgradeType] = upgrade;
                changed = true;
            }
        }
        else if (activeUpgrades.Count < MaxSlots)
        {
            activeUpgrades.Add(upgrade.upgradeType, upgrade);
            changed = true;
        }

        if (changed)
        {
            RecalculateStats();
            OnUpgradesChanged?.Invoke();
        }
        return changed;
    }

    public void RemoveUpgrade(UpgradeType type)
    {
        if (activeUpgrades.Remove(type))
        {
            RecalculateStats();
            OnUpgradesChanged?.Invoke();
        }
    }

    public List<UpgradeData> GetActiveUpgradesList()
    {
        return new List<UpgradeData>(activeUpgrades.Values);
    }

    public bool HasFreeSlot => activeUpgrades.Count < MaxSlots;

    public int GetLevel(UpgradeType type)
    {
        return activeUpgrades.TryGetValue(type, out var up) ? up.level : 0;
    }

    public void RecalculateStats()
    {
        float healthMult = 1f;
        float energyMult = 1f;
        float speedMult = 1f;
        float vampMult = 1f;
        float dotsMult = 1f;

        foreach (var up in activeUpgrades.Values)
        {
            healthMult *= up.healthMultiplier;
            energyMult *= up.energyMultiplier;
            speedMult *= up.speedMultiplier;
            vampMult *= up.vampirismMultiplier;
            dotsMult *= up.dotsDamage;
        }

        DotsMultiplier = dotsMult;

        if (health != null) health.SetMaxHealthMultiplier(healthMult);
        if (energy != null) energy.SetMaxEnergyMultiplier(energyMult);
        if (controller != null) controller.SetMoveSpeedMultiplier(speedMult);
    }
}