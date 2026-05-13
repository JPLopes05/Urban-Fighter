using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerProgressPersistence : MonoBehaviour
{
    [Header("Aplicação automática")]
    [SerializeField] private bool applySavedProgressOnStart = true;
    [SerializeField] private bool logDetails = true;

    [Header("Componentes que representam progressão")]
    [SerializeField] private List<string> componentTypeNamesToPersist = new List<string>
    {
        "PlayerStats",
        "SkillProgressionManager"
    };

    private void Reset()
    {
        EnsureDefaultComponentTypeNames();
    }

    private void Awake()
    {
        EnsureDefaultComponentTypeNames();
    }

    private IEnumerator Start()
    {
        yield return null;

        if (applySavedProgressOnStart && CampaignProgressStore.HasProgress)
        {
            ApplyStoredProgressToThisPlayer();
        }
    }

    public static void SaveCurrentPlayerProgress()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            Debug.LogWarning("Não foi possível salvar progresso: nenhum objeto com Tag Player foi encontrado.");
            return;
        }

        PlayerProgressPersistence persistence = playerObject.GetComponent<PlayerProgressPersistence>();

        if (persistence == null)
        {
            persistence = playerObject.AddComponent<PlayerProgressPersistence>();
        }

        persistence.SaveThisPlayerProgress();
    }

    public void SaveThisPlayerProgress()
    {
        List<ComponentProgressSnapshot> snapshots = CaptureProgressSnapshots();

        CampaignProgressStore.SaveProgress(
            SceneManager.GetActiveScene().name,
            snapshots
        );
    }

    private List<ComponentProgressSnapshot> CaptureProgressSnapshots()
    {
        List<ComponentProgressSnapshot> snapshots = new List<ComponentProgressSnapshot>();
        MonoBehaviour[] components = GetComponents<MonoBehaviour>();

        foreach (string componentTypeName in componentTypeNamesToPersist)
        {
            MonoBehaviour component = FindComponentByTypeName(components, componentTypeName);

            if (component == null)
            {
                if (logDetails)
                {
                    Debug.LogWarning("Componente não encontrado no Player para salvar progresso: " + componentTypeName);
                }

                continue;
            }

            ComponentProgressSnapshot snapshot = CaptureComponentSnapshot(component);
            snapshots.Add(snapshot);

            if (logDetails)
            {
                Debug.Log("Progresso capturado do componente: " + componentTypeName);
            }
        }

        return snapshots;
    }

    private ComponentProgressSnapshot CaptureComponentSnapshot(MonoBehaviour component)
    {
        Type componentType = component.GetType();
        ComponentProgressSnapshot snapshot = new ComponentProgressSnapshot(componentType.Name);

        foreach (FieldInfo field in GetAllInstanceFields(componentType))
        {
            if (!ShouldPersistField(field))
            {
                continue;
            }

            object value = field.GetValue(component);
            snapshot.fieldValues[field.Name] = value;
        }

        return snapshot;
    }

    private void ApplyStoredProgressToThisPlayer()
    {
        MonoBehaviour[] components = GetComponents<MonoBehaviour>();

        foreach (string componentTypeName in componentTypeNamesToPersist)
        {
            MonoBehaviour component = FindComponentByTypeName(components, componentTypeName);

            if (component == null)
            {
                if (logDetails)
                {
                    Debug.LogWarning("Componente não encontrado no Player para aplicar progresso: " + componentTypeName);
                }

                continue;
            }

            if (!CampaignProgressStore.TryGetSnapshot(componentTypeName, out ComponentProgressSnapshot snapshot))
            {
                continue;
            }

            ApplySnapshotToComponent(component, snapshot);

            if (logDetails)
            {
                Debug.Log("Progresso aplicado no componente: " + componentTypeName);
            }
        }

        RefreshProgressionAfterLoad();

        Debug.Log("Progresso da campanha aplicado ao Player sem restaurar HP para o máximo.");
    }

    private void ApplySnapshotToComponent(MonoBehaviour component, ComponentProgressSnapshot snapshot)
    {
        Type componentType = component.GetType();

        foreach (KeyValuePair<string, object> pair in snapshot.fieldValues)
        {
            FieldInfo field = GetFieldIncludingBaseTypes(componentType, pair.Key);

            if (field == null)
            {
                continue;
            }

            if (!ShouldPersistField(field))
            {
                continue;
            }

            try
            {
                object convertedValue = ConvertValueToFieldType(pair.Value, field.FieldType);
                field.SetValue(component, convertedValue);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Não foi possível aplicar o campo '" + pair.Key + "' em " + componentType.Name + ". Erro: " + exception.Message);
            }
        }
    }

    private void RefreshProgressionAfterLoad()
    {
        SkillProgressionManager progressionManager = GetComponent<SkillProgressionManager>();

        if (progressionManager != null)
        {
            progressionManager.RefreshProgressionAfterLoad();
        }

        TryRefreshHUD();
    }

    private void TryRefreshHUD()
    {
        HUDController[] hudControllers = FindObjectsByType<HUDController>(FindObjectsSortMode.None);

        foreach (HUDController hud in hudControllers)
        {
            if (hud != null && hud.playerStats == null)
            {
                hud.playerStats = GetComponent<PlayerStats>();
            }
        }
    }

    private MonoBehaviour FindComponentByTypeName(MonoBehaviour[] components, string componentTypeName)
    {
        foreach (MonoBehaviour component in components)
        {
            if (component == null)
            {
                continue;
            }

            if (component.GetType().Name == componentTypeName)
            {
                return component;
            }
        }

        return null;
    }

    private List<FieldInfo> GetAllInstanceFields(Type type)
    {
        List<FieldInfo> fields = new List<FieldInfo>();
        Type currentType = type;

        while (currentType != null && currentType != typeof(MonoBehaviour))
        {
            fields.AddRange(
                currentType.GetFields(
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly
                )
            );

            currentType = currentType.BaseType;
        }

        return fields;
    }

    private FieldInfo GetFieldIncludingBaseTypes(Type type, string fieldName)
    {
        Type currentType = type;

        while (currentType != null && currentType != typeof(MonoBehaviour))
        {
            FieldInfo field = currentType.GetField(
                fieldName,
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.DeclaredOnly
            );

            if (field != null)
            {
                return field;
            }

            currentType = currentType.BaseType;
        }

        return null;
    }

    private bool ShouldPersistField(FieldInfo field)
    {
        if (field == null)
        {
            return false;
        }

        if (field.IsStatic || field.IsLiteral || field.IsInitOnly || field.IsNotSerialized)
        {
            return false;
        }

        if (!IsSupportedFieldType(field.FieldType))
        {
            return false;
        }

        string lowerName = field.Name.ToLowerInvariant();

        if (lowerName.Contains("timer")) return false;
        if (lowerName.Contains("cooldown")) return false;
        if (lowerName.Contains("dead")) return false;
        if (lowerName.Contains("paused")) return false;
        if (lowerName.Contains("grounded")) return false;
        if (lowerName.Contains("attacking")) return false;
        if (lowerName.Contains("dashing")) return false;
        if (lowerName.Contains("invincible")) return false;
        if (lowerName.Contains("invulnerable")) return false;
        if (lowerName.Contains("velocity")) return false;
        if (lowerName.Contains("routine")) return false;

        // Buffs temporários não devem ser levados entre fases.
        if (lowerName.Contains("attackmultiplier")) return false;
        if (lowerName.Contains("defensereduction")) return false;
        if (lowerName.Contains("godmodedamagemultiplier")) return false;

        return true;
    }

    private bool IsSupportedFieldType(Type type)
    {
        return type == typeof(int) ||
               type == typeof(float) ||
               type == typeof(double) ||
               type == typeof(bool) ||
               type == typeof(string) ||
               type.IsEnum;
    }

    private object ConvertValueToFieldType(object value, Type targetType)
    {
        if (value == null)
        {
            return null;
        }

        if (targetType.IsEnum)
        {
            if (value is string stringValue)
            {
                return Enum.Parse(targetType, stringValue);
            }

            return Enum.ToObject(targetType, value);
        }

        if (targetType == typeof(int))
        {
            return Convert.ToInt32(value);
        }

        if (targetType == typeof(float))
        {
            return Convert.ToSingle(value);
        }

        if (targetType == typeof(double))
        {
            return Convert.ToDouble(value);
        }

        if (targetType == typeof(bool))
        {
            return Convert.ToBoolean(value);
        }

        if (targetType == typeof(string))
        {
            return Convert.ToString(value);
        }

        return value;
    }

    private void EnsureDefaultComponentTypeNames()
    {
        if (componentTypeNamesToPersist == null)
        {
            componentTypeNamesToPersist = new List<string>();
        }

        AddDefaultComponentTypeName("PlayerStats");
        AddDefaultComponentTypeName("SkillProgressionManager");
    }

    private void AddDefaultComponentTypeName(string componentTypeName)
    {
        if (!componentTypeNamesToPersist.Contains(componentTypeName))
        {
            componentTypeNamesToPersist.Add(componentTypeName);
        }
    }
}