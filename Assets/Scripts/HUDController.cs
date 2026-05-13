using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("Referência do jogador")]
    public PlayerStats playerStats;

    [Header("Barras")]
    public Image hpFill;
    public Image xpFill;
    public Image ultFill;

    [Header("Textos")]
    public TMP_Text levelText;

    void Update()
    {
        if (playerStats == null) return;

        hpFill.fillAmount = playerStats.currentHP / playerStats.maxHP;
        xpFill.fillAmount = playerStats.currentXP / playerStats.xpToNextLevel;
        ultFill.fillAmount = playerStats.currentULT / playerStats.maxULT;

        if (levelText != null)
        {
            levelText.text = "LV. " + playerStats.currentLevel;
        }
    }
}