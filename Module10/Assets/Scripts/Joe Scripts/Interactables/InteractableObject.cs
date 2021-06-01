using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

// ||=======================================================================||
// || InteractableObject: An object that can be interacted with by pressing ||
// ||   E while hovering over it within a certain range.                    ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public abstract class InteractableObject : MonoBehaviour
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [Header("Interactable Properties")]

    [Tooltip("How close the player needs to be to this object to interact with it")] // See tooltip
    [SerializeField] private float      interactionRange        = 5.0f;

    [SerializeField] private GameObject interactTooltipPrefab;  // Prefab for the UI tooltip/popup shown while hovering over the object
    [SerializeField] private Vector3    interactTooltipOffset;  // Offset relative to the object's origin position to use when displaying the above tooltip
    [SerializeField] private string     tooltipNameText;        // The object name to be displayed on the above tooltip, e.g. 'Crafting Table', 'Door'

    #endregion

    private bool        mouseOver;                  // Whether the mouse is currently over the object (at any distance)
    private bool        hoveringInRange;            // True if the mouse pointer is over the object AND the player is within range
    private float       hoverTimer;                 // How many seconds the player has been hovering over the object

    private GameObject  playerGameObject;           // Player GameObject used to check for distance from the object
    private Camera      mainPlayerCamera;           // The player camera for calculating the position of the tooltip in screen space

    private Transform   canvasTransform;            // Canvas transform used as a parent for the UI tooltip
    private GameObject  interactTooltip;            // The instantiated interact tooltip, null if not active
    private Vector3     localInteractTooltipOffset; // interactTooltipOffset converted to local space

    private const float InteractPopupDelay = 0.3f;  // The amount of time the player has to hover over the object before interactTooltip is shown

    protected virtual void Start()
    {
        // Get references to player GameObject/camera and canvas
        playerGameObject    = GameObject.FindGameObjectWithTag("Player");
        mainPlayerCamera    = Camera.main;
        canvasTransform     = GameObject.FindGameObjectWithTag("JoeCanvas").transform;

        // Calculate the offset of the interact tooltip in local space (changes depending on the object's rotation in the world)
        localInteractTooltipOffset = transform.InverseTransformDirection(interactTooltipOffset);
    }

    protected virtual void Update()
    {
        // Interactables can only be interacted with when the cursor is locked (i.e. the player is moving around the world/not in a menu)
        if(Cursor.lockState == CursorLockMode.Locked)
        {
            // Check whether the player is hovering within the valid range
            UpdateHoverState();

            if (hoveringInRange)
            {
                //  Ensure no input field is selected to prevent unintended behaviour when pressing E while typing
                if (Input.GetKeyDown(KeyCode.E) && !InputFieldSelection.AnyFieldSelected)
                {
                    // The player has pressed E while hovering over the object, interact with it
                    Interact();
                }

                if (hoverTimer >= InteractPopupDelay && interactTooltipPrefab != null && mainPlayerCamera != null)
                {
                    // Player has been hovering for longer than InteractPopupDelay, show the popup if
                    //   it isn't already showing, otherwise move it to the object's position in screen space

                    if (interactTooltip == null)
                    {
                        ShowInteractTooltip();
                    }
                    else
                    {
                        // Move the tooltip to the object's position + the local offset, all converted to screen space
                        interactTooltip.transform.position = mainPlayerCamera.WorldToScreenPoint(transform.position + localInteractTooltipOffset);
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

    private void ShowInteractTooltip()
    {
        // Instantiate the tooltip GameObject, using the canvas as its parent
        interactTooltip = Instantiate(interactTooltipPrefab, canvasTransform);

        if (!string.IsNullOrEmpty(tooltipNameText))
        {
            // Tooltip name text was set, display the name above the 'Press E to interact' text
            interactTooltip.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = tooltipNameText;
        }
        else
        {
            // No tooltip name text was set, disable the GameObject that would usually display this text
            interactTooltip.transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    private void HideInteractTooltip()
    {
        // Destroy the tooltip if one exists
        if (interactTooltip != null)
        {
            Destroy(interactTooltip);
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
        if((Vector3.Distance(playerGameObject.transform.position, transform.position) <= interactionRange) && !EventSystem.current.IsPointerOverGameObject())
        {
            // The distance between the player and the interactable is less than the interactionRange set in the inspector,
            //   and the pointer is not over another GameObject. Player is in range.
            return true;
        }

        // Player is out of range
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

        // Hide the interact tooltip to it doesn't stay visible while not hovering
        HideInteractTooltip();
    }
}