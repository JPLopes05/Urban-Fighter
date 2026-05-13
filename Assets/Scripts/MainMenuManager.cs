using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Cenas")]
    [SerializeField] private string firstLevelSceneName = "Fase1";

    [Header("Progresso da campanha")]
    [SerializeField] private bool resetCampaignProgressOnStartGame = true;

    private void Awake()
    {
        Time.timeScale = 1f;
    }

    public void StartGame()
    {
        if (string.IsNullOrWhiteSpace(firstLevelSceneName))
        {
            Debug.LogWarning("Nome da primeira fase não foi configurado no MainMenuManager.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(firstLevelSceneName))
        {
            Debug.LogError("A cena da primeira fase '" + firstLevelSceneName + "' não está disponível. Confira o nome da cena e o Build Settings.");
            return;
        }

        if (resetCampaignProgressOnStartGame)
        {
            CampaignProgressStore.ResetProgress();
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(firstLevelSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Saindo do jogo.");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}