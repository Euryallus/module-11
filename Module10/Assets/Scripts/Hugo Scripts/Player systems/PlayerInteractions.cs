using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Used by Hugo's scripts to detect when item / object is interacted with (will eventually be incorperated into Joe's version when we get a chance
// Development window:  Prototype phase
// Inherits from:       MonoBehaviour

public class PlayerInteractions : MonoBehaviour
{
    [SerializeField]    private Camera playerCamera;    // Ref to player's camera (used for raycasting)
    [SerializeField]    private NPCManager npcManager;  // Ref to NPC manager (used when interacting with NPCs)
                        private RaycastHit raycastHit;

    void Update()
    {
        // Runs when player interacts with an object by pressing E
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out raycastHit, 4.0f))
            {
                if (raycastHit.transform.gameObject.GetComponent<NPC>() != null)
                {
                    // If the player tries to interact with an NPC, call the InteractWithNPC() func. using data from the NPC component
                    if (npcManager != null)
                    {
                        // Sends NPC ref to NPCManager
                        NPC hitNPC = raycastHit.transform.gameObject.GetComponent<NPC>();
                        npcManager.InteractWithNPC(hitNPC);
                    }
                }

                // If raycast hits a Collectable object, call PickUp function
                if (raycastHit.transform.gameObject.GetComponent<Collectable>() != null)
                {
                    raycastHit.transform.gameObject.GetComponent<Collectable>().PickUp();
                }
            }
        }
    }
}
