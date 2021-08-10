using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Allows player to teleport between two places (e.g. basement to castle)
// Development window:  Production phase
// Inherits from:       MonoBehaviour

[RequireComponent(typeof(BoxCollider))]
public class TeleportPoint : MonoBehaviour
{
    [SerializeField]    private Transform teleportPoint;    // Point player will teleport to when they enter trigger
                        private BoxCollider triggerVol;     // Trigger collider volume
                        private Animator animator;          // Animator used to fade in / out
                        private GameObject player;          // Ref. to player

    private void Awake()
    {
        // Ensures trigger volume is set up correctly
        triggerVol = gameObject.GetComponent<BoxCollider>();
        triggerVol.isTrigger = true;

        // Assigns refs. to animator and player
        animator = gameObject.GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.transform.CompareTag("Player"))
        {
            // If player enters trigger box, prevent player from moving & play "fade out" animation to disguise TP
            player.GetComponent<PlayerMovement>().StopMoving();
            animator.SetTrigger("FadeOut");

        }
    }

    // Moves player to TeleportPoint (controller is disabled / enabled to prevent issues)
    public void TeleportToPoint()
    {
        player.GetComponent<CharacterController>().enabled = false;
        player.transform.position = teleportPoint.transform.position;
        player.GetComponent<CharacterController>().enabled = true;
    }

    // enables player movement again once anim is finished (called via animation event)
    public void FinishFade()
    {
        player.GetComponent<PlayerMovement>().StartMoving();
    }
}
