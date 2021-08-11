using UnityEngine;
using UnityEngine.Serialization;

// ||=======================================================================||
// || DoorMain: Handles how doors are opened/closed and the events that     ||
// ||   are triggered. Also allows doors to be locked/unlocked.             ||
// ||=======================================================================||
// || Used on prefab: Joe/Environment/Doors/Door                            ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || originally for the prototype phase.                                   ||
// ||                                                                       ||
// || Changes made during the production phase (Module 11):                 ||
// ||                                                                       ||
// || - Added DoorOpenRestriction: Doors can be one-way, or disabled.       ||
// || - Added the ability to show a padlock symbol when the door is locked. ||
// || - Various smaller refinements and fixes.                              ||
// ||=======================================================================||

public class DoorMain : MonoBehaviour, IPersistentSceneObject, IExternalTriggerListener
{
    public enum DoorOpenRestriction
    {
        None,           
        OneWayInside,   // Door can only be opened from the 'inside' side
        OneWayOutside,  // Door can only be opened from the 'outside' side
        Disabled        // Door cannot be opened at all, regardless of other settings
    }

    public enum DoorLockSymbolMode
    {
        NeverShow,          // Don't ever show a lock symbol
        CanShowWhenOpen,    // Only show a lock symbol when the door is locked and open
        CanShowWhenClosed,  // Only show a lock symbol when the door is locked and closed
        CanAlwaysShow       // Always show a lock symbol when the door is locked
    }

    #region InspectorVariables
    // Variables in this region are set in the inspector

    // See tooltips for comments

    [Space]
    [Header("(See tooltips for info)")]
    [Header("Door")]

    [SerializeField]
    private DoorCollider        mainCollider;

    [SerializeField] [FormerlySerializedAs("manualOpen")] [Tooltip("Whether the door can be opened directly by a player (rather than an external method such as puzzle button)")]
    private bool                playerCanOpen = true;
    
    [SerializeField] [Tooltip("If true, the door will open when the player enters the trigger area. Otherwise, it will open when pressing the interaction key")]
    private bool                openOnTriggerEnter;

    [SerializeField] [Tooltip("Whether the door can be opened from both sides, only one side, or is completely disabled (regardless of unlock status)")]
    private DoorOpenRestriction openRestriction;

    [SerializeField]
    private DoorLockSymbolMode  lockSymbolMode = DoorLockSymbolMode.CanShowWhenClosed;

    [SerializeField] [Tooltip("Item required to unlock the door (none if left empty)")]
    private Item                unlockItem;

    [SerializeField] [Tooltip("Number of seconds before the door closes automatiaclly, 0 = stay open forever")]
    private float               closeAfterTime = 5.0f;

    [SerializeField] private Animator           animator;           // Animator used for door open/close animations

    [SerializeField] private ExternalTrigger[]  triggers;           // Triggers to detect if the player is on either side of the door

    [SerializeField] private GameObject         lockedSymbolPrefab; // Prefab instantiated when a lock symbol needs to be shown

    [Header("Sounds")]
    [SerializeField] private SoundClass openSound;  // Sound played when the door is opened
    [SerializeField] private SoundClass closeSound; // Sound played when the door is closed

    #endregion

    #region Properties

    public bool PlayerCanOpen       { get { return playerCanOpen; } }
    public bool OpenOnTriggerEnter  { get { return openOnTriggerEnter; } }

    #endregion

    // Note: 'inwards' and 'outwards' here are somewhat arbitrary and will vary depending on the rotation of the door when placed.
    //   The important thing is that inside/outside or inwards/outwards represent either side of the door, and are always opposites.

    private bool openIn;                    // True if the door is open inwards
    private bool openOut;                   // True if the door is open outwards
    private bool unlocked;                  // Whether the door has been unlocked by 'unlockItem' (see above)

    private bool inInsideTrigger;           // True if the player is standing in the trigger on the 'inside' side of the door
    private bool inOutsideTrigger;          // True if the player is standing in the trigger on the 'outside' side of the door

    private float doorOpenTimer;            // The amount of time the door has been open for (seconds)

    private DoorLockedSymbol lockedSymbol;  // The symbol that is sometimes shown when the door is locked

    private void Start()
    {
        // Subscribe to save/load events so this door's data will be saved when the game is saved
        SaveLoadManager.Instance.SubscribeSceneSaveLoadEvents(OnSceneSave, OnSceneLoadSetup, OnSceneLoadConfigure);

        // Add this door as a listener for the external triggers so
        //   the player can be detected on either side of the door
        for (int i = 0; i < triggers.Length; i++)
        {
            triggers[i].AddListener(this);
        }

        // Instantiate the lock symbol, ready to be shown if needed
        lockedSymbol = Instantiate(lockedSymbolPrefab, GameSceneUI.Instance.transform.Find("DoorSymbols")).GetComponent<DoorLockedSymbol>();
    }

