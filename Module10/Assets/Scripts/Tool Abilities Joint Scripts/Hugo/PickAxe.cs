using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Pickaxe used to destroy stones & pick up objects
// Development window:  Prototype phase
// Inherits from:       HeldTool

public class PickAxe : HeldTool
{
    [Header("Pickaxe")]
    [SerializeField] private SoundClass pickUpAbilitySound; // Ref. to sound that plays when secondary ability is used
    MovableObject heldObj = null;                           // Object currently being held (is NULL when none is held)

    protected override void HitDestructibleObject(DestructableObject destructible, RaycastHit raycastHit)
    {
        base.HitDestructibleObject(destructible, raycastHit);

        // Plays tool animation
        gameObject.GetComponent<Animator>().SetBool("Swing", true);
    }

    public override void StartSecondardAbility()
    {
        // Gets ref. to player camera
        GameObject playerCam = GameObject.FindGameObjectWithTag("MainCamera");

        //Check that the player cam isn't null, this can occur in certain cases when an alternate camera is being used (e.g. talking to an NPC)
        if(playerCam != null)
        {
            if (Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out toolRaycastHit, 4.0f))
            {
                // Gets gameObject ref. to object hit
                MovableObject moveObj = toolRaycastHit.transform.gameObject.GetComponent<MovableObject>();

                if (moveObj != null && !moveObj.isHeld && heldObj == null)
                {
                    // If player is able to pick up item (isn't hlding anything already), decrease food & "pick up" object
                    playerStats.DecreaseFoodLevel(secondaryAbilityHunger);

                    moveObj.PickUp(GameObject.FindGameObjectWithTag("PlayerHand").transform);
                    //Saves held object as object hit by raycast
                    heldObj = moveObj;

                    // Plays ability sound if one was given
                    if(pickUpAbilitySound != null)
                    {
                        AudioManager.Instance.PlaySoundEffect2D(pickUpAbilitySound);
                    }
                }
            }
        }

        base.StartSecondardAbility();
    }

    public override void EndSecondaryAbility()
    {
        // If secondary ability is ended & player is holding an object, put it down
        if(heldObj != null)
        {
            heldObj.DropObject(transform.forward);
            heldObj = null;
        }

        base.EndSecondaryAbility();
    }

    public void StopSwing()
    {
        // Stops swing animation
        gameObject.GetComponent<Animator>().SetBool("Swing", false);
    }
}
