using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabAbility : PlayerAbility
{
    // Hugo added
    public Material objectPlacementMat;

    protected override void FindUIIndicator()
    {
        uiIndicator = GameObject.FindGameObjectWithTag("GrabIndicator").GetComponent<AbilityIndicator>();
    }

    protected override PlayerAbilityType GetAbilityType()
    {
        return PlayerAbilityType.Grab;
    }

    protected override void SetupUIIndicator()
    {
        base.SetupUIIndicator();

        SetCooldownAmount(0.0f);
    }
}