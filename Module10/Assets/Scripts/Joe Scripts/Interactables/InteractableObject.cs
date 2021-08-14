using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

// ||=======================================================================||
// || InteractableObject: An object that can be interacted with by pressing ||
// ||   E while hovering over it within a certain range.                    ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || originally for the prototype phase.                                   ||
// ||                                                                       ||
// || Changes made during the production phase (Module 11):                 ||
// ||                                                                       ||
// || - Added GetPopupScreenPos & GetPopupWorldPos, see code for details    ||
// || - Bug fixes                                                           ||
// ||=======================================================================||

public abstract class InteractableObject : MonoBehaviour
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [Header("Interactable Properties")]

    [Tooltip("How close the player needs to be to this object to interact with it")] // <--
    [SerializeField] private float      interactionRange        = 5.0f;

    [SerializeField] private GameObject interactTooltipPrefab;  // Prefab for the UI tooltip/popup shown while hovering over the object
    [SerializeField] protected Vector3  interactTooltipOffset;  // Offset relative to the object's origin position to use when displaying the above tooltip
    [SerializeField] protected string   tooltipNameText;        // The object name to be displayed on the above tooltip, e.g. 'Crafting Table', 'Door'

    #endregion

    #region Properties

    public bool CanInteract { get { return canInteract; } set { canInteract = value; } }

    #endregion

    private bool            mouseOver;                      // Whether the mouse is currently over the object (at any distance)
    protected bool          hoveringInRange;                // True if the mouse pointer is over the object AND the player is within range
    private float           hoverTimer;                     // How many seconds the player has been hovering over the object

    private Camera          mainPlayerCamera;               // The player camera for calculating the position of the tooltip in screen space

    private Transform       canvasTransform;                // Canvas transform used as a parent for the UI tooltip
    private GameObject      interactTooltip;                // The instantiated interact tooltip, null if not active
    private Vector3         worldInteractTooltipOffset;     // interactTooltipOffset converted to world space

    protected bool          canInteract = true;             // Whether this object can be interacted with
    protected bool          enableTooltip = true;           // Whether the interaction tooltip is enabled (when canInteract = true)
    protected bool          showPressETooltipText = true;   // Whether the 'Press E to interact' should be shown on the tooltip
    protected bool          overrideTooltipBehaviour;       // Whether the default tooltip behaviour (show on hover after InteractPopupDelay) is used

    private const float     InteractPopupDelay = 0.3f;      // The amount of time the player has to hover over the object before interactTooltip is shown by default


    protected virtual void Start()
    {
        canvasTransform  = GameObject.FindGameObjectWithTag("JoeCanvas").transform;

        // Calculate the offset of the interact tooltip in world space (changes depending on the object's rotation in the world)
        worldInteractTooltipOffset = transform.TransformDirection(interactTooltipOffset);
    }

    protected virtual void Update()
    {
        if(mainPlayerCamera == null)
        {
            // Re-find the player camera if it becomes null (can happen when switching scenes)
            mainPlayerCamera = Camera.main;
        }

        if (canInteract)
        {
            // The player is allowed to interact with this object

            // Interactables can only be interacted with when the cursor is locked (i.e. the player is moving around the world/not in a menu)
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                // Check whether the player is hovering within the valid range
                UpdateHoverState();

                if (hoveringInRange)
                {
                    //  Ensure no input field is selected to prevent unintended behaviour when pressing E while typing
                    if (Input.GetKeyDown(KeyCode.E) && !InputFieldSelection.AnyFieldSelected && !GameSceneUI.Instance.ShowingCinematicsCanvas)
                    {
                        // The player has pressed E while hovering over the object, interact with it
                        Interact();
                    }

                    if (hoverTimer >= InteractPopupDelay && interactTooltipPrefab != null && mainPlayerCamera != null)
                    {
                        // Player has been hovering for longer than InteractPopupDelay, show the popup if
                        //   it isn't already showing, otherwise move it to the object's position in screen space

                        if (!overrideTooltipBehaviour && interactTooltip == null)
                        {
                            ShowInteractTooltip();
                        }
                    }
                    else
                    {
                        // Increment the hover timer until it reaches InteractPopupDelay
                        hoverTimer += Time.unscaledDeltaTime;
                    }
                }
            }
            else
            {
                // Cursor is not locked, the player is probably in a menu. End hovering
                EndHoverInRange();
            }

            if (interactTooltip != null)
            {
                Vector3 popupScreenPos = GetPopupScreenPos();
                if (popupScreenPos.z > 0.0f)
                {
                    // The player is facing the target position of the tooltip, move it to 
                    //   the object's position + the world offset, all converted to screen space
                    interactTooltip.transform.position = popupScreenPos;
                }
                else
                {
                    // The player is facing away from the tooltip position, hide it off-screen
                    interactTooltip.transform.position = new Vector3(0.0f, -10000f, 0.0f);
                }
            }
        }
    }

    public Vector3 GetPopupScreenPos(Vector3 offset = default)
    {
        // If using the player's camera, returns the position of the popup tooltip in screen space

        if(mainPlayerCamera != null)
        {
            return mainPlayerCamera.WorldToScreenPoint(transform.position + offset + worldInteractTooltipOffset);
        }

        // Player's camera is null, return a value that will hide any UI off-screen instead
        return new Vector3(0.0f, -10000.0f, 0.0f);
    }

    public Vector3 GetPopupWorldPos(Vector3 offset = default)
    {
        // Returns the position of the popup tooltip in world space

        return (transform.position + offset + worldInteractTooltipOffset);
    }

    private void UpdateHoverState()
    {
        if (PlayerIsWithinRange())
        {
            // Player is within the allowed interaction range

            if (mouseOver && !hoveringInRange)
            {
                // Player is in range and their mouse is over the object, start hovering
                StartHoverInRange();
            }
            else if(!mouseOver && hoveringInRange)
            {
                // Player is in range but no longer has their pointer over the object, end hovering
                EndHoverInRange();
            }
        }
        else
        {
            if (hoveringInRange)
            {
                // Player was in range but is not anymore, end hovering
                EndHoverInRange();
            }
        }
    }

    protected virtual void OnDestroy()
    {
        // Ensure the tooltip is destroyed with the object so it doesn't stay on screen permenantly
        HideInteractTooltip();
    }

    protected virtual void ShowInteractTooltip(string overridePressEText = null)
    {
        // Hide the current popup if one is already showing
        if (interactTooltip != null)
        {
            HideInteractTooltip();
        }

        if(enableTooltip)
        {
            // The interact tooltip is enabled

            // Instantiate the tooltip GameObject, using the canvas as its parent
            interactTooltip = Instantiate(interactTooltipPrefab, canvasTransform);

            if (!string.IsNullOrEmpty(tooltipNameText))
            {
                // Tooltip name text was set, display the name above the 'Press E to interact' text
                interactTooltip.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = tooltipNameText;
            }
            else
            {
                // No tooltip name text was set, disable the GameObject that would usually display this text
                interactTooltip.transform.GetChild(1).gameObject.SetActive(false);
            }

            if(!string.IsNullOrEmpty(overridePressEText))
            {
                // Change the default 'Press E To Interact' text if an override string was given
                interactTooltip.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = overridePressEText;
            }

            if(!showPressETooltipText)
            {
                // Hide the 'Press E to interact' text if showPressETooltipText is false
                interactTooltip.transform.GetChild(2).gameObject.SetActive(false);
            }
        }
    }

    protected void HideInteractTooltip()
    {
        // Destroy the tooltip if one exists
        if (interactTooltip != null)
        {
            Destroy(interactTooltip);
            interactTooltip = null;

            // Also reset the hover timer ready for the next time a tooltip is shown
            hoverTimer = 0.0f;
        }
    }

    private void OnMouseEnter()
    {
        // Called when the mouse pointer enters the object at any distance
        mouseOver = true;
    }

    private void OnMouseExit()
    {
        // Called when the mouse pointer exits the object at any distance
        mouseOver = false;
    }

    private bool PlayerIsWithinRange()
    {
        if(PlayerInstance.ActivePlayer != null)
        {
            if ((Vector3.Distance(PlayerInstance.ActivePlayer.gameObject.transform.position, transform.position) <= interactionRange) && !EventSystem.current.IsPointerOverGameObject())
            {
                // The distance between the player and the interactable is less than the interactionRange set in the inspector,
                //   and the pointer is not over another GameObject. Player is in range.
                return true;
            }
        }

        // Player is out of range/not yet in the scene
        return false;
    }

    public virtual void Interact()
    {
        // End hovering on interact so hover behaviour does not persist, e.g. if a UI menu
        //   opens when interacting the hover popup should not continue being displayed over the top
        EndHoverInRange();
    }

    public virtual void StartHoverInRange()
    {
        // Hovering in range - reset hover timer from any previous times
        hoverTimer = 0.0f;
        hoveringInRange = true;
    }

    public virtual void EndHoverInRange()
    {
        // No longer hovering in range
        hoverTimer = 0.0f;
        hoveringInRange = false;

        if(!overrideTooltipBehaviour)
        {
            // Hide the interact tooltip to it doesn't stay visible while not hovering
            HideInteractTooltip();
        }
    }
}