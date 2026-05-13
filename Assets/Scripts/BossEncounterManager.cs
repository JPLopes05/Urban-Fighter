using System.Collections;
using UnityEngine;

public class BossEncounterManager : MonoBehaviour
{
    [Header("Referências")]
    public GameObject bossPrefab;
    public Transform bossSpawnPoint;
    public GameObject bossBarrier;

    [Header("Configuração")]
    public float spawnDelay = 1f;

    private bool encounterStarted = false;
    private bool bossSpawned = false;
    private bool encounterFinished = false;

    private GameObject currentBoss;

    void Start()
    {
        if (bossBarrier != null)
        {
            bossBarrier.SetActive(false);
        }
    }

    void Update()
    {
        if (!encounterStarted || encounterFinished || !bossSpawned)
            return;

        if (currentBoss == null)
        {
            encounterFinished = true;

            if (bossBarrier != null)
            {
                bossBarrier.SetActive(false);
            }

            Debug.Log("Boss da fase 1 derrotado. Área liberada.");
            gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (encounterStarted || encounterFinished)
            return;

        if (other.CompareTag("Player"))
        {
            encounterStarted = true;

            if (bossBarrier != null)
            {
                bossBarrier.SetActive(true);
            }

            StartCoroutine(SpawnBossRoutine());
        }
    }

    IEnumerator SpawnBossRoutine()
    {
        yield return new WaitForSeconds(spawnDelay);

        if (bossPrefab != null && bossSpawnPoint != null)
        {
            currentBoss = Instantiate(bossPrefab, bossSpawnPoint.position, Quaternion.identity);
            bossSpawned = true;

            Debug.Log("Boss da fase 1 surgiu.");
        }
        else
        {
            Debug.LogWarning("BossEncounterManager sem bossPrefab ou bossSpawnPoint configurados.");
        }
    }
}