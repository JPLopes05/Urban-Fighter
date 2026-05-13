using System;
using System.Collections.Generic;
using UnityEngine;

public static class CampaignProgressStore
{
    public static bool HasProgress { get; private set; }
    public static string LastSavedFromScene { get; private set; }

    private static readonly Dictionary<string, ComponentProgressSnapshot> snapshotsByComponent =
        new Dictionary<string, ComponentProgressSnapshot>();

    public static void SaveProgress(string sourceSceneName, List<ComponentProgressSnapshot> snapshots)
    {
        snapshotsByComponent.Clear();

        foreach (ComponentProgressSnapshot snapshot in snapshots)
        {
            if (snapshot == null || string.IsNullOrWhiteSpace(snapshot.componentTypeName))
            {
                continue;
            }

            snapshotsByComponent[snapshot.componentTypeName] = snapshot.Clone();
        }

        HasProgress = snapshotsByComponent.Count > 0;
        LastSavedFromScene = sourceSceneName;

        Debug.Log("Progresso da campanha salvo a partir da cena: " + sourceSceneName);
    }

    public static bool TryGetSnapshot(string componentTypeName, out ComponentProgressSnapshot snapshot)
    {
        if (snapshotsByComponent.TryGetValue(componentTypeName, out ComponentProgressSnapshot storedSnapshot))
        {
            snapshot = storedSnapshot.Clone();
            return true;
        }

        snapshot = null;
        return false;
    }

    public static void ResetProgress()
    {
        snapshotsByComponent.Clear();
        HasProgress = false;
        LastSavedFromScene = string.Empty;

        Debug.Log("Progresso da campanha reiniciado.");
    }
}

[Serializable]
public class ComponentProgressSnapshot
{
    public string componentTypeName;
    public Dictionary<string, object> fieldValues = new Dictionary<string, object>();

    public ComponentProgressSnapshot(string componentTypeName)
    {
        this.componentTypeName = componentTypeName;
    }

    public ComponentProgressSnapshot Clone()
    {
        ComponentProgressSnapshot clone = new ComponentProgressSnapshot(componentTypeName);

        foreach (KeyValuePair<string, object> pair in fieldValues)
        {
            clone.fieldValues[pair.Key] = pair.Value;
        }

        return clone;
    }
}