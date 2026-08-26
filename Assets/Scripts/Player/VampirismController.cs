using UnityEngine;

public class VampirismController : MonoBehaviour
{
    [SerializeField] private float baseHealPerKill = 1f;
    private Health playerHealth;
    private PlayerUpgrades playerUpgrades;

    private void Start()
    {
        playerHealth = GetComponent<Health>();
        playerUpgrades = GetComponent<PlayerUpgrades>();
        GameManager.OnEnemyDied += OnEnemyDied;
    }

    private void OnDestroy()
    {
        GameManager.OnEnemyDied -= OnEnemyDied;
    }

    private void OnEnemyDied(GameObject enemy, GameObject killer)
    {
        if (enemy == null || killer == null) return;
        // Вампиризм срабатывает, только если убил этот игрок
        if (killer == null || !killer.CompareTag("Player")) return;

        // Получаем текущий множитель вампиризма
        float vampMult = 1f;
        foreach (var up in playerUpgrades.GetActiveUpgradesList())
        {
            vampMult *= up.vampirismMultiplier;
        }

        if (vampMult > 1f) // или если есть хоть какое-то улучшение вампиризма
        {
            float healAmount = baseHealPerKill * vampMult;
            playerHealth.Heal(healAmount);
        }
    }
}