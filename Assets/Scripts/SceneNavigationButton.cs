using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Button))]
public class SceneNavigationButton : MonoBehaviour
{
    [Header("Cena de destino")]
    public string targetSceneName = "Menu";

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
    }

    void OnEnable()
    {
        if (button != null)
        {
            button.onClick.AddListener(LoadTargetScene);
        }
    }

    void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(LoadTargetScene);
        }
    }

    public void LoadTargetScene()
    {
        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            Debug.LogWarning("SceneNavigationButton está sem targetSceneName configurado.");
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(targetSceneName);
    }
}