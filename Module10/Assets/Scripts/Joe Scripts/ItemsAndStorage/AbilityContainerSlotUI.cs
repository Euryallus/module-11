using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityContainerSlotUI : ContainerSlotUI
{
    [Header("Ability Container Slot UI")]
    [SerializeField] private Slider             upgradeLevelSlider;
    [SerializeField] private PlayerAbilityType  abilityType;

    private PlayerAbility linkedPlayerAbility;

    public override void LinkToContainerSlot(ContainerSlot slot)
    {
        base.LinkToContainerSlot(slot);

        UpdateUI();
    }

    protected override void DoExtraUIUpdates(Item itemInSlot, int stackSize)
    {
        if(stackSize > 0 && itemInSlot != null)
        {
            if(linkedPlayerAbility == null)
            {
                linkedPlayerAbility = GetLinkedPlayerAbility();
            }

            CustomFloatProperty itemUpgradeLevelProperty = itemInSlot.GetCustomFloatPropertyWithName("upgradeLevel", true);

            if(itemUpgradeLevelProperty != null)
            {
                float currentUpgradeLevel = itemUpgradeLevelProperty.Value;
                float maxUpgradeLevel = linkedPlayerAbility.MaxUpgradeLevel;

                upgradeLevelSlider.value = currentUpgradeLevel / maxUpgradeLevel;
            }
            else
            {
                upgradeLevelSlider.value = 1.0f;
            }

            upgradeLevelSlider.gameObject.SetActive(true);
        }
        else
        {
            upgradeLevelSlider.gameObject.SetActive(false);
        }
    }

    private PlayerAbility GetLinkedPlayerAbility()
    {
        GameObject playerGameObj = GameObject.FindGameObjectWithTag("Player");

        return abilityType switch
        {
            PlayerAbilityType.Launch            => playerGameObj.GetComponent<LaunchAbility>(),
            PlayerAbilityType.Freeze            => playerGameObj.GetComponent<FreezeAbility>(),
            PlayerAbilityType.Slam_Levitate     => playerGameObj.GetComponent<SlamAbility>(),
            PlayerAbilityType.Grab              => playerGameObj.GetComponent<GrabAbility>(),

            _ => null,
        };
    }
}
