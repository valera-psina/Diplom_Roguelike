using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Level : MonoBehaviour
{
    [Header("Level")]
    [SerializeField] private int startingLevel = 1;
    [SerializeField] private float baseExperienceToLevel = 20f;
    [SerializeField] private float experienceGrowthFactor = 1.5f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Image experienceBar;

    public int CurrentLevel { get; private set; }
    public float CurrentExperience { get; private set; }
    public float ExperienceToNextLevel { get; private set; }

    public event Action<int> OnLevelUp;
    public event Action<float, float> OnExperienceChanged;

    private bool initialized = false;

    private void Start()
    {
        if (initialized) return;

        CurrentLevel = startingLevel;
        CurrentExperience = 0f;
        ExperienceToNextLevel = baseExperienceToLevel;
        initialized = true;
        UpdateUI();
    }

    public bool AddExperience(float amount)
    {
        if (amount <= 0f) return false;

        bool leveledUp = false;
        CurrentExperience += amount;

        while (CurrentExperience >= ExperienceToNextLevel)
        {
            CurrentExperience -= ExperienceToNextLevel;
            CurrentLevel++;
            ExperienceToNextLevel = CalculateRequiredExperience(CurrentLevel);
            leveledUp = true;
            OnLevelUp?.Invoke(CurrentLevel);
        }

        OnExperienceChanged?.Invoke(CurrentExperience, ExperienceToNextLevel);
        UpdateUI();
        return true;
    }

    private float CalculateRequiredExperience(int level)
    {
        return baseExperienceToLevel * Mathf.Pow(experienceGrowthFactor, level - 1);
    }

    public void SetLevel(int level, float currentExp)
    {
        initialized = true;
        CurrentLevel = Mathf.Max(1, level);
        ExperienceToNextLevel = CalculateRequiredExperience(CurrentLevel);
        CurrentExperience = Mathf.Clamp(currentExp, 0f, ExperienceToNextLevel);
        OnExperienceChanged?.Invoke(CurrentExperience, ExperienceToNextLevel);
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (levelText != null)
            levelText.text = $"Уровень {CurrentLevel}";
        if (experienceBar != null)
            experienceBar.fillAmount = CurrentExperience / ExperienceToNextLevel;
    }
}