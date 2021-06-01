using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Adapted version of Enemy1Anim - used to test enemy animations & visuals
// Development window:  Prototype phase
// Inherits from:       Enemy1Anim

public class Enemy1MinionAnim : Enemy1Anim
{
    //Operates the same as Enemy1Anim currently, but since HasSplit is set to true immediately it just melee attacks
    public override void Start()
    {
        HasSplit = true;
        base.Start();
    }
}
