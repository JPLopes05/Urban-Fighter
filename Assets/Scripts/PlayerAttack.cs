using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Referências")]
    public Transform attackPoint;
    public LayerMask enemyLayer;

    private PlayerStats playerStats;
    private PlayerController playerController;
    private CharacterVisualFeedback visualFeedback;

    [Header("Ataque 1")]
    public float attack1Range = 0.8f;
    public float attack1Cooldown = 0.35f;

    [Header("Ataque 2")]
    public float attack2Range = 1.1f;
    public float attack2Cooldown = 0.6f;

    [Header("Visual do ataque leve")]
    public float lightVFXHandHeight = 0.85f;
    public float lightVFXForwardOffset = 0.72f;

    [Header("Visual do ataque pesado")]
    public float heavyVFXHandHeight = 0.90f;
    public float heavyVFXForwardOffset = 0.95f;

    [Header("Ganho de ULT")]
    public float ultGainPerHit = 5f;

    [Header("Ultimate")]
    public float ultimateRange = 2f;
    public float ultimateRecoveryTime = 0.8f;

    private float nextAttackTime = 0f;

    void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        playerController = GetComponent<PlayerController>();
        visualFeedback = GetComponent<CharacterVisualFeedback>();
    }

    void Update()
    {
        if (Time.time < nextAttackTime)
            return;

        if (Input.GetKeyDown(KeyCode.Space) && playerStats.IsUltimateReady())
        {
            UseUltimate();
            nextAttackTime = Time.time + ultimateRecoveryTime;
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            int damage = playerStats.GetAttack1Damage();
            PerformAttack(attack1Range, damage, true, false);
            nextAttackTime = Time.time + attack1Cooldown;
        }
        else if (Input.GetMouseButtonDown(1))
        {
            int damage = playerStats.GetAttack2Damage();
            PerformAttack(attack2Range, damage, true, true);
            nextAttackTime = Time.time + attack2Cooldown;
        }
    }

    void PerformAttack(float range, int damage, bool grantsULT, bool heavyAttack)
    {
        Vector2 facingDirection = GetFacingDirection();

        if (AudioManager.Instance != null)
        {
            if (heavyAttack)
            {
                AudioManager.Instance.PlayAttackHeavy();
            }
            else
            {
                AudioManager.Instance.PlayAttackLight();
            }
        }

        if (visualFeedback != null)
        {
            if (heavyAttack)
            {
                visualFeedback.PlayHeavyAttack(facingDirection);
            }
            else
            {
                visualFeedback.PlayLightAttack(facingDirection);
            }
        }

        Vector3 handVFXPosition = GetHandVFXPosition(facingDirection, heavyAttack);
        SimpleVFX.SpawnPunchImpact(handVFXPosition, facingDirection, heavyAttack);

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, range, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);

                SimpleVFX.SpawnHitSpark(enemy.bounds.center);

                if (grantsULT)
                {
                    playerStats.AddULT(ultGainPerHit);
                }
            }
        }
    }

    Vector2 GetFacingDirection()
    {
        if (playerController != null)
        {
            return playerController.IsFacingRight() ? Vector2.right : Vector2.left;
        }

        return transform.localScale.x >= 0f ? Vector2.right : Vector2.left;
    }

    Vector3 GetHandVFXPosition(Vector2 facingDirection, bool heavyAttack)
    {
        float height = heavyAttack ? heavyVFXHandHeight : lightVFXHandHeight;
        float forwardOffset = heavyAttack ? heavyVFXForwardOffset : lightVFXForwardOffset;

        return transform.position
            + Vector3.up * height
            + new Vector3(facingDirection.x * forwardOffset, 0f, 0f);
    }

    void UseUltimate()
    {
        int ultimateDamage = playerStats.GetUltimateDamage();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUltimate();
        }

        if (visualFeedback != null)
        {
            visualFeedback.PlaySkillCastPulse(new Color(1f, 0.35f, 0.05f, 1f));
        }

        playerStats.ConsumeULT();

        SimpleVFX.SpawnUltimateBurst(
            transform.position + Vector3.up * 0.65f,
            ultimateRange
        );

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, ultimateRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(ultimateDamage);
                SimpleVFX.SpawnHitSpark(enemy.bounds.center);
            }
        }

        Debug.Log("Ultimate usada com dano: " + ultimateDamage);
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attack1Range);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(attackPoint.position, attack2Range);
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, ultimateRange);
    }
}