using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   Base class written by Joe Allen
// Description:         Allows player to interact & get on / off ladders
// Development window:  Prototype phase
// Inherits from:       InteractableWithOutline

public class Ladder : InteractableWithOutline
{
    [SerializeField]    private Transform snapPoint;
    public override void Interact()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        // Flags ladder as having been interacted with in PlayerMovement script
        player.GetComponent<PlayerMovement>().InteractWithLadder(snapPoint.position);
        base.Interact();
    }
}
