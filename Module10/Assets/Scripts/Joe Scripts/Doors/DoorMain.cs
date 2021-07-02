using UnityEngine;
using UnityEngine.Serialization;

// ||=======================================================================||
// || DoorMain: Handles how doors are opened/closed and the events that     ||
// ||   are triggered. Also allows doors to be locked/unlocked.             ||
// ||=======================================================================||
// || Used on prefab: Joe/Environment/Doors/Door                            ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public class DoorMain : MonoBehaviour, IPersistentSceneObject, IExternalTriggerListener
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    // See tooltips for comments

    [Space]
    [Header("(See tooltips for info)")]
    [Header("Door")]

    [SerializeField] [FormerlySerializedAs("manualOpen")] [Tooltip("Whether the door can be opened directly by a player (rather than an external method such as puzzle button)")]
    private bool        playerCanOpen = true;

    [SerializeField] [Tooltip("If true, the door will open when the player enters the trigger area. Otherwise, it will open when pressing the interaction key")]
    private bool        openOnTriggerEnter;

    [SerializeField] [Tooltip("Item required to unlock the door (none if left empty)")]
    private Item        unlockItem;

    [SerializeField] [Tooltip("Number of seconds before the door closes automatiaclly, 0 = stay open forever")]
    private float       closeAfterTime = 5.0f;

    [SerializeField] private Animator   animator;           // Animator used for door open/close animations

    [SerializeField] private ExternalTrigger[] triggers;    // Triggers to detect if the player is on either side of the door

    [Header("Sounds")]
    [SerializeField] private SoundClass openSound;
    [SerializeField] private SoundClass closeSound;

    #endregion

    #region Properties

    public bool PlayerCanOpen          { get { return playerCanOpen; } }
    public bool OpenOnTriggerEnter  { get { return openOnTriggerEnter; } }

    #endregion

    // Note: 'inwards' and 'outwards' here are somewhat arbitrary and will vary depending on the rotation of the door when placed.
    //   The important thing is that inside/outside or inwards/outwards represent either side of the door, and are always opposites.

    private bool openIn;            // True if the door is open inwards
    private bool openOut;           // True if the door is open outwards
    private bool unlocked;          // Whether the door has been unlocked by 'unlockItem' (see above)

    private bool inInsideTrigger;   // True if the player is standing in the trigger on the 'inside' side of the door
    private bool inOutsideTrigger;  // True if the player is standing in the trigger on the 'outside' side of the door

    private float doorOpenTimer;    // The amount of time the door has been open for (seconds)

    private InventoryPanel  playerInventory;    // Reference to the player's inventory panel

    private void Start()
    {
        playerInventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<InventoryPanel>();

        // Subscribe to save/load events so this door's data will be saved when the game is saved
        SaveLoadManager.Instance.SubscribeSceneSaveLoadEvents(OnSceneSave, OnSceneLoadSetup, OnSceneLoadConfigure);

        // Add this door as a listener for the external triggers so
        //   the player can be detected on either side of the door
        for (int i = 0; i < triggers.Length; i++)
        {
            triggers[i].AddListener(this);
        }

        // Disable sounds temporarily on start so any animations that play to
        //   get the doors into the correct state do not cause sounds to be played
        animator.SetBool("DisableSounds", true);
    }

    private void OnDestroy()
    {
        // The door no longer exists, unsubscribe from to save/load events
        SaveLoadManager.Instance.UnsubscribeSceneSaveLoadEvents(OnSceneSave, OnSceneLoadSetup, OnSceneLoadConfigure);
    }

    private void Update()
    {
        if(openIn || openOut)
        {
            // Door is open either inwards or outwards, increment open timerS
            doorOpenTimer += Time.deltaTime;

            if(!openOnTriggerEnter)
            {
                // Door has been open for a while and the player isn't standing in the way of it closing, close it
                if ((closeAfterTime != 0.0f) && (doorOpenTimer >= closeAfterTime) &&
                    ((openIn && !inInsideTrigger) || (openOut && !inOutsideTrigger)))
                {
                    SetAsClosed();
                }
            }
        }
    }

    public void OnSceneSave(SaveData saveData)
    {
        string id = GetUniquePositionId();

        Debug.Log("Saving data for door: " + id);

        // Save the locked state
        saveData.AddData("unlocked_" + id, unlocked);

        // Save whether the door is closed (0), open in (1) or open out (2)
        byte openStateToSave = 0;

        if (openIn)
        {
            openStateToSave = 1;
        }
        else if (openOut)
        {
            openStateToSave = 2;
        }

        saveData.AddData("openState_" + GetUniquePositionId(), openStateToSave);
    }

    public void OnSceneLoadSetup(SaveData saveData)
    {
        string id = GetUniquePositionId();

        Debug.Log("Loading data for door: " + id);

        unlocked = saveData.GetData<bool>("unlocked_" + id);

        byte openState = saveData.GetData<byte>("openState_" + id);

        // If the door was open when saved, open it in the correct direction
        if(openState == 1)
        {
            SetAsOpen(true);
        }
        else if(openState == 2)
        {
            SetAsOpen(false);
        }
    }

    public void OnSceneLoadConfigure(SaveData saveData) { }

    public void OnExternalTriggerEnter(string triggerId, Collider other)
    {
        if(other.CompareTag("Player"))
        {
            if (triggerId == "inside")
            {
                TriggerEntered(true);
            }
            else if (triggerId == "outside")
            {
                TriggerEntered(false);
            }
        }
    }

    public void OnExternalTriggerStay(string triggerId, Collider other) { }

    public void OnExternalTriggerExit(string triggerId, Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (triggerId == "inside")
            {
                TriggerExited(true);
            }
            else if (triggerId == "outside")
            {
                TriggerExited(false);
            }
        }
    }

    public void Interact()
    {
        if(playerCanOpen)
        {
            // Player can open the door by manually interacting

            if (!openIn && !openOut)
            {
                // Door is currently closed - open it in/out depending on where the player is stood

                // Ensure open/close sounds are enabled
                animator.SetBool("DisableSounds", false);

                if (inInsideTrigger)
                {
                    SetAsOpen(false);
                }
                else if (inOutsideTrigger)
                {
                    SetAsOpen(true);
                }
            }
            else
            {
                // Door is currently open - close it

                SetAsClosed();
            }
        }
        else
        {
            // Notify the player that they can't open the door
            NotificationManager.Instance.AddNotificationToQueue(NotificationMessageType.CantOpenDoorDirectly);
        }
    }

    private void TriggerEntered(bool inside)
    {
        // The player entered a trigger on the inside or outside
        //   of the door, set trigger variables as appropriate

        inInsideTrigger = inside;
        inOutsideTrigger = !inside;

        if(openOnTriggerEnter)
        {
            // The door should open when the player enters a trigger,
            //   but only if it is allowed to be opened by the player

            if (playerCanOpen)
            {
                SetAsOpen(!inside);
            }
            else
            {
                // Notify the player that they can't open the door
                NotificationManager.Instance.AddNotificationToQueue(NotificationMessageType.CantOpenDoorDirectly);
            }
        }
    }

    private void TriggerExited(bool inside)
    {
        // Player is exiting a trigger, reset inside/outside
        //   bool depending on which was exited

        if (inside)
        {
            inInsideTrigger = false;
        }
        else
        {
            inOutsideTrigger = false;
        }

        if(openOnTriggerEnter && !inInsideTrigger && !inOutsideTrigger && closeAfterTime > 0.0f)
        {
            // Door is triggered automatically and player is in neither trigger, close the door
            SetAsClosed();
        }
    }

    public void SetAsOpen(bool inwards)
    {
        if(CanOpenDoor())
        {
            // The door can be opened

            // Reset the open timer so it can start accurately counting up again
            doorOpenTimer = 0.0f;

            // Keep track of whether it opened inwards/outwards
            openIn = inwards;
            openOut = !inwards;

            // Animate the door to open in/out
            if (inwards)
            {
                animator.SetBool("OpenIn", true);
            }
            else
            {
                animator.SetBool("OpenOut", true);
            }
        }
    }

    private bool CanOpenDoor()
    {
        if (unlockItem != null && !unlocked)
        {
            // The door requires an item to unlock/open

            // Creating a group of 1 since the ContainsQuantityOfItem functions take an item group
            ItemGroup requiredItemGroup = new ItemGroup(unlockItem, 1);

            // Check if the required item is in the player's inventory/hotbar

            bool itemInInventory = playerInventory.ContainsQuantityOfItem(requiredItemGroup, out _);

            if (itemInInventory)
            {
                // The player has the required item, unlock the door
                unlocked = true;

                // Notify the player that the door was unlocked
                NotificationManager.Instance.AddNotificationToQueue(NotificationMessageType.DoorUnlocked, new string[] { unlockItem.UIName });

                // Remove the item from the inventory or hotbar
                playerInventory.TryRemoveItem(unlockItem);

                // The door can now be opened
                return true;
            }
            else
            {
                // The player does not have the required item so the door cannot be opened. Notify them about which item is needed
                NotificationManager.Instance.AddNotificationToQueue(NotificationMessageType.ItemRequiredForDoor, new string[] { unlockItem.UIName });
                return false;
            }
        }

        // The door is unlocked, or the door requires no item to open
        return true;
    }

    public void SetAsClosed()
    {
        // Set the door as closed and animate it to swing shut

        openIn = false;
        openOut = false;

        animator.SetBool("OpenIn", false);
        animator.SetBool("OpenOut", false);
    }

    private string GetUniquePositionId()
    {
        return "door_" + transform.position.x + "_" + transform.position.y + "_" + transform.position.z;
    }

    // Open/close sounds triggered by animation events:
    //==================================================

    public void PlayOpenSound()
    {
        if(!animator.GetBool("DisableSounds"))
        {
            // Sounds are enabled, play an open sound at the position of the door model
            AudioManager.Instance.PlaySoundEffect3D(openSound, transform.GetChild(0).GetChild(0).position);
        }
        else
        {
            // Sounds were disabled, don't play a sound but re-enable them for next time
            animator.SetBool("DisableSounds", false);
        }
    }

    public void PlayCloseSound()
    {
        if (!animator.GetBool("DisableSounds"))
        {
            // Sounds are enabled, play a close sound at the position of the door model
            AudioManager.Instance.PlaySoundEffect3D(closeSound, transform.GetChild(0).GetChild(0).position);
        }
        else
        {
            // Sounds were disabled, don't play a sound but re-enable them for next time
            animator.SetBool("DisableSounds", false);
        }
    }
}
