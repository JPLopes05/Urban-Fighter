using UnityEngine;
using System.Collections;

public class SkillProgressionManager : MonoBehaviour
{
    [Header("Referências")]
    public PlayerStats playerStats;
    public PlayerSkills playerSkills;
    public SkillChoiceUIController skillChoiceUIController;

    [Header("Escolha inicial")]
    public bool initialChoiceMade = false;
    public int chosenFirstSkill = 0; // 1 ou 2

    [Header("Controle de marcos")]
    public bool level3ChoiceTriggered = false;
    public bool level5OtherSkillUnlocked = false;
    public bool level7FirstChosenSkillLevel2Applied = false;
    public bool level9SecondSkillLevel2Applied = false;
    public bool level10UltimateLevel2Applied = false;
    public bool level12Skill3Unlocked = false;
    public bool level14Skill3Level2Applied = false;
    public bool level17FirstChosenSkillLevel3Applied = false;
    public bool level20SecondSkillLevel3Applied = false;
    public bool level22Skill3Level3Applied = false;
    public bool level25UltimateLevel3Applied = false;

    IEnumerator Start()
    {
        ResolveReferences();

        if (skillChoiceUIController != null)
        {
            skillChoiceUIController.Initialize(this);
        }

        // Espera a persistência aplicar os dados salvos antes de sincronizar skills.
        yield return null;
        yield return null;

        RefreshProgressionAfterLoad();
    }

    void ResolveReferences()
    {
        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();

        if (playerSkills == null)
            playerSkills = GetComponent<PlayerSkills>();
    }

    public void RefreshProgressionAfterLoad()
    {
        ResolveReferences();

        if (playerStats == null || playerSkills == null)
            return;

        HandleLevelProgression(playerStats.currentLevel);
    }

    public void HandleLevelProgression(int currentLevel)
    {
        ResolveReferences();

        if (playerStats == null || playerSkills == null)
            return;

        currentLevel = Mathf.Clamp(currentLevel, 1, playerStats.maxLevel);

        if (currentLevel >= 3 && !initialChoiceMade)
        {
            if (!level3ChoiceTriggered)
            {
                level3ChoiceTriggered = true;

                if (skillChoiceUIController != null)
                {
                    skillChoiceUIController.ShowChoicePanel();
                }
                else
                {
                    Debug.LogWarning("SkillChoiceUIController não configurado. Skill 1 será liberada por padrão.");
                    ResolveInitialSkillChoice(1);
                }
            }

            return;
        }

        ApplyAutomaticMilestones();
    }

    public void ResolveInitialSkillChoice(int chosenSkillIndex)
    {
        ResolveReferences();

        if (playerSkills == null)
            return;

        if (chosenSkillIndex != 1 && chosenSkillIndex != 2)
        {
            chosenSkillIndex = 1;
        }

        initialChoiceMade = true;
        level3ChoiceTriggered = true;
        chosenFirstSkill = chosenSkillIndex;

        playerSkills.UnlockSkill(chosenSkillIndex);

        Debug.Log("Escolha inicial feita: Skill " + chosenSkillIndex + " foi liberada primeiro.");

        ApplyAutomaticMilestones();
    }

    void ApplyAutomaticMilestones()
    {
        ResolveReferences();

        if (playerStats == null || playerSkills == null)
            return;

        if (!initialChoiceMade)
            return;

        if (chosenFirstSkill != 1 && chosenFirstSkill != 2)
        {
            chosenFirstSkill = 1;
        }

        int level = Mathf.Clamp(playerStats.currentLevel, 1, playerStats.maxLevel);

        int firstChosenSkill = chosenFirstSkill;
        int secondChosenSkill = chosenFirstSkill == 1 ? 2 : 1;

        // Nível 3: garante que a primeira skill escolhida esteja desbloqueada.
        if (level >= 3)
        {
            playerSkills.UnlockSkill(firstChosenSkill);
            level3ChoiceTriggered = true;
        }

        // Nível 5: libera automaticamente a outra skill.
        if (level >= 5)
        {
            playerSkills.UnlockSkill(secondChosenSkill);
            level5OtherSkillUnlocked = true;
        }

        // Nível 7: a skill escolhida primeiro vai para nível 2.
        if (level >= 7)
        {
            UpgradeSkillToTargetLevel(firstChosenSkill, 2);
            level7FirstChosenSkillLevel2Applied = true;
        }

        // Nível 9: a outra skill vai para nível 2.
        if (level >= 9)
        {
            UpgradeSkillToTargetLevel(secondChosenSkill, 2);
            level9SecondSkillLevel2Applied = true;
        }

        // Nível 10: Ultimate nível 2.
        if (level >= 10)
        {
            playerStats.EnsureUltimateTierAtLeast(2);
            level10UltimateLevel2Applied = true;
        }

        // Nível 12: Skill 3 desbloqueada.
        if (level >= 12)
        {
            playerSkills.UnlockSkill(3);
            level12Skill3Unlocked = true;
        }

        // Nível 14: Skill 3 nível 2.
        if (level >= 14)
        {
            UpgradeSkillToTargetLevel(3, 2);
            level14Skill3Level2Applied = true;
        }

        // Nível 17: a skill escolhida primeiro vai para nível 3.
        if (level >= 17)
        {
            UpgradeSkillToTargetLevel(firstChosenSkill, 3);
            level17FirstChosenSkillLevel3Applied = true;
        }

        // Nível 20: a outra skill vai para nível 3.
        if (level >= 20)
        {
            UpgradeSkillToTargetLevel(secondChosenSkill, 3);
            level20SecondSkillLevel3Applied = true;
        }

        // Nível 22: Skill 3 nível 3.
        if (level >= 22)
        {
            UpgradeSkillToTargetLevel(3, 3);
            level22Skill3Level3Applied = true;
        }

        // Nível 25: Ultimate nível 3.
        if (level >= 25)
        {
            playerStats.EnsureUltimateTierAtLeast(3);
            level25UltimateLevel3Applied = true;
        }

        Debug.Log("Progressão de skills sincronizada para o nível " + level + ".");
    }

    void UpgradeSkillToTargetLevel(int skillIndex, int targetLevel)
    {
        if (playerSkills == null)
            return;

        if (!playerSkills.IsSkillUnlocked(skillIndex))
        {
            playerSkills.UnlockSkill(skillIndex);
        }

        while (playerSkills.GetSkillLevel(skillIndex) < targetLevel)
        {
            playerSkills.UpgradeSkill(skillIndex);
        }
    }
}