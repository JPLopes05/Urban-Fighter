using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelStateManager : MonoBehaviour
{
    [Header("Painéis")]
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Textos da tela de fase concluída")]
    [SerializeField] private TMP_Text levelCompleteTitleText;
    [SerializeField] private TMP_Text levelCompleteDescriptionText;

    [Header("Textos da tela de derrota")]
    [SerializeField] private TMP_Text gameOverTitleText;
    [SerializeField] private TMP_Text gameOverDescriptionText;

    [Header("Configuração da fase atual")]
    [SerializeField] private string currentLevelName = "Fase 1";

    [Header("Fluxo entre cenas")]
    [SerializeField] private string nextSceneName = "";
    [SerializeField] private GameObject continueButtonObject;
    [SerializeField] private string mainMenuSceneName = "Menu";

    [Header("Progresso da campanha")]
    [SerializeField] private bool saveProgressBeforeContinue = true;

    private bool levelEnded = false;

    private void Awake()
    {
        Time.timeScale = 1f;
        HideAllPanels();
        ConfigureContinueButton();
    }

    public void CompleteLevel()
    {
        if (levelEnded)
        {
            return;
        }

        levelEnded = true;

        if (levelCompleteTitleText != null)
        {
            levelCompleteTitleText.text = currentLevelName + " concluída!";
        }

        if (levelCompleteDescriptionText != null)
        {
            if (HasNextScene())
            {
                levelCompleteDescriptionText.text = "Você derrotou o boss e concluiu a fase. Clique em Continuar para avançar.";
            }
            else
            {
                levelCompleteDescriptionText.text = "Você derrotou o boss e concluiu a fase.";
            }
        }

        ConfigureContinueButton();

        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
        }

        Time.timeScale = 0f;

        Debug.Log(currentLevelName + " concluída.");
    }

    public void GameOver()
    {
        if (levelEnded)
        {
            return;
        }

        levelEnded = true;

        if (gameOverTitleText != null)
        {
            gameOverTitleText.text = "Derrota";
        }

        if (gameOverDescriptionText != null)
        {
            gameOverDescriptionText.text = "O jogador foi derrotado. Tente novamente.";
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        Time.timeScale = 0f;

        Debug.Log("Game Over.");
    }

    public void RestartCurrentLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ContinueToNextLevel()
    {
        if (!levelEnded)
        {
            Debug.LogWarning("Não é possível continuar antes da fase terminar.");
            return;
        }

        if (!HasNextScene())
        {
            Debug.LogWarning("Nenhuma próxima cena foi configurada no LevelStateManager.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            Debug.LogError("A próxima cena '" + nextSceneName + "' não está disponível. Confira o nome da cena e o Build Settings.");
            return;
        }

        if (saveProgressBeforeContinue)
        {
            PlayerProgressPersistence.SaveCurrentPlayerProgress();
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(nextSceneName);
    }

    public void ReturnToMainMenu()
    {
        if (string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            Debug.LogWarning("Nome da cena de menu não foi configurado.");
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

    public bool IsLevelEnded()
    {
        return levelEnded;
    }

    private bool HasNextScene()
    {
        return !string.IsNullOrWhiteSpace(nextSceneName);
    }

    private void ConfigureContinueButton()
    {
        if (continueButtonObject != null)
        {
            continueButtonObject.SetActive(HasNextScene());
        }
    }

    private void HideAllPanels()
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }
}