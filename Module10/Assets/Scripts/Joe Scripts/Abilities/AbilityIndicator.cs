using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityIndicator : MonoBehaviour
{
    [SerializeField] private Image              chargeImage;
    [SerializeField] private Image              cooldownImage;
    [SerializeField] private GameObject         keyPromptBackground;
    [SerializeField] private TextMeshProUGUI    keyPromptText;

    public void SetChargeAmount(float charge)
    {
        chargeImage.fillAmount = charge;
    }

    public void SetCooldownAmount(float cooldown)
    {
        cooldownImage.fillAmount = cooldown;
    }

    public void SetKeyPromptText(string text)
    {
        keyPromptText.text = text;
    }

    public void HideKeyPrompt()
    {
        keyPromptBackground.SetActive(false);
    }
}
