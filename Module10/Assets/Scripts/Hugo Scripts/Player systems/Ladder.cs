using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   Base class written by Joe Allen
// Description:         Allows player to interact & get on / off ladders
// Development window:  Prototype phase & production phase
// Inherits from:       InteractableWithOutline

public class Ladder : InteractableWithOutline
{
    [SerializeField]    private Transform snapPoint;    // Saves X and Z co-ords player should "snap" to when using ladder

    public override void Interact()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        // Flags player as having interacted w/ laadder
        player.GetComponent<PlayerMovement>().InteractWithLadder(snapPoint.position);
        base.Interact();
    }
}
