using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public enum PickupType
    {
        Heal,
        AttackBuff,
        DefenseBuff
    }

    [Header("Configuração")]
    public PickupType pickupType;
    public float valuePercent = 20f;
    public float duration = 15f;
    public float lifetime = 10f;

    private bool collected = false;
    private PickupVisualFeedback visualFeedback;

    void Awake()
    {
        visualFeedback = GetComponent<PickupVisualFeedback>();
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected)
            return;

        if (!other.CompareTag("Player"))
            return;

        PlayerStats playerStats = other.GetComponent<PlayerStats>();

        if (playerStats == null)
            return;

        collected = true;

        switch (pickupType)
        {
            case PickupType.Heal:
                playerStats.HealPercent(valuePercent);

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayPickupHeal();
                }

                SimpleVFX.SpawnCollect(transform.position, new Color(0.2f, 1f, 0.35f, 0.8f));
                break;

            case PickupType.AttackBuff:
                playerStats.ApplyAttackBuff(valuePercent, duration);

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayPickupAttack();
                }

                SimpleVFX.SpawnCollect(transform.position, new Color(1f, 0.35f, 0.05f, 0.8f));
                break;

            case PickupType.DefenseBuff:
                playerStats.ApplyDefenseBuff(valuePercent, duration);

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayPickupDefense();
                }

                SimpleVFX.SpawnCollect(transform.position, new Color(0.2f, 0.65f, 1f, 0.8f));
                break;
        }

        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }

        Debug.Log("Pickup coletado: " + pickupType + " | Valor: " + valuePercent);

        if (visualFeedback != null)
        {
            visualFeedback.PlayCollectAndDestroy();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}