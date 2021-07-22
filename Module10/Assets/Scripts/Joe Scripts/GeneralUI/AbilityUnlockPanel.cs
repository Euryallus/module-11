using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class AbilityUnlockPanel : MonoBehaviour
{
    [SerializeField] private Image              abilityImage;
    [SerializeField] private TextMeshProUGUI    abilityText;
    [SerializeField] private TextMeshProUGUI    descriptionText;
    [SerializeField] private List<Sprite>       abilitySprites;
    [SerializeField] private Transform          propertyTextParent;
    [SerializeField] private GameObject         propertyTextPrefab;

    public event Action ContinueButtonPressEvent;

    public void Setup(Item abilityItem, PlayerAbilityType abilityType, int unlockLevel)
    {
        abilityImage.sprite = abilitySprites[(int)abilityType];

        string abilityBaseName = abilityItem.UIName.Replace(" Ability", "");

        if (unlockLevel > 0)
        {
            abilityText.text = abilityBaseName + ": Level " + unlockLevel;
        }
        else
        {
            abilityText.text = abilityBaseName;
        }

        descriptionText.text = abilityItem.UIDescription;

        foreach (CustomFloatProperty property in abilityItem.CustomFloatProperties)
        {
            if (property.Name != "upgradeLevel")
            {
                AddPropertyText(property.GetDisplayText());
            }
        }

        foreach (CustomStringProperty property in abilityItem.CustomStringProperties)
        {
            AddPropertyText(property.GetDisplayText());
        }
    }

    private void AddPropertyText(string text)
    {
        GameObject propertyTextGameObj = Instantiate(propertyTextPrefab, propertyTextParent);
        propertyTextGameObj.GetComponent<TextMeshProUGUI>().text = text;
    }

    public void ContinueButton()
    {
        ContinueButtonPressEvent.Invoke();

        AudioManager.Instance.PlaySoundEffect2D("buttonClickMain2");
    }
}