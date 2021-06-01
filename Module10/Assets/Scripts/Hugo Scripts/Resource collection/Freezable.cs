using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Flags if an object with a rigidbody can be frozen by the Freeze ability (stops object moving)
// Development window:  Prototype phase
// Inherits from:       MonoBehaviour

public class Freezable : MonoBehaviour
{
    public Rigidbody attachedRB;                        // Ref. to own rigidbody
    public RigidbodyConstraints originalRBConstraints;  // Original rigidbody contraints on the object

    public void Freeze()
    {
        originalRBConstraints = attachedRB.constraints;             // Saves original contraints for use later
        attachedRB.constraints = RigidbodyConstraints.FreezeAll;    // Freezes all movement / rotation
    }

    public void UnFreeze()
    {
        attachedRB.constraints = originalRBConstraints;             // Restores contraints to default
    }
}
