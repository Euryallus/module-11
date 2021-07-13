using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityIndicator : MonoBehaviour
{
    [SerializeField] private Image              chargeImage;
    [SerializeField] private Image              cooldownImage;
    [SerializeField] private GameObject         keyPromptBackground;
    [SerializeField] private TextMeshProUGUI    keyPromptText;
    [SerializeField] private GameObject         upgradeSliderBg;
    [SerializeField] private Slider             upgradeSlider;

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

    public void SetUpgradeLevel(float currentLevel, float maxLevel)
    {
        upgradeSlider.value = currentLevel / maxLevel;
    }

    public void HideKeyPrompt()
    {
        keyPromptBackground.SetActive(false);
    }

    public void HideUpgradeSlider()
    {
        upgradeSliderBg.SetActive(false);
        upgradeSlider.gameObject.SetActive(false);
    }
}
