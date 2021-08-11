using UnityEngine;

// ||=======================================================================||
// || GliderAbility: A child class of PlayerAbility that handles the        ||
// ||    glider ability specifically.                                       ||
// ||=======================================================================||
// || Used on prefab: Hugo/Player                                           ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the production phase (Module 11).                                 ||
// ||=======================================================================||

public class GliderAbility : PlayerAbility
{
    protected override void FindUIIndicator()
    {
        // Finds the UI indicator for the glider ability
        uiIndicator = GameObject.FindGameObjectWithTag("GliderIndicator").GetComponent<AbilityIndicator>();
    }

    protected override PlayerAbilityType GetAbilityType()
    {
        return PlayerAbilityType.Glider;
    }
}