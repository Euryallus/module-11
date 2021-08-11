using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

// ||=======================================================================||
// || AbilityUnlockPanel: UI panel shown when a new ability is unlocked or  ||
// ||   an existing one is upgraded.                                        ||
// ||=======================================================================||
// || Used on prefab: Joe/Environment/Doors/Door                            ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the production phase (Module 11).                                 ||
// ||=======================================================================||

public class AbilityUnlockPanel : MonoBehaviour
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private Image              abilityImage;       // Image showing an icon that corresponds to the ability being unlocked
    [SerializeField] private TextMeshProUGUI    abilityText;        // Text showing the name of the unlocked ability
    [SerializeField] private TextMeshProUGUI    descriptionText;    // Text showing a description of the unlocked ability
    [SerializeField] private List<Sprite>       abilitySprites;     // All possible ability sprites that can be shown on the abilityImage, one for each AbilityType
    [SerializeField] private Transform          propertyTextParent; // Parent for instantiating property text
    [SerializeField] private GameObject         propertyTextPrefab; // Property text that can be instantiated to show extra info about the ability

    #endregion

    public event Action ContinueButtonPressEvent;   // Event triggered when the continue button is pressed
    public event Action PanelAnimationDoneEvent;    // Event triggered when the panel has completed its entrance animation

    public void Setup(Item abilityItem, PlayerAbilityType abilityType, int unlockLevel)
    {
        // Set the abilityImage's sprite based on the acquired ability type
        abilityImage.sprite = abilitySprites[(int)abilityType];

        // By default, the ability's name will be shown with the word 'Ability' removed since it's redundant
        string abilityBaseName = abilityItem.UIName.Replace(" Ability", "");

        if (unlockLevel > 0)
        {
            // The player is unlocking an upgrade, also display the unlock level
            abilityText.text = abilityBaseName + ": Level " + unlockLevel;
        }
        else
        {
            // No upgrade, just show the ability name
            abilityText.text = abilityBaseName;
        }

        // Set the description text to the item's UIDescription
        descriptionText.text = abilityItem.UIDescription;

        foreach (CustomFloatProperty property in abilityItem.CustomFloatProperties)
        {
            // Add some extra text displaying any custom float properties that are not the upgrade level (which is already shown, see above)
            if (property.Name != "upgradeLevel")
            {
                AddPropertyText(property.GetDisplayText());
            }
        }

        foreach (CustomStringProperty property in abilityItem.CustomStringProperties)
        {
            // Add some extra text displaying any custom string properties on the ability item
            AddPropertyText(property.GetDisplayText());
        }
    }

    private void AddPropertyText(string text)
    {
        // Add some extra text to the panel and set it to display the given value

        GameObject propertyTextGameObj = Instantiate(propertyTextPrefab, propertyTextParent);
        propertyTextGameObj.GetComponent<TextMeshProUGUI>().text = text;
    }

    // Called by an animation event when the panel completed its entrance animation
    private void PanelAnimationDone()
    {
        PanelAnimationDoneEvent?.Invoke();
    }

    // Called when the continue button is pressed
    public void ContinueButton()
    {
        // Trigger the ContinueButtonPressEvent and play a sound

        ContinueButtonPressEvent?.Invoke();

        AudioManager.Instance.PlaySoundEffect2D("buttonClickMain2", true);
    }
}