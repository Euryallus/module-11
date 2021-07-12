using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabAbility : PlayerAbility
{
    //protected override void SetupUIIndicator()
    //{
    //    base.SetupUIIndicator();

    //    uiIndicator.HideKeyPrompt();
    //}

    protected override void FindUIIndicator()
    {
        uiIndicator = GameObject.FindGameObjectWithTag("GrabIndicator").GetComponent<AbilityIndicator>();
    }
}
