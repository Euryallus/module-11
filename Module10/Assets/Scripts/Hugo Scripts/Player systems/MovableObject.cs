using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Used by the Grab ability (currently tied to the Pickaxe) to pick up & move physics objects around
// Development window:  Prototype phase
// Inherits from:       MonoBehaviour

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

    [SerializeField]    private Transform hand;

    [Header("Object type (if is large, can only be held w/ upgraded Grab)")]
    [SerializeField] private bool isLargeObject = false;

    private bool canPickUp;

    private Vector3 startPosition; // The position of the object on scene load, used to generate a unique position id

    // Added by Joe, the tooltip text to show be default when the object can be picked up (as originally set in the inspector)
    private string defaultTooltipNameText;

    protected void Awake()
    {
        startPosition = transform.position;
    }

    protected override void Start()
    {
        base.Start();

        // Subscribe to save/load events so the fire monument's data will be saved/loaded with the game
        SaveLoadManager.Instance.SubscribeSceneSaveLoadEvents(OnSceneSave, OnSceneLoadSetup, OnSceneLoadConfigure);

        isHeld = false;
        rb = gameObject.GetComponent<Rigidbody>();

        defaultTooltipNameText = tooltipNameText;
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

    public void MoveToStartPosition()
    {
        transform.position = startPosition;
        transform.rotation = Quaternion.identity;
    }

    public override void StartHoverInRange()
    {
        base.StartHoverInRange();

        UpdatePickUpStatus();

        // Don't show an outline on hover if the object cannot be picked up
        if(!canPickUp)
        {
            outline.enabled = false;
        }
    }

    protected override void Update()
    {
        if(hand == null)
        {
            GameObject playerHandGameObj = GameObject.FindGameObjectWithTag("PlayerHand");
            if(playerHandGameObj != null)
            {
                hand = playerHandGameObj.transform;
            }
        }

        if (isHeld && Input.GetKeyDown(KeyCode.Mouse0))
        {
            ThrowObject(transform.position - GameObject.FindGameObjectWithTag("Player").transform.position);
        }

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
            handTarget = hand.transform.gameObject.GetComponent<Rigidbody>();

            transform.parent = hand.transform;//GameObject.FindGameObjectWithTag("Player").transform;

            joint = gameObject.AddComponent<SpringJoint>();
            joint.spring = jointSpring;
            joint.damper = jointDamper;

            joint.connectedBody = handTarget;

            rb.constraints = RigidbodyConstraints.FreezeRotation;


            // ""Resets"" velocity when object is held to avoid issues when moving around
            rb.velocity = Vector3.zero;
            // Sets target to the player's "hand" and disables gravity on the object, flags isHeld
            target = hand;
            rb.useGravity = false;
            isHeld = true;

            ShowCarryingTooltip();
        }
    }

    // Added by Joe: Shows a tooltip with info about how to drop/throw an object when carrying it
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

    // Sets item down where it is and re-enables grav.
    public void DropObject()
    {
        StopHoldingObject();

        HideCarryingTooltip();
    }

    // Throws object in the direction the player is facing, re-enables grav etc.
    public void ThrowObject(Vector3 direction)
    {
        StopHoldingObject();
        
        rb.AddForce(direction.normalized * 300);

        HideCarryingTooltip();
    }

    private void StopHoldingObject()
    {
        rb.constraints = RigidbodyConstraints.None;

        transform.parent = null;
        Destroy(joint);
        isHeld = false;
        rb.useGravity = true;

        // Added by Joe
        // Move the GameObject back to the active scene. Since it was made a child of the player when picked up,
        //   it was moved to the DontDestroyOnLoad scene, and as such needs to be moved back.
        SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());
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
        string saveId = "movableObjTransform_" + GetUniquePositionId();

        // Save position and euler rotation values to a float array
        saveData.AddData(saveId, new float[6] { transform.position.x, transform.position.y, transform.position.z,
                                                transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z });

        Debug.Log("Saving MovableObject transform for " + saveId); 
    }

    // Load the objects position and rotation
    public void OnSceneLoadSetup(SaveData saveData)
    {
        string saveId = "movableObjTransform_" + GetUniquePositionId();

        float[] transformVals = saveData.GetData<float[]>(saveId, out bool loadSuccess);

        if(loadSuccess)
        {
            // Set the object's transfrom and rotation from loaded values.
            //   Adding a small amount to the y value to prevent intersections causing unpredictable behaviour
            transform.position = new Vector3(transformVals[0], transformVals[1] + 0.1f, transformVals[2]);
            transform.rotation = Quaternion.Euler(transformVals[3], transformVals[4], transformVals[5]);
        }

        Debug.Log("Loading MovableObject transform for " + saveId);
    }

    public void OnSceneLoadConfigure(SaveData saveData) { } // Nothing to configure

    private string GetUniquePositionId()
    {
        return startPosition.x + "_" + startPosition.y + "_" + startPosition.z;
    }
}