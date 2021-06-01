using UnityEngine;

// ||=======================================================================||
// || ShopNPC: An NPC the player can interact with to access a shop that    ||
// ||   allows them to purchase items.                                      ||
// ||=======================================================================||
// || Used on prefab: Joe/Environment/Shop                                  ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public class ShopNPC : InteractableWithOutline
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [Header("Shop NPC")]
    [SerializeField] private ShopType shopType; // The type of shop this NPC runs. Defines the shop name, what is sold etc.

    #endregion

    #region Properties

    public ShopType ShopType { get { return shopType; } }

    #endregion

    private NPCManager      npcManager;         // NPCManager reference, used for camera focus code
    private PlayerMovement  playerMovement;     // PlayerMovement script reference
    private ShopTalkPanel   talkUI;             // UI shown when talking to the NPC
    private bool            focusing;           // Whether the camera is focusing on this NPC

    private void Awake()
    {
        // Find required GameObjects in the scene and get their attached scripts
        playerMovement      = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        npcManager          = GameObject.FindGameObjectWithTag("QuestManager").GetComponent<NPCManager>();
        talkUI              = GameObject.FindGameObjectWithTag("ShopTalkUI").GetComponent<ShopTalkPanel>();
    }

    public override void Interact()
    {
        if (!focusing)
        {
            // Focus on the NPC

            base.Interact();
            
            // Disallow player movement to prevent them moving away while focusing
            playerMovement.StopMoving();

            // Move the camera to focus on the NPC
            npcManager.StartFocusCameraMove(transform.Find("FocusPoint"));

            // Unlock the cursor so the player can interact with shop UI
            Cursor.lockState = CursorLockMode.None;

            // Show the talk UI
            talkUI.ShowAndSetup(this);

            // Player is now focusing on the shop NPC
            focusing = true;
        }
    }

    public void StopInteracting()
    {
        if (focusing)
        {
            // Re-enable player movement
            playerMovement.StartMoving();

            // Set the camera back to its default state
            npcManager.StopFocusCamera();

            // Lock the cursor to the centre of the screen for controlling the player camera
            Cursor.lockState = CursorLockMode.Locked;

            // Hide the talk UI
            talkUI.Hide();

            // No longer focusing
            focusing = false;
        }
    }
}