    private void OnDestroy()
    {
        // The door no longer exists, unsubscribe from to save/load events
        SaveLoadManager.Instance.UnsubscribeSceneSaveLoadEvents(OnSceneSave, OnSceneLoadSetup, OnSceneLoadConfigure);
    }

    private void Update()
    {
        // Update the lock symbol and the timer that keeps track of how long the door is open

        LockSymbolUpdates();

        OpenTimerUpdates();
    }

    private void LockSymbolUpdates()
    {
        // Get the position that the lock symbol should be shown at, both in screen space and world space
        Vector3 lockSymbolOffset = new Vector3(0.0f, -0.4f, 0.0f);
        Vector3 lockedSymbolScreenPos = mainCollider.GetPopupScreenPos(lockSymbolOffset);
        Vector3 lockedSymbolWorldPos = mainCollider.GetPopupWorldPos(lockSymbolOffset);

        // Determine whether the lock symbol should be drawn
        bool drawLockSymbol = DrawLockSymbol(lockedSymbolScreenPos, lockedSymbolWorldPos, out float symbolDrawSize);

        if (drawLockSymbol)
        {
            // The symbol should be drawn, show it
            lockedSymbol.Show();

            if (unlockItem != null && GameSceneUI.Instance.PlayerInventory.ContainsItem(unlockItem))
            {
                // The door required an unlock item and the player has it, shown a green 'ready to unlock' icon
                lockedSymbol.SetIcon(DoorSymbolIcon.ReadyToUnlock);
            }
            else
            {
                // Otherwise show the standard red 'locked' icon
                lockedSymbol.SetIcon(DoorSymbolIcon.Locked);
            }
        }
        else
        {
            // The symbol should not be drawn, hide it
            lockedSymbol.Hide();
        }

        if (lockedSymbol.Visible)
        {
            if (lockedSymbolScreenPos.z > 0.0f)
            {
                // The locked symbol is visible within camera view

                // Position it at the calculated screen position
                lockedSymbol.gameObject.transform.position = lockedSymbolScreenPos;

                // Scale it based on distance from the player if a valid symmbol draw size was returned
                if (symbolDrawSize != -1.0f)
                {
                    lockedSymbol.gameObject.transform.localScale = new Vector3(symbolDrawSize, symbolDrawSize, 1.0f);
                }
            }
            else
            {
                // The locked symbol can be shown but is behind the camera, hide it off-screen

                lockedSymbol.gameObject.transform.position = new Vector3(0.0f, -10000f, 0.0f);
            }
        }
    }

    private bool DrawLockSymbol(Vector3 lockedSymbolScreenPos, Vector3 lockedSymbolWorldPos, out float symbolDrawSize)
    {
        // Size to draw the symbol at, set to a valid value if the symbol should be drawn
        symbolDrawSize = -1.0f;

        if (lockedSymbolScreenPos.z > 0.0f)
        {
            // The symbol is not behind the camera

            if (ShouldShowDoorPopups() && Camera.main != null)
            {
                // Lock popups are currently enabled

                // Get a vector in the direction of the player from the symbol position, will act as a 'line of sight'
                Vector3 rayDirection = Vector3.Normalize(Camera.main.transform.position - lockedSymbolWorldPos);

                // Send out a ray to find if the player is blocked from seeing the symbol
                //  (Adding rayDirection * 0.5f to start point so the ray ignoers any part of the door itself)
                Ray r = new Ray(lockedSymbolWorldPos + rayDirection * 0.5f, rayDirection);

                if (Physics.Raycast(r, out RaycastHit hitInfo, 50.0f) && hitInfo.collider.CompareTag("Player"))
                {
                    // The ray hit the player

                    // Calculate a scale to draw the symbol at between 0 and 1, the closer to the player, the larger it will be drawn
                    symbolDrawSize = 1.0f - hitInfo.distance / 45.0f;
                    symbolDrawSize = Mathf.Clamp01(symbolDrawSize);

                    // The symbol will be drawn
                    return true;
                }
            }
        }

        // The symbol will not be drawn
        return false;
    }

    private void OpenTimerUpdates()
    {
        if (IsOpen() && !openOnTriggerEnter)
        {
            // Door is open either inwards or outwards, and not controlled by trigger enter/exit, increment the open timer
            doorOpenTimer += Time.deltaTime;

            // Door has been open for a while and the player isn't standing in the way of it closing, close it
            if ((closeAfterTime != 0.0f) && (doorOpenTimer >= closeAfterTime) &&
                ((openIn && !inInsideTrigger) || (openOut && !inOutsideTrigger)))
            {
                SetAsClosed();
            }
        }
    }

