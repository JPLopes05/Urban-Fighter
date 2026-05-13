using UnityEngine;

public class SkillChoiceUIController : MonoBehaviour
{
    [Header("Referências")]
    public GameObject skillChoicePanel;

    private SkillProgressionManager progressionManager;

    public void Initialize(SkillProgressionManager manager)
    {
        progressionManager = manager;

        if (skillChoicePanel != null)
        {
            skillChoicePanel.SetActive(false);
        }
    }

    public void ShowChoicePanel()
    {
        if (skillChoicePanel != null)
        {
            skillChoicePanel.SetActive(true);
        }

        Time.timeScale = 0f;
        Debug.Log("Painel de escolha de skill exibido.");
    }

    public void HideChoicePanel()
    {
        if (skillChoicePanel != null)
        {
            skillChoicePanel.SetActive(false);
        }

        Time.timeScale = 1f;
    }

    public void ChooseSkill1First()
    {
        if (progressionManager != null)
        {
            progressionManager.ResolveInitialSkillChoice(1);
        }

        HideChoicePanel();
    }

    public void ChooseSkill2First()
    {
        if (progressionManager != null)
        {
            progressionManager.ResolveInitialSkillChoice(2);
        }

        HideChoicePanel();
    }
}