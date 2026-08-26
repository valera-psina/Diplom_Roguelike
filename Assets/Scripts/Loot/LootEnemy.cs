using UnityEngine;

public class LootEnemy : MonoBehaviour
{
    [SerializeField] private GameObject lootPrefab;
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 0.5f, 0f);
    [SerializeField] [Range(0f, 1f)] private float dropChance = 0.5f;
    [SerializeField] private float destroyDelay = 0.1f;

    private Health health;

    private void Start()
    {
        health = GetComponent<Health>();
        if (health != null)
            health.OnDie += OnDieHandler;
    }

    private void OnDestroy()
    {
        if (health != null)
            health.OnDie -= OnDieHandler;
    }

    private void OnDieHandler(GameObject killer)
    {
        if (Random.value <= dropChance)
        {
            Vector3 randomOffset = new Vector3(Random.Range(-0.8f, 0.8f), 0f, Random.Range(-0.8f, 0.8f));
            Instantiate(lootPrefab, transform.position + spawnOffset + randomOffset, Quaternion.identity);
        }
        Destroy(gameObject, destroyDelay);
    }
}
