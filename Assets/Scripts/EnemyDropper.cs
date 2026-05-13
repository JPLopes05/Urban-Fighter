using UnityEngine;

public class EnemyDropper : MonoBehaviour
{
    [Header("Chance de drop")]
    [Range(0f, 1f)]
    public float dropChance = 0.5f;

    [Header("Possíveis drops")]
    public GameObject[] possibleDrops;

    [Header("Spawn")]
    public float spawnYOffset = 0.3f;

    public void TryDrop()
    {
        if (possibleDrops == null || possibleDrops.Length == 0)
            return;

        float roll = Random.Range(0f, 1f);

        if (roll > dropChance)
        {
            Debug.Log("Nenhum item dropou.");
            return;
        }

        int chosenIndex = Random.Range(0, possibleDrops.Length);
        GameObject chosenDrop = possibleDrops[chosenIndex];

        Vector3 spawnPosition = transform.position + new Vector3(0f, spawnYOffset, 0f);

        Instantiate(chosenDrop, spawnPosition, Quaternion.identity);
        Debug.Log("Drop gerado: " + chosenDrop.name);
    }
}