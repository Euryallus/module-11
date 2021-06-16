using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlamAbility : PlayerAbility
{
    protected override void FindUIIndicator()
    {
        uiIndicator = GameObject.FindGameObjectWithTag("SlamIndicator").GetComponent<AbilityIndicator>();
    }
}