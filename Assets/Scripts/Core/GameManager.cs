using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameObject CurrentPlayer { get; private set; }

    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;

    private float savedHealth;
    private float savedMaxHealth;
    private int savedEnergy;
    private int savedMaxEnergy;
    private List<WeaponController> savedWeaponPrefabs = new List<WeaponController>();
    private int savedActiveWeaponIndex;
    private int currentLevelIndex = 1;
    private int savedLevel;
    private float savedExperience;
    private float savedExperienceToNext;
    private List<UpgradeData> savedUpgrades = new List<UpgradeData>();

    public static event System.Action<GameObject, GameObject> OnEnemyDied;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartNewGame()
    {
        savedHealth = 100f;
        savedMaxHealth = 100f;
        savedEnergy = 200;
        savedMaxEnergy = 200;
        savedWeaponPrefabs.Clear();
        savedLevel = 1;
        savedExperience = 0f;
        savedUpgrades.Clear();
        Time.timeScale = 1.0f;

        currentLevelIndex = 1;
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(currentLevelIndex);
    }

    public void LoadNextLevel()
    {
        SavePlayerState();
        currentLevelIndex++;
        if (currentLevelIndex > 3)
        {
            SceneManager.LoadScene(0);
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
        else
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene(currentLevelIndex);
        }
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    private void SavePlayerState()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("SavePlayerState: игрок не найден!");
            return;
        }

        Health health = player.GetComponent<Health>();
        Energy energy = player.GetComponent<Energy>();

        if (health != null)
        {
            savedHealth = health.CurrentHealth;
            savedMaxHealth = health.MaxHealth;
        }
        else
            Debug.LogError("SavePlayerState: на игроке нет Health!");

        if (energy != null)
        {
            savedEnergy = energy.currentEnergy;
            savedMaxEnergy = energy.maxEnergy;
        }

        PlayerWeaponsManager weapons = player.GetComponent<PlayerWeaponsManager>();
        if (weapons != null)
        {
            savedWeaponPrefabs = weapons.GetAllWeaponPrefabs();
            savedActiveWeaponIndex = weapons.ActiveWeaponIndex;
        }

        Level level = player.GetComponent<Level>();
        if (level != null)
        {
            savedLevel = level.CurrentLevel;
            savedExperience = level.CurrentExperience;
        }

        PlayerUpgrades upgrades = player.GetComponent<PlayerUpgrades>();
        if (upgrades != null)
            savedUpgrades = upgrades.GetActiveUpgradesList();

        CurrentPlayer = null;
        Destroy(player);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        StartCoroutine(RestorePlayerDelayed());
    }

    private IEnumerator RestorePlayerDelayed()
    {
        yield return null;

        GameObject startPoint = GameObject.FindGameObjectWithTag("StartPoint");
        Vector3 spawnPos = startPoint != null ? startPoint.transform.position : Vector3.zero;

        GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        CurrentPlayer = player;

        PlayerWeaponsManager weapons = player.GetComponent<PlayerWeaponsManager>();
        if (weapons != null)
        {
            foreach (var wp in savedWeaponPrefabs)
                weapons.AddWeapon(wp);
            weapons.SwitchToWeaponByIndex(savedActiveWeaponIndex);
        }

        Level level = player.GetComponent<Level>();
        if (level != null)
            level.SetLevel(savedLevel, savedExperience);

        PlayerUpgrades upgrades = player.GetComponent<PlayerUpgrades>();
        if (upgrades != null && savedUpgrades != null)
        {
            foreach (var up in savedUpgrades)
                upgrades.TryApplyUpgrade(up);
            upgrades.RecalculateStats();
        }

        Health health = player.GetComponent<Health>();
        if (health != null)
            health.SetCurrentHealth(savedHealth);

        Energy energy = player.GetComponent<Energy>();
        if (energy != null)
            energy.SetCurrentEnergy(savedEnergy);
    }

    public static void RaiseEnemyDied(GameObject enemy, GameObject killer)
    {
        OnEnemyDied?.Invoke(enemy, killer);
    }
}