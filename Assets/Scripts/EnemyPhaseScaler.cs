using UnityEngine;
using UnityEngine.SceneManagement;
using System.Reflection;

[DisallowMultipleComponent]
public class EnemyPhaseScaler : MonoBehaviour
{
    [Header("Ativar escalonamento")]
    public bool scaleHealth = true;
    public bool scaleDamage = true;
    public bool scaleRewards = true;
    public bool scaleDropChance = true;

    [Header("Multiplicadores de HP por fase")]
    public float fase1HealthMultiplier = 1f;
    public float fase2HealthMultiplier = 1.6f;
    public float fase3HealthMultiplier = 2.4f;

    [Header("Multiplicadores de dano por fase")]
    public float fase1DamageMultiplier = 1f;
    public float fase2DamageMultiplier = 1.15f;
    public float fase3DamageMultiplier = 1.30f;

    [Header("Multiplicadores de XP por fase")]
    public float fase1XPMultiplier = 1f;
    public float fase2XPMultiplier = 1.8f;
    public float fase3XPMultiplier = 2.5f;

    [Header("Multiplicadores de ULT por fase")]
    public float fase1ULTMultiplier = 1f;
    public float fase2ULTMultiplier = 1.2f;
    public float fase3ULTMultiplier = 1.35f;

    [Header("Multiplicadores de drop por fase")]
    public float fase1DropMultiplier = 1f;
    public float fase2DropMultiplier = 1.15f;
    public float fase3DropMultiplier = 1.25f;
    public float maxDropChance = 0.50f;

    [Header("Boss")]
    public bool affectBossController = false;

    private bool applied = false;

    void Start()
    {
        ApplyPhaseScaling();
    }

    void ApplyPhaseScaling()
    {
        if (applied)
            return;

        applied = true;

        string sceneName = SceneManager.GetActiveScene().name;

        float hpMultiplier = GetHealthMultiplier(sceneName);
        float damageMultiplier = GetDamageMultiplier(sceneName);
        float xpMultiplier = GetXPMultiplier(sceneName);
        float ultMultiplier = GetULTMultiplier(sceneName);
        float dropMultiplier = GetDropMultiplier(sceneName);

        EnemyHealth health = GetComponent<EnemyHealth>();

        if (health != null && (scaleHealth || scaleRewards))
        {
            float finalHpMultiplier = scaleHealth ? hpMultiplier : 1f;
            float finalXpMultiplier = scaleRewards ? xpMultiplier : 1f;
            float finalUltMultiplier = scaleRewards ? ultMultiplier : 1f;

            health.ApplyBalanceMultipliers(finalHpMultiplier, finalXpMultiplier, finalUltMultiplier);
        }

        if (scaleDamage)
        {
            ScaleEnemyDamage(damageMultiplier);
        }

        if (scaleDropChance)
        {
            ScaleDropChance(dropMultiplier);
        }

        Debug.Log(gameObject.name + " recebeu escala da cena " + sceneName +
                  " | HP x" + hpMultiplier +
                  " | Dano x" + damageMultiplier +
                  " | XP x" + xpMultiplier +
                  " | ULT x" + ultMultiplier +
                  " | Drop x" + dropMultiplier);
    }

    float GetHealthMultiplier(string sceneName)
    {
        if (sceneName.Contains("Fase3")) return fase3HealthMultiplier;
        if (sceneName.Contains("Fase2")) return fase2HealthMultiplier;
        return fase1HealthMultiplier;
    }

    float GetDamageMultiplier(string sceneName)
    {
        if (sceneName.Contains("Fase3")) return fase3DamageMultiplier;
        if (sceneName.Contains("Fase2")) return fase2DamageMultiplier;
        return fase1DamageMultiplier;
    }

    float GetXPMultiplier(string sceneName)
    {
        if (sceneName.Contains("Fase3")) return fase3XPMultiplier;
        if (sceneName.Contains("Fase2")) return fase2XPMultiplier;
        return fase1XPMultiplier;
    }

    float GetULTMultiplier(string sceneName)
    {
        if (sceneName.Contains("Fase3")) return fase3ULTMultiplier;
        if (sceneName.Contains("Fase2")) return fase2ULTMultiplier;
        return fase1ULTMultiplier;
    }

    float GetDropMultiplier(string sceneName)
    {
        if (sceneName.Contains("Fase3")) return fase3DropMultiplier;
        if (sceneName.Contains("Fase2")) return fase2DropMultiplier;
        return fase1DropMultiplier;
    }

    void ScaleEnemyDamage(float damageMultiplier)
    {
        EnemyController enemyController = GetComponent<EnemyController>();

        if (enemyController != null)
        {
            enemyController.attackDamage = Mathf.Round(enemyController.attackDamage * damageMultiplier);
        }

        if (affectBossController)
        {
            BossController bossController = GetComponent<BossController>();

            if (bossController != null)
            {
                bossController.attackDamage = Mathf.Round(bossController.attackDamage * damageMultiplier);
                bossController.specialDamage = Mathf.Round(bossController.specialDamage * damageMultiplier);
            }
        }
    }

    void ScaleDropChance(float dropMultiplier)
    {
        MonoBehaviour[] behaviours = GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour == null)
                continue;

            if (behaviour.GetType().Name != "EnemyDropper")
                continue;

            TryScaleDropChanceField(behaviour, dropMultiplier);
        }
    }

    void TryScaleDropChanceField(MonoBehaviour dropper, float dropMultiplier)
    {
        FieldInfo field = FindDropChanceField(dropper.GetType());

        if (field == null)
        {
            Debug.LogWarning("EnemyPhaseScaler não encontrou campo de drop chance em " + dropper.GetType().Name);
            return;
        }

        object rawValue = field.GetValue(dropper);

        if (rawValue is float floatValue)
        {
            float scaledValue = ScaleDropValue(floatValue, dropMultiplier);
            field.SetValue(dropper, scaledValue);
            Debug.Log(gameObject.name + " drop chance ajustado para " + scaledValue);
        }
        else if (rawValue is double doubleValue)
        {
            double scaledValue = ScaleDropValue((float)doubleValue, dropMultiplier);
            field.SetValue(dropper, scaledValue);
            Debug.Log(gameObject.name + " drop chance ajustado para " + scaledValue);
        }
        else if (rawValue is int intValue)
        {
            int scaledValue = Mathf.RoundToInt(ScaleDropValue(intValue, dropMultiplier));
            field.SetValue(dropper, scaledValue);
            Debug.Log(gameObject.name + " drop chance ajustado para " + scaledValue);
        }
    }

    FieldInfo FindDropChanceField(System.Type type)
    {
        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        FieldInfo exactField = type.GetField("dropChance", flags);
        if (exactField != null)
            return exactField;

        FieldInfo[] fields = type.GetFields(flags);

        foreach (FieldInfo field in fields)
        {
            string lowerName = field.Name.ToLowerInvariant();

            if (lowerName.Contains("drop") && lowerName.Contains("chance"))
            {
                return field;
            }

            if (lowerName.Contains("drop") && lowerName.Contains("rate"))
            {
                return field;
            }
        }

        return null;
    }

    float ScaleDropValue(float originalValue, float dropMultiplier)
    {
        float scaledValue = originalValue * dropMultiplier;

        if (originalValue <= 1f)
        {
            return Mathf.Clamp(scaledValue, 0f, maxDropChance);
        }

        return Mathf.Clamp(scaledValue, 0f, maxDropChance * 100f);
    }
}