using System.Linq;
using UnityEngine;

[System.Serializable]
public class LootItem
{
    public GameObject itemPrefab;
    [Range(0f, 1f)] public float chance;
}

public class LootBox : MonoBehaviour
{
    [SerializeField] private LootItem[] lootTable;
    [SerializeField] private Vector3 spawnOffset = new Vector3 (0f, 0.5f, 0f);
    [SerializeField] private float destroyDelay = 0.1f;

    private Health health;
    private bool isOpened = false;

    private void Start()
    {
        health = GetComponent<Health>();
        if (health == null) return;
        health.OnDie += OnDieHandler;
    }

    private void OnDestroy()
    {
        if (health != null)
            health.OnDie -= OnDieHandler;
    }

    private void OnDieHandler(GameObject killer)
    {
        if (isOpened) return;
        isOpened = true;
        Open();
    }

    private void Open()
    {
        GameObject selectedLoot = SelectLootItem();
        if (selectedLoot != null)
        {
            Vector3 spawnPosition = transform.position + spawnOffset;
            Instantiate(selectedLoot, spawnPosition, Quaternion.identity);
        }
        Destroy(gameObject, destroyDelay);
    }

    private GameObject SelectLootItem()
    {
        if (lootTable == null || lootTable.Length == 0)
            return null;
        float totalChance = lootTable.Sum(item => item.chance);
        if (Mathf.Abs(totalChance - 1f) > 0.001f)
            Debug.Log("Ошибка шанса лута!");
        float randomValue = Random.Range(0f, totalChance);
        float sum = 0f;
        foreach (LootItem item in lootTable)
        {
            sum += item.chance;
            if (randomValue <= sum)
                return item.itemPrefab;
        }
        return lootTable[lootTable.Length - 1].itemPrefab;
    }
}