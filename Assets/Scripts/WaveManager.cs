using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class EnemySpawnData
    {
        public GameObject enemyPrefab;
        public int count = 1;
    }

    [System.Serializable]
    public class WaveData
    {
        public float delayBeforeWave = 1.5f;
        public EnemySpawnData[] enemiesToSpawn;
    }

    [Header("Referências")]
    public Transform[] spawnPoints;
    public GameObject frontBarrier;

    [Header("Ondas")]
    public WaveData[] waves;

    private List<GameObject> aliveEnemies = new List<GameObject>();
    private int currentWaveIndex = -1;

    private bool encounterStarted = false;
    private bool encounterFinished = false;
    private bool waitingNextWave = false;

    void Start()
    {
        if (frontBarrier != null)
        {
            frontBarrier.SetActive(false);
        }
    }

    void Update()
    {
        if (!encounterStarted || encounterFinished)
            return;

        aliveEnemies.RemoveAll(enemy => enemy == null);

        if (aliveEnemies.Count == 0 && !waitingNextWave)
        {
            StartCoroutine(StartNextWave());
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (encounterStarted || encounterFinished)
            return;

        if (other.CompareTag("Player"))
        {
            encounterStarted = true;

            if (frontBarrier != null)
            {
                frontBarrier.SetActive(true);
            }

            StartCoroutine(StartNextWave());
        }
    }

    IEnumerator StartNextWave()
    {
        waitingNextWave = true;
        currentWaveIndex++;

        if (currentWaveIndex >= waves.Length)
        {
            encounterFinished = true;

            if (frontBarrier != null)
            {
                frontBarrier.SetActive(false);
            }

            gameObject.SetActive(false);
            yield break;
        }

        float delay = waves[currentWaveIndex].delayBeforeWave;

        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        SpawnWave(waves[currentWaveIndex]);
        waitingNextWave = false;
    }

    void SpawnWave(WaveData wave)
    {
        aliveEnemies.Clear();

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("WaveManager sem spawnPoints configurados.");
            return;
        }

        int spawnIndex = 0;

        foreach (EnemySpawnData spawnData in wave.enemiesToSpawn)
        {
            if (spawnData.enemyPrefab == null)
                continue;

            for (int i = 0; i < spawnData.count; i++)
            {
                Transform chosenSpawn = spawnPoints[spawnIndex % spawnPoints.Length];
                GameObject enemy = Instantiate(spawnData.enemyPrefab, chosenSpawn.position, Quaternion.identity);
                aliveEnemies.Add(enemy);

                spawnIndex++;
            }
        }

        Debug.Log("Onda " + (currentWaveIndex + 1) + " iniciada com " + aliveEnemies.Count + " inimigos.");
    }
}