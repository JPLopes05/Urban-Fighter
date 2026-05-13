using UnityEngine;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    private SkillProgressionManager skillProgressionManager;
    private CharacterVisualFeedback visualFeedback;

    [Header("Level")]
    public int currentLevel = 1;
    public int maxLevel = 25;

    [Header("HP")]
    public float maxHP = 180f;
    public float currentHP = 180f;
    public float hpIncreasePerLevel = 8f;

    [Header("Invulnerabilidade após dano")]
    public float hurtInvulnerabilityDuration = 0.45f;
    public bool isHurtInvulnerable = false;

    [Header("XP")]
    public float currentXP = 0f;
    public float xpToNextLevel = 70f;
    public float xpGrowthMultiplier = 1.12f;

    [Header("ULT")]
    public float maxULT = 100f;
    public float currentULT = 0f;

    [Header("Ultimate Tier")]
    public int currentUltimateTier = 1;
    public float ultimateTierMultiplierLevel1 = 1f;
    public float ultimateTierMultiplierLevel2 = 1.35f;
    public float ultimateTierMultiplierLevel3 = 1.7f;

    [Header("Ataques Básicos")]
    public int baseAttack1Damage = 8;
    public int baseAttack2Damage = 14;
    public int damageIncreasePerLevel = 1;

    [Header("Ultimate")]
    public int baseUltimateDamage = 45;
    public int ultimateDamageIncreasePerLevel = 3;

    [Header("Buffs")]
    public float attackMultiplier = 1f;
    public float defenseReductionPercent = 0f;

    [Header("God Mode")]
    public bool isInvincible = false;
    public float godModeDamageMultiplier = 1f;

    [Header("Estado da fase")]
    [SerializeField] private LevelStateManager levelStateManager;

    private bool isDead = false;

    private Coroutine attackBuffRoutine;
    private Coroutine defenseBuffRoutine;
    private Coroutine godModeRoutine;
    private Coroutine hurtInvulnerabilityRoutine;

    void Awake()
    {
        skillProgressionManager = GetComponent<SkillProgressionManager>();
        visualFeedback = GetComponent<CharacterVisualFeedback>();

        currentLevel = Mathf.Clamp(currentLevel, 1, maxLevel);
        currentHP = Mathf.Clamp(currentHP, 0f, maxHP);
        currentULT = Mathf.Clamp(currentULT, 0f, maxULT);

        isHurtInvulnerable = false;
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
        {
            return;
        }

        if (isInvincible)
        {
            if (visualFeedback != null)
            {
                visualFeedback.PlayInvincibleBlockFeedback();
            }

            Debug.Log("Player está invencível e ignorou o dano.");
            return;
        }

        if (isHurtInvulnerable)
        {
            if (visualFeedback != null)
            {
                visualFeedback.PlayInvincibleBlockFeedback();
            }

            Debug.Log("Player ignorou dano por invulnerabilidade curta após hit.");
            return;
        }

        float finalDamage = damage * (1f - defenseReductionPercent);
        finalDamage = Mathf.Max(0f, finalDamage);

        currentHP -= finalDamage;
        currentHP = Mathf.Clamp(currentHP, 0f, maxHP);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayHitPlayer();
        }

        if (visualFeedback != null)
        {
            visualFeedback.PlayHitFlash();
        }

        SimpleVFX.SpawnHitSpark(transform.position + Vector3.up * 0.8f);

        StartHurtInvulnerability();

        Debug.Log("Player tomou " + finalDamage + " de dano. HP atual: " + currentHP);

        if (currentHP <= 0f)
        {
            Die();
        }
    }

    void StartHurtInvulnerability()
    {
        if (hurtInvulnerabilityRoutine != null)
        {
            StopCoroutine(hurtInvulnerabilityRoutine);
        }

        hurtInvulnerabilityRoutine = StartCoroutine(HurtInvulnerabilityCoroutine());
    }

    IEnumerator HurtInvulnerabilityCoroutine()
    {
        isHurtInvulnerable = true;

        yield return new WaitForSeconds(hurtInvulnerabilityDuration);

        isHurtInvulnerable = false;
        hurtInvulnerabilityRoutine = null;
    }

    public void HealPercent(float percent)
    {
        if (isDead)
            return;

        float healAmount = maxHP * (percent / 100f);
        currentHP += healAmount;
        currentHP = Mathf.Clamp(currentHP, 0f, maxHP);

        SimpleVFX.SpawnCollect(transform.position + Vector3.up * 0.8f, new Color(0.2f, 1f, 0.35f, 0.8f));

        Debug.Log("Player curou " + percent + "% da vida.");
    }

    public void ApplyAttackBuff(float percent, float duration)
    {
        if (attackBuffRoutine != null)
        {
            StopCoroutine(attackBuffRoutine);
        }

        attackBuffRoutine = StartCoroutine(AttackBuffCoroutine(percent, duration));
    }

    public void ApplyDefenseBuff(float percent, float duration)
    {
        if (defenseBuffRoutine != null)
        {
            StopCoroutine(defenseBuffRoutine);
        }

        defenseBuffRoutine = StartCoroutine(DefenseBuffCoroutine(percent, duration));
    }

    public void ActivateGodMode(float duration, float damageMultiplier)
    {
        if (godModeRoutine != null)
        {
            StopCoroutine(godModeRoutine);
        }

        if (visualFeedback != null)
        {
            visualFeedback.SetAuraActive(false, Color.clear);
        }

        godModeRoutine = StartCoroutine(GodModeCoroutine(duration, damageMultiplier));
    }

    IEnumerator AttackBuffCoroutine(float percent, float duration)
    {
        attackMultiplier = 1f + (percent / 100f);

        if (visualFeedback != null)
        {
            visualFeedback.PlaySkillCastPulse(new Color(1f, 0.45f, 0.1f, 1f));
        }

        Debug.Log("Buff de ataque ativado: +" + percent + "% por " + duration + "s");

        yield return new WaitForSeconds(duration);

        attackMultiplier = 1f;
        attackBuffRoutine = null;
        Debug.Log("Buff de ataque terminou.");
    }

    IEnumerator DefenseBuffCoroutine(float percent, float duration)
    {
        defenseReductionPercent = percent / 100f;

        if (visualFeedback != null)
        {
            visualFeedback.PlaySkillCastPulse(new Color(0.25f, 0.65f, 1f, 1f));
        }

        Debug.Log("Buff de defesa ativado: +" + percent + "% por " + duration + "s");

        yield return new WaitForSeconds(duration);

        defenseReductionPercent = 0f;
        defenseBuffRoutine = null;
        Debug.Log("Buff de defesa terminou.");
    }

    IEnumerator GodModeCoroutine(float duration, float damageMultiplier)
    {
        isInvincible = true;
        godModeDamageMultiplier = damageMultiplier;

        if (visualFeedback != null)
        {
            visualFeedback.SetAuraActive(true, new Color(0.35f, 0.8f, 1f, 0.35f));
        }

        Debug.Log("God Mode ativado por " + duration + "s | Multiplicador de dano: " + damageMultiplier);

        yield return new WaitForSeconds(duration);

        isInvincible = false;
        godModeDamageMultiplier = 1f;
        godModeRoutine = null;

        if (visualFeedback != null)
        {
            visualFeedback.SetAuraActive(false, Color.clear);
        }

        Debug.Log("God Mode terminou.");
    }

    public bool IsGodModeActive()
    {
        return isInvincible;
    }

    public void AddXP(float amount)
    {
        if (amount <= 0f)
            return;

        if (currentLevel >= maxLevel)
        {
            currentLevel = maxLevel;
            currentXP = xpToNextLevel;
            Debug.Log("Player já está no nível máximo. XP mantido no limite.");
            return;
        }

        currentXP += amount;
        Debug.Log("Player ganhou " + amount + " de XP. XP atual: " + currentXP + " / " + xpToNextLevel);

        while (currentXP >= xpToNextLevel && currentLevel < maxLevel)
        {
            currentXP -= xpToNextLevel;
            LevelUp();
        }

        if (currentLevel >= maxLevel)
        {
            currentLevel = maxLevel;
            currentXP = xpToNextLevel;
            Debug.Log("Player atingiu o nível máximo: " + maxLevel);
        }
    }

    public void AddULT(float amount)
    {
        currentULT += amount;
        currentULT = Mathf.Clamp(currentULT, 0f, maxULT);

        Debug.Log("Player ganhou " + amount + " de ULT. ULT atual: " + currentULT + " / " + maxULT);
    }

    public bool IsUltimateReady()
    {
        return currentULT >= maxULT;
    }

    public void ConsumeULT()
    {
        currentULT = 0f;
        Debug.Log("Ultimate usada. Barra de ULT zerada.");
    }

    void LevelUp()
    {
        if (currentLevel >= maxLevel)
        {
            currentLevel = maxLevel;
            currentXP = xpToNextLevel;
            return;
        }

        currentLevel++;

        float previousMaxHP = maxHP;

        maxHP += hpIncreasePerLevel;

        float hpIncreaseAmount = maxHP - previousMaxHP;
        currentHP += hpIncreaseAmount;
        currentHP = Mathf.Clamp(currentHP, 0f, maxHP);

        xpToNextLevel *= xpGrowthMultiplier;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayLevelUp();
        }

        if (visualFeedback != null)
        {
            visualFeedback.PlayLevelUpEffect();
        }

        Debug.Log("LEVEL UP! Agora o player está no nível " + currentLevel);
        Debug.Log("Nova vida máxima: " + maxHP + " | HP atual ajustado para: " + currentHP);

        if (skillProgressionManager != null)
        {
            skillProgressionManager.HandleLevelProgression(currentLevel);
        }
    }

    public void UpgradeUltimateTier()
    {
        if (currentUltimateTier < 3)
        {
            currentUltimateTier++;
            Debug.Log("Ultimate subiu para o nível " + currentUltimateTier + ".");
        }
        else
        {
            Debug.Log("Ultimate já está no nível máximo.");
        }
    }

    public void EnsureUltimateTierAtLeast(int targetTier)
    {
        targetTier = Mathf.Clamp(targetTier, 1, 3);

        while (currentUltimateTier < targetTier)
        {
            UpgradeUltimateTier();
        }
    }

    float GetUltimateTierMultiplier()
    {
        switch (currentUltimateTier)
        {
            case 1: return ultimateTierMultiplierLevel1;
            case 2: return ultimateTierMultiplierLevel2;
            case 3: return ultimateTierMultiplierLevel3;
            default: return 1f;
        }
    }

    public int ApplyCurrentDamageMultipliers(int baseDamage)
    {
        float rawDamage = baseDamage * attackMultiplier * godModeDamageMultiplier;
        return Mathf.RoundToInt(rawDamage);
    }

    public int GetAttack1Damage()
    {
        int baseValue = baseAttack1Damage + ((currentLevel - 1) * damageIncreasePerLevel);
        return ApplyCurrentDamageMultipliers(baseValue);
    }

    public int GetAttack2Damage()
    {
        int baseValue = baseAttack2Damage + ((currentLevel - 1) * damageIncreasePerLevel);
        return ApplyCurrentDamageMultipliers(baseValue);
    }

    public int GetUltimateDamage()
    {
        int baseValue = baseUltimateDamage + ((currentLevel - 1) * ultimateDamageIncreasePerLevel);
        float rawDamage = baseValue * attackMultiplier * godModeDamageMultiplier * GetUltimateTierMultiplier();
        return Mathf.RoundToInt(rawDamage);
    }

    void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        currentHP = 0f;

        Debug.Log("Player morreu.");

        if (levelStateManager != null)
        {
            levelStateManager.GameOver();
        }
        else
        {
            Debug.LogWarning("PlayerStats está sem LevelStateManager configurado.");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TakeDamage(10f);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            AddXP(25f);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            AddULT(20f);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            currentHP = maxHP;
            currentXP = 0f;
            currentULT = 0f;
        }
    }
}