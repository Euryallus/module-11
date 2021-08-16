using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Anim event ref. - allows enemy to take damage at apex of swing rather than at start of animation
// Development window:  Prototype phase & production phase
// Inherits from:       MonoBehaviour
public class SwordSwingAnimEvent : MonoBehaviour
{
    [SerializeField] private MeleeWeapon swordRef;

    public void SwingStart()
    {
        swordRef.Swing();
    }
 }
