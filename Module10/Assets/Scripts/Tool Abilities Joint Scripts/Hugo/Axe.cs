using UnityEngine;

// Main author:         Hugo Bailey & Joe Allen
// Additional author:   N/A
// Description:         Axe tool used by player to chop trees
// Development window:  Prototype phase
// Inherits from:       HeldTool

public class Axe : HeldTool
{
    [Header("Axe")]
    [SerializeField] private SoundClass freezeAbilitySound; // Ref. to audio that plays when secondary ability is used
                     private Freezable frozenObject = null; // Ref. to object currently frozen by ability

    protected override void HitDestructibleObject(DestructableObject destructible, RaycastHit raycastHit)
    {
        // Joe code \/
        base.HitDestructibleObject(destructible, raycastHit);

        if (!(destructible is Tree))
        {
            // Play a quick chop animation when hitting a destructible, unless it's a tree because they have a custom axe animation
            gameObject.GetComponent<Animator>().SetTrigger("Chop");
            playerCameraShake.ShakeCameraForTime(0.3f, CameraShakeType.ReduceOverTime, 0.03f);

            // Play the use tool sound if one was set
            if (useToolSound != null)
            {
                AudioManager.Instance.PlaySoundEffect3D(useToolSound, raycastHit.point);
            }
        }
    }

    public override void StartSecondardAbility()
    {
        // Hugo code \/
        // Finds ref. to player camera
        GameObject playerCam = GameObject.FindGameObjectWithTag("MainCamera");

        // Check that the player cam isn't null, this can occur in certain cases when an alternate camera is being used (e.g. talking to an NPC)
        if (playerCam != null)
        {
            if (Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out toolRaycastHit, 4.0f))
            {
                // Gets Freezable component of object hit by raycast
                Freezable freeze = toolRaycastHit.transform.gameObject.GetComponent<Freezable>();

                if (freeze != null)
                {
                    // If object can be frozen, decrease food level by amount specified in base HeldTool class
                    playerStats.DecreaseFoodLevel(secondaryAbilityHunger);
                    frozenObject = freeze;
                    // Call Freeze() func. on object
                    frozenObject.Freeze();
                    // Play ability sound
                    if(freezeAbilitySound != null)
                    {
                        AudioManager.Instance.PlaySoundEffect2D(freezeAbilitySound);
                    }
                }
            }
        }

        base.StartSecondardAbility();
    }

    public override void EndSecondaryAbility()
    {
        // When secondary ability is ended & object has been frozen, un-freeze it
        if(frozenObject != null)
        {
            frozenObject.UnFreeze();
            frozenObject = null;
        }

        base.EndSecondaryAbility();
    }
}
