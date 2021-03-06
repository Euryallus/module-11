using UnityEngine;

// ||=======================================================================||
// || PuzzleButton: A button that can be activated by the player or by      ||
// ||   dragging an object onto it. Can trigger Doors and MovingPlatforms.  ||
// ||=======================================================================||
// || Used on prefab: Joe/PuzzleElements/PuzzleButton                       ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || originally for the prototype phase.                                   ||
// ||                                                                       ||
// || Changes made during the production phase (Module 11):                 ||
// ||                                                                       ||
// || - Added the ability to change the look of button by adjusting its     ||
// ||    material when pressed/released                                     ||
// ||=======================================================================||

public class PuzzleButton : InteractableObject
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [Header("Puzzle Button")]
    [SerializeField] private bool                   playerCanActivate     = true;  // Whether the player can stand on the button to press it
    [SerializeField] private bool                   movableObjCanActivate = true;  // Whether movable objects can be placed on the button to press it
    [SerializeField] private bool                   requiresHeavyObject   = false; // If movableObjCanActivate, detemines whether a heavy/large object is required to press the button
    [SerializeField] private DoorPuzzleData[]       connectedDoors;                // Doors that will be opened/closed by the button
    [SerializeField] private PlatformPuzzleData[]   connectedPlatforms;            // Platforms that will be activated/paused by the button
    
    [SerializeField] private Animator               animator;                      // Controls button press/release animations
    [SerializeField] private BoxCollider            buttonCollider;                // Collider that detects when the player/an object is on the button

    [Header("Appearance")]

    [SerializeField] private MeshRenderer           buttonMeshRenderer;     // Renderer that the button material is applied to
    [SerializeField] private Material               baseButtonMaterial;     // Base material, an instance of which is used on the button

    // Button colours:

    [SerializeField]
    private Color buttonBaseColour;         // Albedo colour of the button

    [SerializeField] [ColorUsage(true, true)]
    private Color standardEmissionColour;   // Emission colour of the button when not pressed

    [SerializeField] [ColorUsage(true, true)]
    private Color pressedEmissionColour;    // Emission colour of the button when pressed

    #endregion

    private bool                 playerIsColliding;      // True if the player is colliding with the button
    private bool                 movableObjIsColliding;  // True if any movable object is colliding with the button
                                                            
    private bool                 lastFramePressed;       // Whether the button was pressed on the previous frame
    private bool                 pressed;                // Whether the button is currently pressed
    private PuzzleButtonSequence sequence;               // The sequence this button belongs to, if any

    private void Awake()
    {
        for (int i = 0; i < connectedPlatforms.Length; i++)
        {
            // Tell all platforms with TriggerOnPress/LoopOnPress that they are triggered by a button press

            if (connectedPlatforms[i].Behaviour == PlatformButtonBehaviour.TriggerOnPress ||
                connectedPlatforms[i].Behaviour == PlatformButtonBehaviour.LoopOnPress)
            {
                connectedPlatforms[i].Platform.TriggeredByButton = true;
            }

            connectedPlatforms[i].Platform.ButtonBehaviour = connectedPlatforms[i].Behaviour;
        }
    }

    protected override void Start()
    {
        base.Start();

        // Create an instance of the baseButtonMaterial to be used on this button and set its base colour
        buttonMeshRenderer.material = new Material(baseButtonMaterial);
        buttonMeshRenderer.material.SetColor("_BaseColor", buttonBaseColour);

        // Ensure all connected doors are in the released button state by default
        ButtonReleasedEvents();

        if(sequence != null)
        {
            // If the button is not part of a sequence, reset to its standard colour by default
            SetToStandardColour();
        }
    }

    protected override void Update()
    {
        base.Update();
        
        // The button is pressed if the player or a movable object is colliding with it
        pressed = (playerIsColliding || movableObjIsColliding);

        if(pressed && !lastFramePressed)
        {
            // Button was pressed this frame - trigger events
            ButtonPressedEvents();
        }
        else if(!pressed && lastFramePressed)
        {
            // Button was released this frame - trigger events
            ButtonReleasedEvents();
        }

        // Update the animator Pressed property so the button's current state will be reflected visually
        animator.SetBool("Pressed", pressed);

        lastFramePressed = pressed;
    }

    public void RegisterWithSequence(PuzzleButtonSequence sequence)
    {
        this.sequence = sequence;
    }

    private void ButtonPressedEvents()
    {
        // Hide the interaction tooltip
        enableTooltip = false;
        HideInteractTooltip();

        // Disable the collider now the button is pressed down
        buttonCollider.enabled = false;

        // Set the button to the pressed colour
        SetToPressedColour();

        for (int i = 0; i < connectedDoors.Length; i++)
        {
            // Open/close all doors depending on their default states
            DoorPuzzleData doorData = connectedDoors[i];

            if (doorData.OpenByDefault)
            {
                doorData.Door.SetAsClosed();
            }
            else
            {
                doorData.Door.SetAsOpen(doorData.OpenInwards);
            }
        }

        for (int i = 0; i < connectedPlatforms.Length; i++)
        {
            // Either move connected platforms or pause their movement depending on set behaviour type
            PlatformPuzzleData platformData = connectedPlatforms[i];

            if(platformData.Behaviour == PlatformButtonBehaviour.PauseOnPress)
            {
                platformData.Platform.Paused = true;
            }
            else // TriggerOnPress or LoopOnPress
            {
                platformData.Platform.StartMovingForwards();
            }
        }

        // If the button is in a sequence, register that it was pressed in that sequence
        if(sequence != null)
        {
            sequence.ButtonInSequencePressed(this);
        }

        // Play the 'button on' sound if not loading data (prevents sounds playing while setting up objecs on load)
        if (!SaveLoadManager.Instance.LoadingSceneData)
        {
            AudioManager.Instance.PlaySoundEffect3D("puzzleButtonOn", transform.position);
        }
    }

    private void ButtonReleasedEvents()
    {
        // Allow the interaction tooltip to be shown again
        enableTooltip = true;

        // Re-enable collisions since the button is no longer pressed down
        buttonCollider.enabled = true;

        if(sequence == null)
        {
            // The button is not in a sequence, reset its colour
            //   (buttons in a sequence retain their pressed colour until the sequence is failed)
            SetToStandardColour();
        }

        for (int i = 0; i < connectedDoors.Length; i++)
        {
            // Close/open all doors depending on their default states
            DoorPuzzleData doorData = connectedDoors[i];
            if (doorData.OpenByDefault)
            {
                doorData.Door.SetAsOpen(doorData.OpenInwards);
            }
            else
            {
                doorData.Door.SetAsClosed();
            }
        }

        for (int i = 0; i < connectedPlatforms.Length; i++)
        {
            // Either move connected platforms back to start or pause their movement depending on set behaviour type
            PlatformPuzzleData platformData = connectedPlatforms[i];
            if (platformData.Behaviour == PlatformButtonBehaviour.TriggerOnPress)
            {
                platformData.Platform.StartMovingBackwards();
            }
            else if(platformData.Behaviour == PlatformButtonBehaviour.PauseOnPress)
            {
                platformData.Platform.Paused = false;
            }
        }

        // Play the 'button off' sound if not loading data (prevents sounds playing while setting up objecs on load)
        if (!SaveLoadManager.Instance.LoadingSceneData)
        {
            AudioManager.Instance.PlaySoundEffect3D("puzzleButtonOff", transform.position);
        }
    }

    public void SetToStandardColour()
    {
        // Sets the button's emission colour to the standard colour
        buttonMeshRenderer.material.SetColor("_EmissionColor", standardEmissionColour);
    }

    public void SetToPressedColour()
    {
        // Sets the button's emission colour to the pressed colour
        buttonMeshRenderer.material.SetColor("_EmissionColor", pressedEmissionColour);
    }

    public void SetMaterialColour(Color colour)
    {
        // Sets the button's emission colour to the given colour
        buttonMeshRenderer.material.SetColor("_EmissionColor", colour);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (playerCanActivate && other.gameObject.CompareTag("Player"))
        {
            // The player entered the trigger and can press the button
            playerIsColliding = true;
        }
        else if (movableObjCanActivate && other.CompareTag("MovableObj"))
        {
            // Check if the movable object is not required to be heavy, or is required and is heavy
            if(!requiresHeavyObject || (requiresHeavyObject && other.GetComponent<MovableObject>().IsLargeObject))
            {
                // A movable object entered the trigger and can press the button
                movableObjIsColliding = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (playerCanActivate && other.gameObject.CompareTag("Player"))
        {
            // The player exited the trigger and was pressing the button
            playerIsColliding = false;
        }
        else if (movableObjCanActivate && other.CompareTag("MovableObj"))
        {
            // A movable object exited the trigger and was pressing the button
            movableObjIsColliding = false;
        }
    }

    protected override void ShowInteractTooltip(string overridePressEText = null)
    {
        // Override for the ShowInteractTooltip function, forcing it to show 'To Activate'
        //   title text instead of the standard 'Press E to Interact' message
        base.ShowInteractTooltip("To Activate");
    }
}


// DoorPuzzleData defines how button presses will affect a certain door
[System.Serializable]
public struct DoorPuzzleData
{
    public DoorMain Door;           // The door to be affected by the puzzle element
    public bool     OpenInwards;    // Whether the door will open inwards or outwards
    public bool     OpenByDefault;  // Whether the door's default state will be open or closed
}

// DoorPuzzleData defines how button presses will affect a certain moving platform
[System.Serializable]
public struct PlatformPuzzleData
{
    public MovingPlatform           Platform;   // The door to be affected by the puzzle element
    public PlatformButtonBehaviour  Behaviour;  // How the platform will respond to the button being pressed/released
}