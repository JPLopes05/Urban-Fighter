using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerSkillEffects : MonoBehaviour
{
    [Header("Referências")]
    public LayerMask enemyLayer;

    private PlayerController playerController;
    private PlayerSkills playerSkills;
    private PlayerStats playerStats;
    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private CharacterVisualFeedback visualFeedback;

    [Header("Skill 1 - Salto de Impacto")]
    public float skill1JumpMultiplier = 2f;
    public float skill1ImpactRadius = 2f;
    public float skill1KnockbackUpForce = 2f;
    public float skill1KnockbackDuration = 0.25f;

    [Header("Skill 1 - Dano por nível")]
    public int skill1DamageLevel1 = 3;
    public int skill1DamageLevel2 = 5;
    public int skill1DamageLevel3 = 7;

    [Header("Skill 1 - Knockback horizontal por nível")]
    public float skill1KnockbackLevel1 = 4f;
    public float skill1KnockbackLevel2 = 6f;
    public float skill1KnockbackLevel3 = 8f;

    [Header("Skill 2 - Dash Recuo")]
    public float skill2DashDistance = 3f;
    public float skill2DashDuration = 0.18f;
    public float skill2ExtraClearStep = 0.2f;
    public int skill2MaxExtraSteps = 20;

    [Header("Skill 2 - Impacto na origem")]
    public float skill2OriginImpactRadius = 1.3f;
    public float skill2OriginKnockbackUpForce = 1.5f;
    public float skill2OriginKnockbackDuration = 0.2f;

    [Header("Skill 2 - Dano")]
    public int skill2DamageLevel2 = 3;
    public int skill2DamageLevel3 = 5;

    [Header("Skill 2 - Knockback")]
    public float skill2KnockbackLevel3 = 6f;

    [Header("Skill 3 - God Mode")]
    public float skill3DurationLevel1 = 3f;
    public float skill3DurationLevel2 = 5f;
    public float skill3DurationLevel3 = 5f;

    [Header("Skill 3 - Multiplicador de dano")]
    public float skill3DamageMultiplierLevel1 = 1.5f;
    public float skill3DamageMultiplierLevel2 = 1.75f;
    public float skill3DamageMultiplierLevel3 = 2f;

    private bool skill1InProgress = false;
    private bool skill1LeftGround = false;

    private bool skill2InProgress = false;
    private bool skill3InProgress = false;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerSkills = GetComponent<PlayerSkills>();
        playerStats = GetComponent<PlayerStats>();
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        visualFeedback = GetComponent<CharacterVisualFeedback>();
    }

    void Update()
    {
        HandleSkill1Landing();
    }

    public bool TryUseSkill1()
    {
        if (skill1InProgress)
        {
            Debug.Log("Skill 1 já está em execução.");
            return false;
        }

        if (playerController == null)
        {
            Debug.LogWarning("PlayerController não encontrado no Player.");
            return false;
        }

        if (!playerController.IsGrounded())
        {
            Debug.Log("Skill 1 só pode ser usada no chão.");
            return false;
        }

        if (visualFeedback != null)
        {
            visualFeedback.PlaySkillCastPulse(new Color(1f, 0.65f, 0.15f, 1f));
        }

        float specialJumpForce = playerController.jumpForce * skill1JumpMultiplier;
        playerController.ForceJump(specialJumpForce);

        skill1InProgress = true;
        skill1LeftGround = false;

        Debug.Log("Skill 1 iniciada: salto especial.");
        return true;
    }

    void HandleSkill1Landing()
    {
        if (!skill1InProgress || playerController == null)
            return;

        if (!skill1LeftGround)
        {
            if (!playerController.IsGrounded())
            {
                skill1LeftGround = true;
            }

            return;
        }

        if (playerController.IsGrounded())
        {
            ExecuteSkill1Impact();
            skill1InProgress = false;
            skill1LeftGround = false;
        }
    }

    void ExecuteSkill1Impact()
    {
        int skillLevel = playerSkills.GetSkillLevel(1);
        int damage = GetSkill1Damage(skillLevel);

        if (playerStats != null)
        {
            damage = playerStats.ApplyCurrentDamageMultipliers(damage);
        }

        float knockbackForce = GetSkill1Knockback(skillLevel);
        Vector3 impactPosition = GetGroundImpactPosition();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySkill1Impact();
        }

        SimpleVFX.SpawnGroundShockwave(
            impactPosition,
            skill1ImpactRadius
        );

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, skill1ImpactRadius, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            EnemyController enemyController = enemy.GetComponent<EnemyController>();

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }

            SimpleVFX.SpawnHitSpark(enemy.bounds.center);

            Vector2 horizontalDirection = enemy.transform.position.x >= transform.position.x ? Vector2.right : Vector2.left;
            Vector2 knockbackVector = new Vector2(horizontalDirection.x * knockbackForce, skill1KnockbackUpForce);

            if (enemyController != null)
            {
                enemyController.ApplyKnockback(knockbackVector, skill1KnockbackDuration);
            }
            else
            {
                Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();

                if (enemyRb != null)
                {
                    enemyRb.linearVelocity = knockbackVector;
                }
            }
        }

        Debug.Log("Skill 1 impactou no chão. Nível " + skillLevel + " | Dano: " + damage + " | Knockback: " + knockbackForce);
    }

    Vector3 GetGroundImpactPosition()
    {
        if (playerCollider != null)
        {
            Bounds bounds = playerCollider.bounds;
            return new Vector3(bounds.center.x, bounds.min.y + 0.08f, transform.position.z);
        }

        return transform.position + Vector3.down * 0.45f;
    }

    int GetSkill1Damage(int level)
    {
        switch (level)
        {
            case 1: return skill1DamageLevel1;
            case 2: return skill1DamageLevel2;
            case 3: return skill1DamageLevel3;
            default: return 0;
        }
    }

    float GetSkill1Knockback(int level)
    {
        switch (level)
        {
            case 1: return skill1KnockbackLevel1;
            case 2: return skill1KnockbackLevel2;
            case 3: return skill1KnockbackLevel3;
            default: return 0f;
        }
    }

    public bool TryUseSkill2()
    {
        if (skill2InProgress)
        {
            Debug.Log("Skill 2 já está em execução.");
            return false;
        }

        if (playerController == null || rb == null || playerCollider == null)
        {
            Debug.LogWarning("Referências do player não encontradas para a Skill 2.");
            return false;
        }

        if (!playerController.IsGrounded())
        {
            Debug.Log("Skill 2 só pode ser usada no chão.");
            return false;
        }

        int skillLevel = playerSkills.GetSkillLevel(2);

        if (skillLevel <= 0)
        {
            Debug.Log("Skill 2 sem nível válido.");
            return false;
        }

        StartCoroutine(Skill2DashRoutine(skillLevel));
        return true;
    }

    IEnumerator Skill2DashRoutine(int skillLevel)
    {
        skill2InProgress = true;
        playerController.SetMovementLocked(true);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySkill2Dash();
        }

        Vector3 dashOrigin = transform.position;
        Vector2 dashDirection = playerController.IsFacingRight() ? Vector2.left : Vector2.right;

        if (visualFeedback != null)
        {
            visualFeedback.PlaySkillCastPulse(new Color(0.55f, 0.85f, 1f, 1f));
        }

        List<Collider2D> ignoredEnemyColliders = new List<Collider2D>();
        IgnoreEnemyCollisions(true, ignoredEnemyColliders);

        if (skillLevel >= 2)
        {
            ExecuteSkill2OriginImpact(dashOrigin, skillLevel);
        }

        float dashSpeed = skill2DashDistance / skill2DashDuration;
        float elapsed = 0f;
        float nextAfterimageTime = 0f;

        while (elapsed < skill2DashDuration)
        {
            rb.linearVelocity = new Vector2(dashDirection.x * dashSpeed, rb.linearVelocity.y);

            if (Time.time >= nextAfterimageTime && visualFeedback != null)
            {
                SimpleVFX.SpawnAfterimage(
                    visualFeedback.GetSpriteRenderer(),
                    0.18f,
                    new Color(0.45f, 0.8f, 1f, 0.35f)
                );

                nextAfterimageTime = Time.time + 0.04f;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        int extraStepCount = 0;

        while (IsOverlappingEnemy() && extraStepCount < skill2MaxExtraSteps)
        {
            float clearDuration = 0.05f;
            float clearSpeed = skill2ExtraClearStep / clearDuration;
            float clearElapsed = 0f;

            while (clearElapsed < clearDuration)
            {
                rb.linearVelocity = new Vector2(dashDirection.x * clearSpeed, rb.linearVelocity.y);
                clearElapsed += Time.deltaTime;
                yield return null;
            }

            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            extraStepCount++;
        }

        if (IsOverlappingEnemy())
        {
            Debug.LogWarning("Skill 2 terminou, mas o player ainda parece sobreposto a um inimigo. Aumente skill2MaxExtraSteps se necessário.");
        }

        IgnoreEnemyCollisions(false, ignoredEnemyColliders);
        playerController.SetMovementLocked(false);

        skill2InProgress = false;
        Debug.Log("Skill 2 finalizada.");
    }

    void ExecuteSkill2OriginImpact(Vector3 originPosition, int skillLevel)
    {
        if (skillLevel < 2)
            return;

        int damage = GetSkill2Damage(skillLevel);

        if (playerStats != null)
        {
            damage = playerStats.ApplyCurrentDamageMultipliers(damage);
        }

        SimpleVFX.SpawnCircleBurst(
            originPosition + Vector3.up * 0.55f,
            skill2OriginImpactRadius,
            new Color(0.45f, 0.8f, 1f, 0.55f),
            0.28f,
            75
        );

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(originPosition, skill2OriginImpactRadius, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            EnemyController enemyController = enemy.GetComponent<EnemyController>();

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }

            SimpleVFX.SpawnHitSpark(enemy.bounds.center);

            if (skillLevel >= 3)
            {
                Vector2 horizontalDirection = enemy.transform.position.x >= originPosition.x ? Vector2.right : Vector2.left;
                Vector2 knockbackVector = new Vector2(horizontalDirection.x * skill2KnockbackLevel3, skill2OriginKnockbackUpForce);

                if (enemyController != null)
                {
                    enemyController.ApplyKnockback(knockbackVector, skill2OriginKnockbackDuration);
                }
                else
                {
                    Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();

                    if (enemyRb != null)
                    {
                        enemyRb.linearVelocity = knockbackVector;
                    }
                }
            }
        }

        Debug.Log("Skill 2 executou impacto na origem. Nível " + skillLevel + " | Dano: " + damage);
    }

    int GetSkill2Damage(int level)
    {
        switch (level)
        {
            case 2: return skill2DamageLevel2;
            case 3: return skill2DamageLevel3;
            default: return 0;
        }
    }

    bool IsOverlappingEnemy()
    {
        if (playerCollider == null)
            return false;

        Vector2 boxCenter = playerCollider.bounds.center;
        Vector2 boxSize = playerCollider.bounds.size * 0.9f;

        Collider2D[] overlaps = Physics2D.OverlapBoxAll(boxCenter, boxSize, 0f, enemyLayer);
        return overlaps.Length > 0;
    }

    void IgnoreEnemyCollisions(bool ignore, List<Collider2D> cache)
    {
        if (playerCollider == null)
            return;

        if (ignore)
        {
            cache.Clear();

            Collider2D[] allColliders = FindObjectsByType<Collider2D>(FindObjectsSortMode.None);

            foreach (Collider2D col in allColliders)
            {
                if (col == null || col == playerCollider)
                    continue;

                if (IsInLayerMask(col.gameObject.layer, enemyLayer))
                {
                    Physics2D.IgnoreCollision(playerCollider, col, true);
                    cache.Add(col);
                }
            }
        }
        else
        {
            foreach (Collider2D col in cache)
            {
                if (col != null)
                {
                    Physics2D.IgnoreCollision(playerCollider, col, false);
                }
            }

            cache.Clear();
        }
    }

    bool IsInLayerMask(int layer, LayerMask mask)
    {
        return ((1 << layer) & mask.value) != 0;
    }

    public bool TryUseSkill3()
    {
        if (skill3InProgress)
        {
            Debug.Log("Skill 3 já está ativa.");
            return false;
        }

        if (playerStats == null || playerSkills == null)
        {
            Debug.LogWarning("Referências não encontradas para a Skill 3.");
            return false;
        }

        int skillLevel = playerSkills.GetSkillLevel(3);

        if (skillLevel <= 0)
        {
            Debug.Log("Skill 3 sem nível válido.");
            return false;
        }

        float duration = GetSkill3Duration(skillLevel);
        float damageMultiplier = GetSkill3DamageMultiplier(skillLevel);

        StartCoroutine(Skill3Routine(duration, damageMultiplier, skillLevel));
        return true;
    }

    IEnumerator Skill3Routine(float duration, float damageMultiplier, int skillLevel)
    {
        skill3InProgress = true;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySkill3Aura();
        }

        if (visualFeedback != null)
        {
            visualFeedback.PlaySkillCastPulse(new Color(0.35f, 0.85f, 1f, 1f));
        }

        if (playerStats != null)
        {
            playerStats.ActivateGodMode(duration, damageMultiplier);
        }

        Debug.Log("Skill 3 ativada. Nível " + skillLevel + " | Duração: " + duration + "s | Multiplicador: " + damageMultiplier);

        yield return new WaitForSeconds(duration);

        skill3InProgress = false;
        Debug.Log("Skill 3 finalizada.");
    }

    float GetSkill3Duration(int level)
    {
        switch (level)
        {
            case 1: return skill3DurationLevel1;
            case 2: return skill3DurationLevel2;
            case 3: return skill3DurationLevel3;
            default: return 0f;
        }
    }

    float GetSkill3DamageMultiplier(int level)
    {
        switch (level)
        {
            case 1: return skill3DamageMultiplierLevel1;
            case 2: return skill3DamageMultiplierLevel2;
            case 3: return skill3DamageMultiplierLevel3;
            default: return 1f;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, skill1ImpactRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, skill2OriginImpactRadius);
    }
}