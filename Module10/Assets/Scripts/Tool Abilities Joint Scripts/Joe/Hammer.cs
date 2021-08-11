using UnityEngine;

// ||=======================================================================||
// || Hammer: Held tool used by the player to collect certain resources.    ||
// ||=======================================================================||
// || Used on all prefabs in: HeldItems/Hammer                              ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || originally for the prototype phase.                                   ||
// ||                                                                       ||
// || Changes made during the production phase (Module 11):                 ||
// ||                                                                       ||
// || - Removed the launch functionality as this ability is now entirely    ||
// ||    separate from the hammer tool.                                     ||
// ||=======================================================================||

[RequireComponent(typeof(Animator))]
public class Hammer : HeldTool
{
    private Animator animator; // Animator for tool swing animation

    protected override void Awake()
    {
        base.Awake();

        animator = GetComponent<Animator>();
    }

    protected override void HitDestructibleObject(DestructableObject destructible, RaycastHit raycastHit)
    {
        base.HitDestructibleObject(destructible, raycastHit);

        // Show the swing animation when something is hit by the hammer
        animator.SetTrigger("Swing");
    }
}