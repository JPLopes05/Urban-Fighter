using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalScreenManager : MonoBehaviour
{
    [Header("Cenas")]
    [SerializeField] private string mainMenuSceneName = "Menu";

    private void Awake()
    {
        Time.timeScale = 1f;
    }

    public void BackToMainMenu()
    {
        if (string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            Debug.LogWarning("Nome da cena de menu não foi configurado no FinalScreenManager.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
        {
            Debug.LogError("A cena de menu '" + mainMenuSceneName + "' não está disponível. Confira o nome da cena e o Build Settings.");
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
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