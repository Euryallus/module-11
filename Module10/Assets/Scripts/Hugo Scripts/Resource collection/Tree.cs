using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Adaptation of CollectableResource - Tree behaves differently to rock & crystal etc. when destroyed (top falls over)
// Development window:  Prototype phase
// Inherits from:       CollectableResource

public class Tree : CollectableResource
{
    [SerializeField] private Rigidbody treeTop; // Ref. to "treetop"

    public override void TryToDestroy()
    {
        // Runs base function
        base.TryToDestroy();

        if(toBeDestroyed)
        {
            // Resets Velocity of treetop, calculates direction force from axe is added in
            treeTop.velocity = Vector3.zero;
            Vector3 forceDir = (transform.position - GameObject.FindGameObjectWithTag("Player").transform.position) * 300;
            
            // Adds force to treetop (prompts it to fall over)
            treeTop.AddForce(forceDir);
        }
    }
}
