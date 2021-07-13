using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabAbility : PlayerAbility
{
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

        if(uiIndicator != null)
        {
            uiIndicator.HideKeyPrompt();
        }
    }
}