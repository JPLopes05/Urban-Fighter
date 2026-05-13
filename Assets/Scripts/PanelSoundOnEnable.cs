using UnityEngine;

public class PanelSoundOnEnable : MonoBehaviour
{
    public enum PanelSoundType
    {
        None,
        Pause,
        Victory,
        Defeat
    }

    public PanelSoundType soundType = PanelSoundType.None;
    public bool playOnlyOnce = false;

    private bool alreadyPlayed = false;

    void OnEnable()
    {
        if (playOnlyOnce && alreadyPlayed)
            return;

        if (AudioManager.Instance == null)
            return;

        switch (soundType)
        {
            case PanelSoundType.Pause:
                AudioManager.Instance.PlayUIPause();
                break;

            case PanelSoundType.Victory:
                AudioManager.Instance.PlayVictory();
                break;

            case PanelSoundType.Defeat:
                AudioManager.Instance.PlayDefeat();
                break;
        }

        alreadyPlayed = true;
    }
}