    public void OnSceneSave(SaveData saveData)
    {
        string id = GetUniquePositionId();

        Debug.Log("Saving data for door: " + id);

        // Save the unlocked state
        saveData.AddData("doorUnlocked_" + id, unlocked);

        // Save the open restiction state
        saveData.AddData("doorRestriction_" + id, openRestriction);

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

        saveData.AddData("doorOpenState_" + id, openStateToSave);
    }

    public void OnSceneLoadSetup(SaveData saveData)
    {
        string id = GetUniquePositionId();

        Debug.Log("Loading data for door: " + id);

        // Load the unlocked state
        unlocked = saveData.GetData<bool>("doorUnlocked_" + id);

        // Load the open restriction state
        openRestriction = saveData.GetData<DoorOpenRestriction>("doorRestriction_" + id);

        byte openState = saveData.GetData<byte>("doorOpenState_" + id);

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
                // The player entered the 'inside' trigger
                TriggerEntered(true);
            }
            else if (triggerId == "outside")
            {
                // The player entered the 'outside' trigger
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
                // The player exited the 'inside' trigger
                TriggerExited(true);
            }
            else if (triggerId == "outside")
            {
                // The player exited the 'outside' trigger
                TriggerExited(false);
            }
        }
    }

    private bool ShouldShowDoorPopups()
    {
        // Determines whether the door lock symbol should be shown when the player is looking at the door

        if ((lockSymbolMode == DoorLockSymbolMode.NeverShow) ||
            (lockSymbolMode == DoorLockSymbolMode.CanShowWhenOpen && !IsOpen()) ||
            (lockSymbolMode == DoorLockSymbolMode.CanShowWhenClosed && IsOpen()))
        {
            // The symbol should never be shown, or the door is opened/closed when the symbol is set to show in the opposite state
            return false;
        }

        // The z position of the player relative to the door that determines which side of the door they are on
        float playerRelativeZ = transform.InverseTransformPoint(PlayerInstance.ActivePlayer.transform.position).z;

        if ((openRestriction == DoorOpenRestriction.OneWayInside && playerRelativeZ > 0.0f) ||
            (openRestriction == DoorOpenRestriction.OneWayOutside && playerRelativeZ < 0.0f))
        {
            // The player is on the wrong side of a one-way door, the lock symbol should be shown
            return true;
        }

        // Show the symbol if the door is disabled, locked without the required item, or the player is not allowed to open it
        return openRestriction == DoorOpenRestriction.Disabled || (unlockItem != null && !unlocked) || !playerCanOpen;
    }

    private bool IsOpen()
    {
        // Returns true if the door is open in any direction
        return openIn || openOut;
    }

    public void SetDoorOpenRestriction(DoorOpenRestriction restriction)
    {
        // Sets the restriction and closes the door if it's open when it should be disabled

        openRestriction = restriction;

        if(openRestriction == DoorOpenRestriction.Disabled && IsOpen())
        {
            SetAsClosed();
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
        if(CanOpenDoor(inwards))
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

    private bool CanOpenDoor(bool inwards)
    {
        if(openRestriction == DoorOpenRestriction.Disabled)
        {
            // The door is not allowed to be opened at all
            NotificationManager.Instance.AddNotificationToQueue(NotificationMessageType.DoorCannotBeOpened);
            return false;
        }

        if ((inwards && openRestriction == DoorOpenRestriction.OneWayOutside) || (!inwards && openRestriction == DoorOpenRestriction.OneWayInside))
        {
            // The door is one-way and the player is tying to open it from the unallowed side
            NotificationManager.Instance.AddNotificationToQueue(NotificationMessageType.DoorWrongSide);
            return false;
        }

        if (unlockItem != null && !unlocked)
        {
            // The door requires an item to unlock/open

            // Check if the required item is in the player's inventory/hotbar

            InventoryPanel playerInventory = GameSceneUI.Instance.PlayerInventory;

            bool itemInInventory = playerInventory.ContainsItem(unlockItem);

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
        // Set the door as closed and animate it to shut

        openIn = false;
        openOut = false;

        animator.SetBool("OpenIn", false);
        animator.SetBool("OpenOut", false);
    }

    private string GetUniquePositionId()
    {
        return transform.position.x + "_" + transform.position.y + "_" + transform.position.z;
    }

    // Open/close sounds triggered by animation events:
    //==================================================

    // Sounds aren't played when loading scene data to avoid sound spam while doors are entering their saved states

    public void PlayOpenSound()
    {
        if(!SaveLoadManager.Instance.LoadingSceneData)
        {
            // Play an open sound at the position of the door model
            AudioManager.Instance.PlaySoundEffect3D(openSound, transform.GetChild(0).GetChild(0).position);
        }
    }

    public void PlayCloseSound()
    {
        if (!SaveLoadManager.Instance.LoadingSceneData)
        {
            // Play a close sound at the position of the door model
            AudioManager.Instance.PlaySoundEffect3D(closeSound, transform.GetChild(0).GetChild(0).position);
        }
    }
}
