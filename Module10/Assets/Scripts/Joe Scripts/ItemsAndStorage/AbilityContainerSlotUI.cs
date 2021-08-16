using UnityEngine;
using UnityEngine.UI;

// ||=======================================================================||
// || AbilityContainerSlotUI: A ContainerSlotUI used specifically for       ||
// ||   displaying player ability items that show an extra visual slider    ||
// ||   on the slot that displays the ability's upgrade level               ||
// ||=======================================================================||
// || Used on prefab: Joe/UI/Inventory/DropItemButton                       ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the production phase (Module 11).                                 ||
// ||=======================================================================||

public class AbilityContainerSlotUI : ContainerSlotUI
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [Header("Ability Container Slot UI")]

    [SerializeField] private Slider             upgradeLevelSlider; // The slider that shows the upgrade level of the ability item in the slot (if there is one)
    [SerializeField] private PlayerAbilityType  abilityType;        // The type of ability item this slot will contain

    #endregion

    private PlayerAbility linkedPlayerAbility;  // The player ability linked to the ability type this slot holds

    public override void LinkToContainerSlot(ContainerSlot slot)
    {
        base.LinkToContainerSlot(slot);

        // Update slot UI when the slots are first linked to initialise the upgradeLevelSlider
        UpdateUI();
    }

    protected override void DoExtraUIUpdates(Item itemInSlot, int stackSize)
    {
        // Called after the standard UI that all ContainerSlotUI objects have is updated

        if(stackSize > 0 && itemInSlot != null)
        {
            // This slot contains an item

            // Get the 'upgradeLevel' property from the item to check how much the ability has been upgraded
            CustomFloatProperty itemUpgradeLevelProperty = itemInSlot.GetCustomFloatPropertyWithName("upgradeLevel", true);

            if(itemUpgradeLevelProperty != null)
            {
                // The item in the slot can be upgraded - we need to check how close it is to being fully
                //   upgraded so that this information can be displayed using the upgradeLevelSlider

                // Get the PlayerAbility script linked to the ability type that this slot contains
                if (linkedPlayerAbility == null)
                {
                    linkedPlayerAbility = GetLinkedPlayerAbility();
                }

                // Update the upgradeLevelSlider's value by getting the ability's current upgrade level
                //   as a fraction of the maximum possible upgrade level for the ability

                float currentUpgradeLevel = itemUpgradeLevelProperty.Value;
                float maxUpgradeLevel = linkedPlayerAbility.MaxUpgradeLevel;

                upgradeLevelSlider.value = currentUpgradeLevel / maxUpgradeLevel;
            }
            else
            {
                // There is no upgrade property, meaning the item cannot be upgraded
                //   so it's already at the max possible level - fill the upgrade slider
                upgradeLevelSlider.value = 1.0f;
            }

            upgradeLevelSlider.gameObject.SetActive(true);
        }
        else
        {
            // There is no item in the slot, hide the slider that shows an ability's upgrade level
            upgradeLevelSlider.gameObject.SetActive(false);
        }
    }

    private PlayerAbility GetLinkedPlayerAbility()
    {
        // Returns the ability script linked to the type of ability this slot will contain
        //   All ability scripts are on the player GameObject

        return abilityType switch
        {
            PlayerAbilityType.Launch        => PlayerInstance.ActivePlayer.GetComponent<LaunchAbility>(),
            PlayerAbilityType.Freeze        => PlayerInstance.ActivePlayer.GetComponent<FreezeAbility>(),
            PlayerAbilityType.Slam_Levitate => PlayerInstance.ActivePlayer.GetComponent<SlamAbility>(),
            PlayerAbilityType.Grab          => PlayerInstance.ActivePlayer.GetComponent<GrabAbility>(),

            _ => null,
        };
    }
}
