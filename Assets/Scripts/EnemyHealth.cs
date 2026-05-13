using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public float xpReward = 35f;
    public float ultReward = 20f;

    [Header("Hit Stun ao tomar dano")]
    public bool applyHitStun = true;
    public float hitStunDuration = 0.3f;

    private int currentHealth;
    private bool isDead = false;
    private bool balanceApplied = false;
    private CharacterVisualFeedback visualFeedback;

    void Awake()
    {
        currentHealth = maxHealth;
        visualFeedback = GetComponent<CharacterVisualFeedback>();
    }

    public void ApplyBalanceMultipliers(float hpMultiplier, float xpMultiplier, float ultMultiplier)
    {
        if (balanceApplied)
            return;

        balanceApplied = true;

        hpMultiplier = Mathf.Max(0.01f, hpMultiplier);
        xpMultiplier = Mathf.Max(0.01f, xpMultiplier);
        ultMultiplier = Mathf.Max(0.01f, ultMultiplier);

        maxHealth = Mathf.Max(1, Mathf.RoundToInt(maxHealth * hpMultiplier));
        currentHealth = maxHealth;

        xpReward *= xpMultiplier;
        ultReward *= ultMultiplier;

        Debug.Log(gameObject.name + " balanceado por fase | HP: " + maxHealth + " | XP: " + xpReward + " | ULT: " + ultReward);
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        if (visualFeedback != null)
        {
            visualFeedback.PlayHitFlash();
        }

        SimpleVFX.SpawnHitSpark(transform.position + Vector3.up * 0.7f);

        Debug.Log(gameObject.name + " tomou " + damage + " de dano. Vida restante: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            ApplyHitStunFeedback();

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayHitEnemy();
            }
        }
    }

    void ApplyHitStunFeedback()
    {
        if (!applyHitStun)
            return;

        if (hitStunDuration <= 0f)
            return;

        EnemyController enemyController = GetComponent<EnemyController>();
        if (enemyController != null)
        {
            enemyController.ApplyHitStun(hitStunDuration);
        }

        BossController bossController = GetComponent<BossController>();
        if (bossController != null)
        {
            bossController.ApplyHitStun(hitStunDuration);
        }
    }

    public void Heal(int amount)
    {
        if (isDead)
            return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        SimpleVFX.SpawnCollect(transform.position + Vector3.up * 0.7f, new Color(0.25f, 1f, 0.35f, 0.75f));

        Debug.Log(gameObject.name + " regenerou " + amount + " de vida. Vida atual: " + currentHealth);
    }

    void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyDeath();
        }

        EnemyController enemyController = GetComponent<EnemyController>();
        if (enemyController != null)
        {
            enemyController.enabled = false;
        }

        BossController bossController = GetComponent<BossController>();
        if (bossController != null)
        {
            bossController.enabled = false;
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }

        EnemyDropper dropper = GetComponent<EnemyDropper>();

        if (dropper != null)
        {
            dropper.TryDrop();
        }

        GameObject playerObject = GameObject.FindWithTag("Player");

        if (playerObject != null)
        {
            PlayerStats playerStats = playerObject.GetComponent<PlayerStats>();

            if (playerStats != null)
            {
                playerStats.AddXP(xpReward);
                playerStats.AddULT(ultReward);
            }
        }

        Debug.Log(gameObject.name + " morreu.");

        if (visualFeedback != null)
        {
            visualFeedback.PlayDeathEffectAndDestroy(gameObject, 0.25f);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}