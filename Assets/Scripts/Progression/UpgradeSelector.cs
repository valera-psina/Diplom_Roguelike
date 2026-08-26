using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeSelector : MonoBehaviour
{
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private Button[] choiceButtons;
    [SerializeField] private List<UpgradeData> allAvailableUpgrades;

    private PlayerUpgrades playerUpgrades;
    private bool selecting = false;
    private int pendingLevelUps = 0;

    private void Awake()
    {
        playerUpgrades = GetComponent<PlayerUpgrades>();
        upgradePanel.SetActive(false);
        GetComponent<Level>().OnLevelUp += OnLevelUp;
    }

    private void OnLevelUp(int newLevel)
    {
        pendingLevelUps++;
        if (!selecting) StartNextSelection();
    }

    private void StartNextSelection()
    {
        if (pendingLevelUps <= 0) return;
        if (!HasAvailableUpgrades())
        {
            pendingLevelUps = 0;
            selecting = false;
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            return;
        }
        selecting = true;
        pendingLevelUps--;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        ShowRandomUpgrades();
    }

    private void ShowRandomUpgrades()
    {
        List<UpgradeData> possible = new List<UpgradeData>();

        foreach (var active in playerUpgrades.GetActiveUpgradesList())
        {
            if (active.level < active.maxLevel)
            {
                var next = allAvailableUpgrades.Find(u => u.upgradeType == active.upgradeType && u.level == active.level + 1);
                if (next != null)
                    possible.Add(next);
            }
        }

        if (playerUpgrades.HasFreeSlot)
        {
            foreach (var upgrade in allAvailableUpgrades)
            {
                if (upgrade.level == 1 && !playerUpgrades.GetActiveUpgradesList().Exists(u => u.upgradeType == upgrade.upgradeType))
                {
                    possible.Add(upgrade);
                }
            }
        }

        int count = Mathf.Min(choiceButtons.Length, possible.Count);
        List<UpgradeData> chosen = new List<UpgradeData>();
        for (int i = 0; i < count; i++)
        {
            int idx = Random.Range(0, possible.Count);
            chosen.Add(possible[idx]);
            possible.RemoveAt(idx);
        }

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i < chosen.Count)
            {
                choiceButtons[i].gameObject.SetActive(true);
                UpgradeData upgrade = chosen[i];
                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].onClick.AddListener(() => SelectUpgrade(upgrade));
                var text = choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                    text.text = $"{upgrade.upgradeName} (ур.{upgrade.level})\n{upgrade.description}";
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }

        upgradePanel.SetActive(true);
    }

    public void SelectUpgrade(UpgradeData upgrade)
    {
        if (playerUpgrades.TryApplyUpgrade(upgrade))
        {
            upgradePanel.SetActive(false);
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            selecting = false;
            if (pendingLevelUps > 0) StartNextSelection();
        }
    }

    private bool HasAvailableUpgrades()
    {
        foreach (var active in playerUpgrades.GetActiveUpgradesList())
        {
            if (active.level < active.maxLevel)
            {
                var next = allAvailableUpgrades.Find(u => u.upgradeType == active.upgradeType && u.level == active.level + 1);
                if (next != null) return true;
            }
        }

        if (playerUpgrades.HasFreeSlot)
        {
            foreach (var upgrade in allAvailableUpgrades)
            {
                if (upgrade.level == 1 && !playerUpgrades.GetActiveUpgradesList().Exists(u => u.upgradeType == upgrade.upgradeType))
                {
                    return true;
                }
            }
        }

        return false;
    }
}