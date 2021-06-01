using UnityEngine;
using UnityEngine.EventSystems;

// ||=======================================================================||
// || HeldItemManager: Handles the creation/deletion of and input events    ||
// ||   for held items.                                                     ||
// ||=======================================================================||
// || Used on prefab: Player                                                ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public class HeldItemManager : MonoBehaviour
{
    private Item            heldItem;               // The item being held
    private ContainerSlotUI heldItemSlot;           // The slot containing the above item
    private GameObject      heldGameObject;         // The GameObject related to the held item
    private HeldItem        heldItemScript;         // The held item script on the above GameObject
    private Transform       playerCameraTransform;  // Transform of the main player camera, acts as the parent for held objects
    private HotbarPanel     hotbarPanel;            // The player's hotbar
    private float           mouseHoldTimer;         // How long the mouse has been held down for (seconds)

    private const float     MouseHoldThreshold = 0.2f;  //How long the mouse has to be pressed down for to count as a 'hold' rather than a 'click'

    void Start()
    {
        hotbarPanel             = GameObject.FindGameObjectWithTag("Hotbar").GetComponent<HotbarPanel>();
        playerCameraTransform   = GameObject.FindGameObjectWithTag("MainCamera").transform;

        //OnHeldItemSelectionChanged will be called when the selected hotbar item changes
        hotbarPanel.HeldItemChangedEvent += OnHeldItemSelectionChanged;
    }

    void Update()
    {
        CheckForPlayerInput();
    }

    private void CheckForPlayerInput()
    {
        if(heldItemScript != null)
        {
            if (heldItem != null && heldGameObject != null)
            {
                // Player is holding an item with a HeldItem script attached

                if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject())
                {
                    // Player released the left mouse button while their pointer was not over a UI object

                    if (mouseHoldTimer < MouseHoldThreshold)
                    {
                        // If the threshold for a click and release counting as a 'hold' was not reached, perform the held item's main ability
                        heldItemScript.PerformMainAbility();
                    }

                    if (heldItemScript.PerformingSecondaryAbility)
                    {
                        // If the player is currently performing the held item's secondary ability, stop when the mouse button is released
                        heldItemScript.EndSecondaryAbility();
                    }
                }
                else if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
                {
                    // The left mouse button is down and not over a UI object

                    // Increment the timer that keeps track of how long the mouse is held down for
                    UpdateMouseHoldTimer(mouseHoldTimer + Time.unscaledDeltaTime);

                    if (!heldItemScript.PerformingSecondaryAbility && mouseHoldTimer > MouseHoldThreshold)
                    {
                        //If the player is not already performing a secondary ability and the threshold for a mouse 'hold' was reached,
                        //  start the secondary ability behaviour for the held item
                        heldItemScript.StartSecondardAbility();
                    }
                }
                else
                {
                    // Player is not holding/releasing the mouse, default the mouse hold timer to 0
                    UpdateMouseHoldTimer(0.0f);
                }
            }
            else
            {
                // Player is holding nothing/holding an item without an attached GameObject, there is no mouse click/hold
                //   behaviour needed so default the mouse hold timer to 0
                UpdateMouseHoldTimer(0.0f);
            }
        }
    }

    private void UpdateMouseHoldTimer(float value)
    {
        if(mouseHoldTimer != value)
        {
            // Set the mouse hold timer to the given value
            mouseHoldTimer = value;

            // Consumable items show a 'fill' over their hotbar slot which increases in size
            //   as the mouse is held down, to show that the player is 'eating' the item
            if (heldItemSlot != null && (value == 0.0f || heldItem is ConsumableItem))
            {
                heldItemSlot.SetCoverFillAmount(value / MouseHoldThreshold);
            }
        }
    }

    public void OnHeldItemSelectionChanged(Item item, ContainerSlotUI containerSlot)
    {
        Item oldHeldItem = heldItem;
        heldItem = item;

        // Reset the timer that keeps track of how long the mouse is held down for so it doesn't
        //   carry over from the previously selected item
        UpdateMouseHoldTimer(0.0f);

        heldItemSlot = containerSlot;

        if (heldItem != null)
        {
            // The player is now holding a new item

            if (oldHeldItem == null || heldItem.Id != oldHeldItem.Id)
            {
                // The player was previously holding either nothing or a different item,
                //   so the held item GameObject needs to be updated

                Debug.Log("Player is holding " + heldItem.UIName);

                // Destroy the current held object to make way for a new one
                DestroyHeldGameObject();

                if (heldItem.HeldItemGameObject != null)
                {
                    // The item being held has a related GameObject - instantiate it
                    heldGameObject = Instantiate(heldItem.HeldItemGameObject, playerCameraTransform);
                    heldItemScript = heldGameObject.GetComponent<HeldItem>();

                    // Setup the held GameObject's HeldItem script if it has one
                    if(heldItemScript != null)
                    {
                        heldItemScript.Setup(heldItem, containerSlot);
                    }
                }
            }
        }
        else
        {
            // The player is now holding nothing
            DestroyHeldGameObject();
        }
    }

    private void DestroyHeldGameObject()
    {
        // Destroy the current held GameObject if one exists
        if (heldGameObject != null)
        {
            heldItemScript = null;
            Destroy(heldGameObject);
        }
    }
}
