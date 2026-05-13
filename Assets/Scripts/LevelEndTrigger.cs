using UnityEngine;

public class LevelEndTrigger : MonoBehaviour
{
    [Header("Condição para permitir finalizar")]
    [SerializeField] private GameObject requiredClearedObject;

    [Header("Gerenciador de estado")]
    [SerializeField] private LevelStateManager levelStateManager;

    [Header("Comportamento")]
    [SerializeField] private bool disableAfterComplete = true;

    private bool completed = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (completed)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (!CanCompleteLevel())
        {
            Debug.Log("A fase ainda não pode ser concluída. Derrote o boss primeiro.");
            return;
        }

        CompleteLevel();
    }

    private bool CanCompleteLevel()
    {
        if (requiredClearedObject == null)
        {
            return true;
        }

        return !requiredClearedObject.activeInHierarchy;
    }

    private void CompleteLevel()
    {
        completed = true;

        if (levelStateManager != null)
        {
            levelStateManager.CompleteLevel();
        }
        else
        {
            Debug.LogWarning("LevelEndTrigger está sem LevelStateManager configurado.");
        }

        if (disableAfterComplete)
        {
            gameObject.SetActive(false);
        }
    }
}