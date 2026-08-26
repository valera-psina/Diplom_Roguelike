using UnityEngine;

public enum UpgradeType
{
    Health,
    Energy,
    Speed,
    Vampirism,
    DotsDamage
}

[CreateAssetMenu(fileName = "UpgradeData", menuName = "Upgrades/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    public Sprite icon;
    public string upgradeName;
    public UpgradeType upgradeType;
    public int level;
    public int maxLevel = 5;

    [TextArea] public string description;

    [Header("Effects")]
    public float speedMultiplier = 1f;
    public float healthMultiplier = 1f;
    public float energyMultiplier = 1f;
    public float vampirismMultiplier = 1f;
    public float dotsDamage = 1f;
}