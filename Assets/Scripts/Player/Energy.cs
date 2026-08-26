using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Energy : MonoBehaviour
{
    public int maxEnergy = 200;
    public int currentEnergy;
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private Image energyBar;

    public event Action<int, int> OnEnergyChanged;

    private int baseMaxEnergy;
    private bool baseInitialized;
    private float energyMultiplier = 1f;

    private void Awake()
    {
        if (!baseInitialized)
        {
            baseMaxEnergy = maxEnergy;
            baseInitialized = true;
        }

        currentEnergy = maxEnergy;
        OnEnergyChanged += UpdateEnergyInfo;
        UpdateEnergyInfo(currentEnergy, maxEnergy);
    }

    private void UpdateEnergyInfo(int current, int max)
    {
        if (energyText != null)
            energyText.text = $"{current}/{max}";
        if (energyBar != null)
            energyBar.fillAmount = (float)current / max;
    }

    public bool TryConsumeEnergy(int amount) => amount <= currentEnergy;

    public void ConsumeEnergy(int amount)
    {
        if (TryConsumeEnergy(amount))
        {
            currentEnergy -= amount;
            OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
        }
    }

    public bool AddEnergy(int amount)
    {
        if (amount < 0) return false;
        if (currentEnergy >= maxEnergy) return false;

        int boostedAmount = Mathf.RoundToInt(amount * energyMultiplier);
        if (boostedAmount <= 0) return false;

        int before = currentEnergy;
        currentEnergy += boostedAmount;
        if (currentEnergy > maxEnergy) currentEnergy = maxEnergy;

        if (currentEnergy != before)
        {
            OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
            return true;
        }
        return false;
    }

    public void SetEnergy(int current, int max)
    {
        maxEnergy = max;
        currentEnergy = Mathf.Clamp(current, 0, maxEnergy);
        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
    }

    public void SetMaxEnergyMultiplier(float mult)
    {
        energyMultiplier = mult;
        maxEnergy = Mathf.RoundToInt(baseMaxEnergy * mult);
        if (currentEnergy > maxEnergy) currentEnergy = maxEnergy;
        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
    }

    public void SetCurrentEnergy(int current)
    {
        currentEnergy = Mathf.Clamp(current, 0, maxEnergy);
        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
    }
}