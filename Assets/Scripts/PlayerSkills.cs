using UnityEngine;

public class PlayerSkills : MonoBehaviour
{
    [System.Serializable]
    public class SkillData
    {
        public string skillName = "Nova Skill";
        public KeyCode activationKey = KeyCode.None;
        public bool unlocked = false;
        public int currentLevel = 0;
        public int maxLevel = 3;
        public float cooldown = 5f;

        [HideInInspector] public float nextReadyTime = 0f;
    }

    [Header("Skills do Jogador")]
    public SkillData skill1 = new SkillData();
    public SkillData skill2 = new SkillData();
    public SkillData skill3 = new SkillData();

    private PlayerSkillEffects skillEffects;

    void Awake()
    {
        skillEffects = GetComponent<PlayerSkillEffects>();
    }

    void Reset()
    {
        skill1.skillName = "Skill 1";
        skill1.activationKey = KeyCode.G;
        skill1.cooldown = 5f;
        skill1.unlocked = false;
        skill1.currentLevel = 0;
        skill1.maxLevel = 3;

        skill2.skillName = "Skill 2";
        skill2.activationKey = KeyCode.H;
        skill2.cooldown = 6f;
        skill2.unlocked = false;
        skill2.currentLevel = 0;
        skill2.maxLevel = 3;

        skill3.skillName = "Skill 3";
        skill3.activationKey = KeyCode.J;
        skill3.cooldown = 15f;
        skill3.unlocked = false;
        skill3.currentLevel = 0;
        skill3.maxLevel = 3;
    }

    void Update()
    {
        HandleSkillInput(skill1, 1);
        HandleSkillInput(skill2, 2);
        HandleSkillInput(skill3, 3);
    }

    void HandleSkillInput(SkillData skill, int skillIndex)
    {
        if (!Input.GetKeyDown(skill.activationKey))
            return;

        if (!skill.unlocked)
        {
            Debug.Log(skill.skillName + " ainda está bloqueada.");
            return;
        }

        if (skill.currentLevel <= 0)
        {
            Debug.Log(skill.skillName + " está desbloqueada, mas sem nível configurado.");
            return;
        }

        if (Time.time < skill.nextReadyTime)
        {
            float timeRemaining = skill.nextReadyTime - Time.time;
            Debug.Log(skill.skillName + " em cooldown. Falta " + timeRemaining.ToString("F1") + "s");
            return;
        }

        bool activated = ActivateSkill(skill, skillIndex);

        if (activated)
        {
            skill.nextReadyTime = Time.time + skill.cooldown;
            Debug.Log(skill.skillName + " ativada no nível " + skill.currentLevel + ".");
        }
    }

    bool ActivateSkill(SkillData skill, int skillIndex)
    {
        switch (skillIndex)
        {
            case 1:
                if (skillEffects != null)
                {
                    return skillEffects.TryUseSkill1();
                }

                Debug.LogWarning("PlayerSkillEffects não encontrado no Player.");
                return false;

            case 2:
                if (skillEffects != null)
                {
                    return skillEffects.TryUseSkill2();
                }

                Debug.LogWarning("PlayerSkillEffects não encontrado no Player.");
                return false;

            case 3:
                if (skillEffects != null)
                {
                    return skillEffects.TryUseSkill3();
                }

                Debug.LogWarning("PlayerSkillEffects não encontrado no Player.");
                return false;
        }

        return false;
    }

    public void UnlockSkill(int skillIndex)
    {
        SkillData skill = GetSkillByIndex(skillIndex);
        if (skill == null) return;

        skill.unlocked = true;

        if (skill.currentLevel <= 0)
            skill.currentLevel = 1;

        Debug.Log(skill.skillName + " foi desbloqueada.");
    }

    public void UpgradeSkill(int skillIndex)
    {
        SkillData skill = GetSkillByIndex(skillIndex);
        if (skill == null) return;

        if (!skill.unlocked)
        {
            Debug.Log(skill.skillName + " ainda não foi desbloqueada.");
            return;
        }

        if (skill.currentLevel < skill.maxLevel)
        {
            skill.currentLevel++;
            Debug.Log(skill.skillName + " subiu para o nível " + skill.currentLevel + ".");
        }
        else
        {
            Debug.Log(skill.skillName + " já está no nível máximo.");
        }
    }

    public int GetSkillLevel(int skillIndex)
    {
        SkillData skill = GetSkillByIndex(skillIndex);
        if (skill == null) return 0;

        return skill.currentLevel;
    }

    public bool IsSkillUnlocked(int skillIndex)
    {
        SkillData skill = GetSkillByIndex(skillIndex);
        if (skill == null) return false;

        return skill.unlocked;
    }

    SkillData GetSkillByIndex(int skillIndex)
    {
        switch (skillIndex)
        {
            case 1: return skill1;
            case 2: return skill2;
            case 3: return skill3;
            default: return null;
        }
    }
}