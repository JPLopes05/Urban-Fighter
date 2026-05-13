using UnityEngine;
using System.Collections;

public class BossController : MonoBehaviour
{
    public enum BossSpecialType
    {
        None,
        HeavyStrike,
        KnockbackBlast,
        SelfRegen,
        AreaShockwave,
        SpeedBurst
    }

    [Header("Referências")]
    public Transform player;
    private PlayerStats playerStats;
    private Rigidbody2D playerRb;
    private Rigidbody2D rb;
    private EnemyHealth enemyHealth;

    [Header("Movimento Base")]
    public float moveSpeed = 1.5f;
    public float detectDistance = 12f;
    public float attackDistance = 1.5f;

    [Header("Ataque Base")]
    public float attackDamage = 18f;
    public float attackCooldown = 1.8f;

    [Header("Hit Stun")]
    public bool canBeStunned = true;
    public bool ignoreStunWhileCastingSpecial = true;

    [Header("Especial do Boss")]
    public BossSpecialType specialType = BossSpecialType.None;
    public float specialCooldown = 6f;
    public float specialCastDelay = 0.3f;
    public float specialRange = 2.2f;
    public float specialDamage = 25f;
    public int specialRegenAmount = 3;
    public float specialKnockbackForceX = 8f;
    public float specialKnockbackForceY = 2.5f;
    public float speedBurstMultiplier = 1.6f;
    public float speedBurstDuration = 2f;
    public bool useSpecialOnlyInRange = true;
    public bool stopMovementWhileCasting = true;

    private float nextAttackTime = 0f;
    private float nextSpecialTime = 0f;
    private bool facingRight = true;
    private bool isCastingSpecial = false;
    private float baseMoveSpeed;
    private Coroutine speedBurstRoutine;

    private bool isStunned = false;
    private float stunEndTime = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyHealth = GetComponent<EnemyHealth>();
        baseMoveSpeed = moveSpeed;
    }

    void Start()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
                playerStats = playerObject.GetComponent<PlayerStats>();
                playerRb = playerObject.GetComponent<Rigidbody2D>();
            }
        }
        else
        {
            playerStats = player.GetComponent<PlayerStats>();
            playerRb = player.GetComponent<Rigidbody2D>();
        }
    }

    void Update()
    {
        if (player == null) return;

        if (isStunned)
        {
            if (Time.time >= stunEndTime)
            {
                isStunned = false;
            }

            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        FlipTowardPlayer();

        if (!isCastingSpecial && specialType != BossSpecialType.None && Time.time >= nextSpecialTime)
        {
            if (!useSpecialOnlyInRange || distanceToPlayer <= specialRange)
            {
                StartCoroutine(UseSpecialRoutine());
                return;
            }
        }

        if (!isCastingSpecial && distanceToPlayer <= attackDistance && Time.time >= nextAttackTime)
        {
            BasicAttack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    void FixedUpdate()
    {
        if (player == null)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        if (isStunned)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        if (isCastingSpecial && stopMovementWhileCasting)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer > detectDistance)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        if (distanceToPlayer <= attackDistance)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        float direction = player.position.x > transform.position.x ? 1f : -1f;
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
    }

    void BasicAttack()
    {
        if (playerStats != null)
        {
            playerStats.TakeDamage(attackDamage);
            Debug.Log(gameObject.name + " atacou o player e causou " + attackDamage + " de dano.");
        }
    }

    IEnumerator UseSpecialRoutine()
    {
        isCastingSpecial = true;
        nextSpecialTime = Time.time + specialCooldown;

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        Debug.Log(gameObject.name + " iniciou a habilidade especial: " + specialType);

        if (specialCastDelay > 0f)
        {
            yield return new WaitForSeconds(specialCastDelay);
        }

        float distanceToPlayer = player != null ? Vector2.Distance(transform.position, player.position) : float.PositiveInfinity;

        switch (specialType)
        {
            case BossSpecialType.HeavyStrike:
                if (distanceToPlayer <= specialRange)
                {
                    DamagePlayer(specialDamage);
                }
                break;

            case BossSpecialType.KnockbackBlast:
                if (distanceToPlayer <= specialRange)
                {
                    DamagePlayer(specialDamage);
                    ApplyKnockbackToPlayer();
                }
                break;

            case BossSpecialType.SelfRegen:
                if (enemyHealth != null)
                {
                    enemyHealth.Heal(specialRegenAmount);
                }
                break;

            case BossSpecialType.AreaShockwave:
                if (distanceToPlayer <= specialRange)
                {
                    DamagePlayer(specialDamage);
                    ApplyKnockbackToPlayer();
                }
                break;

            case BossSpecialType.SpeedBurst:
                ActivateSpeedBurst();
                break;
        }

        isCastingSpecial = false;
    }

    void DamagePlayer(float damage)
    {
        if (playerStats != null)
        {
            playerStats.TakeDamage(damage);
            Debug.Log(gameObject.name + " usou habilidade especial e causou " + damage + " de dano.");
        }
    }

    void ApplyKnockbackToPlayer()
    {
        if (player == null || playerRb == null)
            return;

        Vector2 direction = player.position.x >= transform.position.x ? Vector2.right : Vector2.left;
        Vector2 knockback = new Vector2(direction.x * specialKnockbackForceX, specialKnockbackForceY);

        playerRb.linearVelocity = knockback;
        Debug.Log(gameObject.name + " aplicou knockback no player.");
    }

    void ActivateSpeedBurst()
    {
        if (speedBurstRoutine != null)
        {
            StopCoroutine(speedBurstRoutine);
        }

        speedBurstRoutine = StartCoroutine(SpeedBurstRoutine());
    }

    IEnumerator SpeedBurstRoutine()
    {
        moveSpeed = baseMoveSpeed * speedBurstMultiplier;
        Debug.Log(gameObject.name + " ativou Speed Burst.");

        yield return new WaitForSeconds(speedBurstDuration);

        moveSpeed = baseMoveSpeed;
        speedBurstRoutine = null;

        Debug.Log(gameObject.name + " terminou o Speed Burst.");
    }

    public void ApplyHitStun(float duration)
    {
        if (!canBeStunned)
            return;

        if (duration <= 0f)
            return;

        if (ignoreStunWhileCastingSpecial && isCastingSpecial)
            return;

        isStunned = true;
        stunEndTime = Mathf.Max(stunEndTime, Time.time + duration);

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
    }

    void FlipTowardPlayer()
    {
        if (player.position.x < transform.position.x && facingRight)
        {
            Flip();
        }
        else if (player.position.x > transform.position.x && !facingRight)
        {
            Flip();
        }
    }

    void Flip()
    {
        facingRight = !facingRight;

        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, specialRange);
    }
}