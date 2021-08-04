using UnityEngine;

// ||=======================================================================||
// || PuzzleButton: A button that can be activated by the player or by      ||
// ||   dragging an object onto it. Can trigger Doors and MovingPlatforms.  ||
// ||=======================================================================||
// || Used on prefab: Joe/PuzzleElements/PuzzleButton                       ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
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
    [SerializeField] private BoxCollider            buttonCollider;

    [Header("Appearance")]

    [SerializeField] private MeshRenderer           buttonMeshRenderer;
    [SerializeField] private Material               baseButtonMaterial;

    [SerializeField]
    private Color buttonBaseColour;

    [SerializeField] [ColorUsage(true, true)]
    private Color standardEmissionColour;

    [SerializeField] [ColorUsage(true, true)]
    private Color pressedEmissionColour;

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

        buttonMeshRenderer.material = new Material(baseButtonMaterial);
        buttonMeshRenderer.material.SetColor("_BaseColor", buttonBaseColour);

        // Ensure all connected doors are in the released button state by default
        ButtonReleasedEvents();

        if(sequence != null)
        {
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
        enableTooltip = false;
        HideInteractTooltip();

        buttonCollider.enabled = false;

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

        if (!SaveLoadManager.Instance.LoadingSceneData)
        {
            AudioManager.Instance.PlaySoundEffect3D("puzzleButtonOn", transform.position);
        }
    }

    private void ButtonReleasedEvents()
    {
        enableTooltip = true;

        buttonCollider.enabled = true;

        if(sequence == null)
        {
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

        if(!SaveLoadManager.Instance.LoadingSceneData)
        {
            AudioManager.Instance.PlaySoundEffect3D("puzzleButtonOff", transform.position);
        }
    }

    public void SetToStandardColour()
    {
        buttonMeshRenderer.material.SetColor("_EmissionColor", standardEmissionColour);
    }

    public void SetToPressedColour()
    {
        buttonMeshRenderer.material.SetColor("_EmissionColor", pressedEmissionColour);
    }

    public void SetMaterialColour(Color colour)
    {
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