using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Referências")]
    public Transform player;
    private PlayerStats playerStats;
    private Rigidbody2D rb;

    [Header("Movimento")]
    public float moveSpeed = 2f;
    public float detectDistance = 10f;
    public float attackDistance = 1.2f;

    [Header("Ataque")]
    public float attackDamage = 10f;
    public float attackCooldown = 1.2f;

    [Header("Knockback")]
    public bool canBeKnockedBack = true;

    [Header("Hit Stun")]
    public bool canBeStunned = true;

    private float nextAttackTime = 0f;
    private bool facingRight = true;

    private bool isKnockedBack = false;
    private float knockbackEndTime = 0f;

    private bool isStunned = false;
    private float stunEndTime = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
            }
        }
        else
        {
            playerStats = player.GetComponent<PlayerStats>();
        }
    }

    void Update()
    {
        if (isKnockedBack)
        {
            if (Time.time >= knockbackEndTime)
            {
                isKnockedBack = false;
            }

            return;
        }

        if (isStunned)
        {
            if (Time.time >= stunEndTime)
            {
                isStunned = false;
            }

            return;
        }

        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (player.position.x < transform.position.x && facingRight)
        {
            Flip();
        }
        else if (player.position.x > transform.position.x && !facingRight)
        {
            Flip();
        }

        if (distanceToPlayer <= attackDistance && Time.time >= nextAttackTime)
        {
            Attack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    void FixedUpdate()
    {
        if (isKnockedBack)
            return;

        if (isStunned)
        {
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }

            return;
        }

        if (player == null)
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

    void Attack()
    {
        if (playerStats != null)
        {
            playerStats.TakeDamage(attackDamage);
            Debug.Log(gameObject.name + " atacou o player e causou " + attackDamage + " de dano.");
        }
    }

    void Flip()
    {
        facingRight = !facingRight;

        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    public void ApplyKnockback(Vector2 force, float duration)
    {
        if (!canBeKnockedBack)
            return;

        isKnockedBack = true;
        knockbackEndTime = Time.time + duration;

        if (rb != null)
        {
            rb.linearVelocity = force;
        }
    }

    public void ApplyHitStun(float duration)
    {
        if (!canBeStunned)
            return;

        if (duration <= 0f)
            return;

        if (isKnockedBack)
            return;

        isStunned = true;
        stunEndTime = Mathf.Max(stunEndTime, Time.time + duration);

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }
}