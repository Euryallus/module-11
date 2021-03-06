using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Main author:         Hugo Bailey
// Additional author:   Joe Allen (see comments)
// Description:         Used by the Grab ability (currently tied to the Pickaxe) to pick up & move physics objects around
// Development window:  Prototype phase & production phase
// Inherits from:       InteractableWithOutline & IPersistentSceneObject

[RequireComponent(typeof(Rigidbody))]
public class MovableObject : InteractableWithOutline, IPersistentSceneObject
{
    [HideInInspector]   public bool isHeld;         // Flags if player is currently holding the object
    [SerializeField]    private Transform target;   // Ref. to transform of the player's ""hand"" - position the object moves towards
    [SerializeField]    private Rigidbody rb;       // Ref. to own RigidBody component

    [SerializeField]    private float jointSpring = 5f;                     // Saves default "spring" value for spring joint
    [SerializeField]    private float jointDamper = 10f;                    // Dampen factor of Spring joing
    [SerializeField]    private float movementDampen = 0.3f;                // Max. movement speed of object when moving towards hand pos.
                        private Rigidbody handTarget;                       // Rigidbody of target (attached to player "hand")
                        private SpringJoint joint;                          // Ref. to own spring joint
                        private Vector3 currentVelocity = Vector3.zero;     // Current velocity of object (used & altered by Vector3.SmoothDamp)

    [SerializeField]    private Transform hand;                             // Ref. to position of player's ""hand""

    [Header("Object type (if is large, can only be held w/ upgraded Grab)")]
    [SerializeField] private bool isLargeObject = false;

    private GameObject player;          // Ref. to player
    private bool canPickUp;             // Flags is object can be picked up
    private Material originalMat;       // Ref. to objects original material
    private GrabAbility grabAbility;    // Ref. to GrabAbility main component
    private Vector3 startPosition;      // The position of the object on scene load, used to generate a unique position id

    [SerializeField] private bool isProjectile = false;

    // Added by Joe, the tooltip text to show be default when the object can be picked up (as originally set in the inspector)
    private string defaultTooltipNameText;

    public bool IsLargeObject { get { return isLargeObject; } }

    protected void Awake()
    {
        // Flags where object started (allows respawn if lost in lava or water)
        startPosition = transform.position;
    }

    protected override void Start()
    {
        base.Start();

        // Subscribe to save/load events so the fire monument's data will be saved/loaded with the game
        SaveLoadManager.Instance.SubscribeSceneSaveLoadEvents(OnSceneSave, OnSceneLoadSetup, OnSceneLoadConfigure);


        isHeld = false;

        // Prevents object moving unless the player has picked it up at least once (e.g. large objects can't just be pushed to button, must be picked up)
        rb = gameObject.GetComponent<Rigidbody>();
        rb.isKinematic = true;

        defaultTooltipNameText = tooltipNameText;

        if (!isProjectile)
        {
            // Saves originalMat as material used on start
            originalMat = gameObject.GetComponent<MeshRenderer>().material;
        }
        // Saves ref. to player
        player = GameObject.FindGameObjectWithTag("Player");
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        // Unsubscribe from save/load events to prevent null ref errors if the object is destroyed
        SaveLoadManager.Instance.UnsubscribeSceneSaveLoadEvents(OnSceneSave, OnSceneLoadSetup, OnSceneLoadConfigure);
    }

    private void FixedUpdate()
    {
        // If the player is currently holding the object, move towards the players hand position
        if (isHeld)
        {
            // Originally was using Vector3.Lerp but this caused issues with collisions being ignored
            // Used https://answers.unity.com/questions/510853/how-to-keep-objects-from-passing-through-colliders.html as ref. for fix

            transform.position = Vector3.SmoothDamp(transform.position, hand.position, ref currentVelocity, movementDampen);
        }
    }

    protected override void Update()
    {
        // Finds "hand" if hand hasnt been assigned yet
        if(hand == null)
        {
            GameObject playerHandGameObj = GameObject.FindGameObjectWithTag("PlayerHand");
            if(playerHandGameObj != null)
            {
                hand = playerHandGameObj.transform;
            }
        }


        // Finds "grab ability" ref is grab hasnt been assigned yet
        if(grabAbility == null)
        {
            grabAbility = PlayerInstance.ActivePlayer.gameObject.GetComponent<GrabAbility>();
        }

        // Input detection for "throwing" object
        if (isHeld && Input.GetKeyDown(KeyCode.Mouse0))
        {
            ThrowObject(transform.position - GameObject.FindGameObjectWithTag("Player").transform.position);
        }

        // Input detection for "dropping" object
        if (isHeld && Input.GetKeyDown(KeyCode.Mouse1))
        {
            DropObject();
        }

        // Only show the interaction tooltip when the object is not held
        enableTooltip = !isHeld;

        base.Update();
    }

    // Sets target position to players hand, turns off grav & sets isHeld to true
    public override void Interact()
    {
        UpdatePickUpStatus();

        
        if(!isHeld && canPickUp)
        {
            base.Interact();

            // Switches object to be affected by physics when first picked up
            rb.isKinematic = false;
            handTarget = hand.transform.gameObject.GetComponent<Rigidbody>();

            // Childs object to "hand" of player 
            transform.parent = hand.transform;//GameObject.FindGameObjectWithTag("Player").transform;

            if(!isProjectile)
            {
                // Switches material to transparent (allows larger objects to not obscure play)
                gameObject.GetComponent<MeshRenderer>().material = player.GetComponent<GrabAbility>().objectPlacementMat;
            }

            // Sets up spring joint - allows object to "spring" towards player hand
            joint = gameObject.AddComponent<SpringJoint>();
            joint.spring = jointSpring;
            joint.damper = jointDamper;

            joint.connectedBody = handTarget;

            // Stops object rotating when held
            rb.constraints = RigidbodyConstraints.FreezeRotation;


            // ""Resets"" velocity when object is held to avoid issues when moving around
            rb.velocity = Vector3.zero;
            // Sets target to the player's "hand" and disables gravity on the object, flags isHeld
            target = hand;
            rb.useGravity = false;
            isHeld = true;

            // Displays carry tooltip (shows key bindings to place & throw object)
            ShowCarryingTooltip();

            // Flags to grab ability script that object has been picked up - resets charge
            grabAbility.SetChargeAmount(1.0f);
        }
    }
    
