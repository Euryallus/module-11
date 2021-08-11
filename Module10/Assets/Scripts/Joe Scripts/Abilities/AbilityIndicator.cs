using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ||=======================================================================||
// || AbilityIndicator: A UI element that appears when the player has       ||
// ||   a certain ability.                                                  ||
// ||=======================================================================||
// || Used on prefab: Joe/UI/AbilityIndicators/AbilityIndicator             ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the production phase (Module 11).                                 ||
// ||=======================================================================||

public class AbilityIndicator : MonoBehaviour
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private Image              chargeImage;            // The image used to show how charged the ability is
    [SerializeField] private Image              cooldownImage;          // The image used to show the cooldown value of the ability
    [SerializeField] private GameObject         keyPromptBackground;    // Background for the key prompt text
    [SerializeField] private TextMeshProUGUI    keyPromptText;          // Text showing which key should be pressed to activate the ability
    [SerializeField] private GameObject         upgradeSliderBg;        // Background for the slider below, shows if the ability can be upgraded
    [SerializeField] private Slider             upgradeSlider;          // Slider that shows how much the ability has been upgraded

    #endregion

    public void SetChargeAmount(float charge)
    {
        // Sets the charge image's fill amount based on charge value to act as a sort of progress bar
        chargeImage.fillAmount = charge;
    }

    public void SetCooldownAmount(float cooldown)
    {
        // Sets the cooldown image's fill amount based on cooldown value to act as a sort of progress bar
        cooldownImage.fillAmount = cooldown;

        if(cooldown >= 1.0f && chargeImage.fillAmount == 0.0f)
        {
            // If cooldown is complete and the ability is not being charged, re-show the key prompt
            keyPromptBackground.SetActive(true);
        }
        else
        {
            // Otherwise hide the key prompt
            keyPromptBackground.SetActive(false);
        }
    }

    public void SetKeyPromptText(string text)
    {
        keyPromptText.text = text;
    }

    public void SetUpgradeLevel(float currentLevel, float maxLevel)
    {
        // Sets the upgrade slider value using the current upgrade level as a fraction of the max possible level
        upgradeSlider.value = currentLevel / maxLevel;
    }

    public void HideUpgradeSlider()
    {
        // Hides the upgrade slider and its background
        upgradeSliderBg.SetActive(false);
        upgradeSlider.gameObject.SetActive(false);
    }
}