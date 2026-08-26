using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Wave
{
    public GameObject[] enemyPrefabs;
    public int minCount = 3;
    public int maxCount = 5;
    public float delayBeforeWave = 1.5f;
}

public class RoomManager : MonoBehaviour
{
    public enum RoomState
    {
        Inactive,
        Active,
        Cleared
    }

    [Header("Room")]
    public RoomState currentState = RoomState.Inactive;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private GameObject spawnEffectPrefab;

    [Space(10)]
    [SerializeField] private Wave[] waves;

    [Header("Door")]
    [SerializeField] private GameObject[] doors;
    [SerializeField] private GameObject[] doorCollisions;
    [SerializeField] private float downDist = 7.0f;

    [Header("Loot")]
    [SerializeField] private GameObject lootChestPrefab;

    private Vector3[] closedPos;
    private int currentWaveIndex = 0;
    private List<GameObject> currentEnemies = new List<GameObject>();

    private void Start()
    {
        closedPos = new Vector3[doors.Length];
        for (int i = 0; i < doors.Length; i++)
            closedPos[i] = doors[i].transform.position;
        OpenDoor();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || currentState != RoomState.Inactive)
            return;

        currentState = RoomState.Active;
        CloseDoor();
        currentWaveIndex = 0;
        StartNextWave();
    }

    private void StartNextWave()
    {
        if (currentWaveIndex >= waves.Length)
        {
            CompleteRoom();
            return;
        }

        StartCoroutine(SpawnWaveAfterDelay(waves[currentWaveIndex].delayBeforeWave));
    }

    private IEnumerator SpawnWaveAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        Wave wave = waves[currentWaveIndex];
        int enemyCount = Random.Range(wave.minCount, wave.maxCount + 1);

        List<int> chosenIndices = new List<int>();
        List<GameObject> effects = new List<GameObject>();
        for (int i = 0; i < enemyCount; i++)
        {
            int idx = Random.Range(0, spawnPoints.Length);
            chosenIndices.Add(idx);
            effects.Add(Instantiate(spawnEffectPrefab, spawnPoints[idx].position, Quaternion.identity));
        }

        yield return new WaitForSeconds(1.5f);

        foreach (var e in effects) Destroy(e);

        foreach (int idx in chosenIndices)
        {
            int enemyIdx = Random.Range(0, wave.enemyPrefabs.Length);
            GameObject newEnemy = Instantiate(wave.enemyPrefabs[enemyIdx], spawnPoints[idx].position, Quaternion.identity);

            RangedCellsEnemy ranged = newEnemy.GetComponent<RangedCellsEnemy>();
            if (ranged != null)
                ranged.SetTeleportPoints(spawnPoints);

            Health health = newEnemy.GetComponent<Health>();
            if (health != null)
                health.OnDie += (killer) => OnEnemyDied(newEnemy, killer);
            currentEnemies.Add(newEnemy);
        }
        currentWaveIndex++;
    }

    private void OnEnemyDied(GameObject enemy, GameObject killer)
    {
        if (currentEnemies.Contains(enemy))
            currentEnemies.Remove(enemy);

        if (currentEnemies.Count == 0 && currentState == RoomState.Active)
        {
            StartNextWave();
        }
    }

    private void CompleteRoom()
    {
        currentState = RoomState.Cleared;
        OpenDoor();

        Debug.Log("Комната полностью очищена.");
        if (lootChestPrefab != null)
        {
            int randomPos = Random.Range(0, spawnPoints.Length);
            Vector3 spawnOffset = new Vector3(0f, 0.5f, 0f);
            Instantiate(lootChestPrefab, spawnPoints[randomPos].position + spawnOffset, Quaternion.identity);
        }
    }

    private void CloseDoor()
    {
        for (int i = 0; i < doors.Length; i++)
            StartCoroutine(MoveDoor(doors[i], closedPos[i], 1f));
        for (int i = 0; i < doorCollisions.Length; i++)
            doorCollisions[i].SetActive(true);
    }

    private void OpenDoor()
    {
        for (int i = 0; i < doors.Length; i++)
            StartCoroutine(MoveDoor(doors[i], closedPos[i] + Vector3.down * downDist, 1f));
        for (int i = 0; i < doorCollisions.Length; i++)
            doorCollisions[i].SetActive(false);
    }

    private IEnumerator MoveDoor(GameObject door, Vector3 target, float time)
    {
        Vector3 start = door.transform.position;
        float deltaTime = 0f;
        while (deltaTime < time)
        {
            door.transform.position = Vector3.Lerp(start, target, deltaTime / time);
            deltaTime += Time.deltaTime;
            yield return null;
        }
        door.transform.position = target;
    }

    public Transform[] GetSpawnPoints()
    {
        return spawnPoints;
    }
}