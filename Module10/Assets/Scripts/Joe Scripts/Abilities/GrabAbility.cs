using UnityEngine;

// ||=======================================================================||
// || GrabAbility: A child class of PlayerAbility that handles the grab     ||
// ||    ability specifically.                                              ||
// ||=======================================================================||
// || Used on prefab: Hugo/Player                                           ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the production phase (Module 11).                                 ||
// ||=======================================================================||

public class GrabAbility : PlayerAbility
{
    // Hugo added - transparent material used for movable objects when picked up
    public Material objectPlacementMat;

    protected override void SetupUIIndicator()
    {
        base.SetupUIIndicator();

        // By default, the grab ability cooldown is not complete, as the cooldown
        //   value is only set to 1.0 when hovering over a movable object
        SetCooldownAmount(0.0f);
    }

    protected override void FindUIIndicator()
    {
        // Finds the UI indicator for the grab ability
        uiIndicator = GameObject.FindGameObjectWithTag("GrabIndicator").GetComponent<AbilityIndicator>();
    }

    protected override PlayerAbilityType GetAbilityType()
    {
        return PlayerAbilityType.Grab;
    }
}