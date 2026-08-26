using System.Collections;
using UnityEngine;

public class BossArenaManager : MonoBehaviour
{
    [Header("Enemies")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private int enemiesPerWave = 3;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Effect")]
    [SerializeField] private GameObject spawnEffectPrefab;
    [SerializeField] private float effectDuration = 1.5f;

    [Header("Timing")]
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private float initialDelay = 0f;

    private Coroutine spawnRoutine;
    private GameObject bossObject;
    private Health bossHealth;
    private bool bossDefeated;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !bossDefeated)
        {
            StartSpawning();
            GetComponent<Collider>().enabled = false;
        }
    }

    private void Start()
    {
        TryFindBoss();
    }

    /// <summary> Ищет босса по тегу. Если найден — подписывается на его смерть. </summary>
    private void TryFindBoss()
    {
        if (bossObject != null) return; // уже есть

        GameObject found = GameObject.FindGameObjectWithTag("Boss");
        if (found != null)
        {
            bossObject = found;
            bossHealth = bossObject.GetComponent<Health>();
            if (bossHealth != null)
            {
                bossHealth.OnDie += OnBossDefeated;
                // Если босс мёртв уже сейчас (на всякий случай)
                if (bossHealth.IsDead)
                    OnBossDefeated(null);
            }
        }
    }

    private void OnBossDefeated(GameObject killer)
    {
        if (bossDefeated) return;
        bossDefeated = true;
        StopSpawning();
        if (bossHealth != null)
            bossHealth.OnDie -= OnBossDefeated;
    }

    public void StartSpawning()
    {
        if (bossDefeated) return;

        // Попробуем найти босса, если ещё не найден
        TryFindBoss();

        // Даже если босс не найден — запускаем спавн (он появится позже)
        if (spawnRoutine != null) StopCoroutine(spawnRoutine);
        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    private IEnumerator SpawnLoop()
    {
        if (initialDelay > 0f)
            yield return new WaitForSeconds(initialDelay);

        while (!bossDefeated)
        {
            // Пытаемся найти босса, если его ещё нет (появился позже)
            if (bossObject == null) TryFindBoss();

            // Если босс был найден, но потом уничтожен — завершаем спавн
            if (bossObject == null && bossHealth != null) // был найден и потерян
            {
                OnBossDefeated(null);
                yield break;
            }

            // Если босс найден и он уже мёртв (умер до того, как мы подписались)
            if (bossHealth != null && bossHealth.IsDead)
            {
                OnBossDefeated(null);
                yield break;
            }

            yield return new WaitForSeconds(spawnInterval);
            if (bossDefeated) yield break;
            yield return StartCoroutine(SpawnWave());
        }
    }

    private IEnumerator SpawnWave()
    {
        int count = Mathf.Min(enemiesPerWave, spawnPoints.Length);
        if (count == 0) yield break;

        int[] indices = new int[spawnPoints.Length];
        for (int i = 0; i < indices.Length; i++) indices[i] = i;
        for (int i = 0; i < count; i++)
        {
            int rand = Random.Range(i, indices.Length);
            (indices[i], indices[rand]) = (indices[rand], indices[i]);
        }

        for (int i = 0; i < count; i++)
        {
            if (spawnEffectPrefab != null)
            {
                GameObject fx = Instantiate(spawnEffectPrefab, spawnPoints[indices[i]].position, Quaternion.identity);
                Destroy(fx, effectDuration);
            }
        }

        yield return new WaitForSeconds(effectDuration);

        for (int i = 0; i < count; i++)
        {
            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            Instantiate(prefab, spawnPoints[indices[i]].position, Quaternion.identity);
        }
    }

    private void OnDestroy()
    {
        if (bossHealth != null)
            bossHealth.OnDie -= OnBossDefeated;
    }
}