    // Sets item down where it is and re-enables grav.
    public void DropObject()
    {
        StopHoldingObject();

        HideCarryingTooltip();
    }

    // Throws object in the direction the player is facing, re-enables grav etc.
    public void ThrowObject(Vector3 direction)
    {
        // Removes all restraints attached
        StopHoldingObject();
        
        // Adds force in direction player is facing
        rb.AddForce(direction.normalized * 300);
        // Hides tooltips
        HideCarryingTooltip();
    }

    private void StopHoldingObject()
    {
        // Removes any physics contraints
        rb.constraints = RigidbodyConstraints.None;

        // Unchilds from players hand & removes spring joint component, re-enables gravilty
        transform.parent = null;
        Destroy(joint);
        isHeld = false;
        rb.useGravity = true;

        if(!isProjectile)
        {
            // Returns mesh to original material
            gameObject.GetComponent<MeshRenderer>().material = originalMat;
        }

        
        // Resets charge on grab ability
        grabAbility.SetChargeAmount(0.0f);


        // Added by Joe
        if(hoveringInRange)
        {
            grabAbility.SetCooldownAmount(1.0f);
        }

        // Move the GameObject back to the active scene. Since it was made a child of the player when picked up,
        //   it was moved to the DontDestroyOnLoad scene, and as such needs to be moved back.
        SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());
    }

    // All below added by Joe
    public void MoveToStartPosition()
    {
        transform.position = startPosition;
        transform.rotation = Quaternion.identity;
    }

    public override void StartHoverInRange()
    {
        base.StartHoverInRange();

        UpdatePickUpStatus();

        if(canPickUp)
        {
            grabAbility.SetCooldownAmount(1.0f);
        }
        else
        {
            // Don't show an outline on hover if the object cannot be picked up
            outline.enabled = false;
        }
    }

    public override void EndHoverInRange()
    {
        base.EndHoverInRange();

        if(!isHeld)
        {
            grabAbility.SetCooldownAmount(0.0f);
        }
    }

    // Shows a tooltip with info about how to drop/throw an object when carrying it
    private void ShowCarryingTooltip()
    {
        // Override default tooltip behaviour so it always shows while carrying
        //   an object, regardless of whether the mouse is over it
        overrideTooltipBehaviour = true;

        tooltipNameText = "Right Click: Drop";

        ShowInteractTooltip("Left Click: Throw");
    }

    // Added by Joe: Opposite of the above function, reverts to standard tooltip behaviour
    private void HideCarryingTooltip()
    {
        overrideTooltipBehaviour = false;

        tooltipNameText = defaultTooltipNameText;

        HideInteractTooltip();
    }

    

    // Added by Joe, determines whether the object can be picked up based on
    //   the unlock status and upgrade level of the grab ability
    private void UpdatePickUpStatus()
    {
        if(PlayerAbility.AbilityIsUnlocked(PlayerAbilityType.Grab))
        {
            if(!isLargeObject || PlayerAbility.GetAbilityUpgradeLevel(PlayerAbilityType.Grab) > 1)
            {
                // This is a small object, or the grab ability was upgraded to level 2, object can be picked up
                canPickUp = true;

                tooltipNameText = defaultTooltipNameText;
                showPressETooltipText = true;
            }
            else
            {
                // This is a large object and the grab ability has not been upgraded, object cannot be picked up
                canPickUp = false;

                tooltipNameText = "Requires grab upgrade";
                showPressETooltipText = false;
            }
        }
        else
        {
            // Grab ability is not unlocked, object cannot be picked up
            canPickUp = false;

            tooltipNameText = "Requires grab ability";
            showPressETooltipText = false;
        }
    }

    // Save the object's position and rotation
    public void OnSceneSave(SaveData saveData)
    {
        // Save position and euler rotation values to a float array
        saveData.AddData(GetSaveId(), new float[6] { transform.position.x, transform.position.y, transform.position.z,
                                                transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z });

        Debug.Log("Saving MovableObject transform for " + GetSaveId(), gameObject); 
    }

    // Load the objects position and rotation
    public void OnSceneLoadSetup(SaveData saveData)
    {
        float[] transformVals = saveData.GetData<float[]>(GetSaveId(), out bool loadSuccess);

        if(loadSuccess)
        {
            // Set the object's transfrom and rotation from loaded values.
            transform.position = new Vector3(transformVals[0], transformVals[1], transformVals[2]);
            transform.rotation = Quaternion.Euler(transformVals[3], transformVals[4], transformVals[5]);
        }

        Debug.Log("Loading MovableObject transform for " + GetSaveId());
    }

    public void OnSceneLoadConfigure(SaveData saveData) { } // Nothing to configure

    private string GetUniquePositionId()
    {
        return startPosition.x + "_" + startPosition.y + "_" + startPosition.z;
    }

    private string GetSaveId()
    {
        return "movableObjTransform_" + gameObject.name + "_" + GetUniquePositionId(); ;
    }
}