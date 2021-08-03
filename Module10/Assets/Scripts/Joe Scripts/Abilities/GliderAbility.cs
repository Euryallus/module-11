using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GliderAbility : PlayerAbility
{
    protected override void FindUIIndicator()
    {
        uiIndicator = GameObject.FindGameObjectWithTag("GliderIndicator").GetComponent<AbilityIndicator>();
    }

    protected override PlayerAbilityType GetAbilityType()
    {
        return PlayerAbilityType.Glider;
    }
}
