using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Used by the Grab ability (currently tied to the Pickaxe) to pick up & move physics objects around
// Development window:  Prototype phase
// Inherits from:       MonoBehaviour

[RequireComponent(typeof(Rigidbody))]
public class MovableObject : InteractableWithOutline
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

    // Added by Joe, the tooltip text to show be default when the object can be picked up (as originally set in the inspector)
    private string defaultTooltipNameText;

    //protected void Awake()
    //{
    //    if (!isLargeObject)
    //    {
    //        canPickUp = true;
    //    }
    //}

    protected override void Start()
    {
        base.Start();

        isHeld = false;
        rb = gameObject.GetComponent<Rigidbody>();

        hand = GameObject.FindGameObjectWithTag("PlayerHand").transform;

        defaultTooltipNameText = tooltipNameText;

        //if(!canPickUp)
        //{
        //    gameObject.GetComponent<Outline>().OutlineWidth = 0f;
        //}
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

        // If the spring joint has broken, let go of object
        //if(!gameObject.GetComponent<SpringJoint>())
        //{
        //    transform.parent = null;
        //    isHeld = false;
        //    rb.useGravity = true;
        //}
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
        if(isHeld && Input.GetKeyDown(KeyCode.Mouse0))
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
        }
    }

    // Sets item down where it is and re-enables grav.
    public void DropObject()
    {
        rb.constraints = RigidbodyConstraints.None;

        transform.parent = null;
        Destroy(joint);
        isHeld = false;
        rb.useGravity = true;
    }

    // Throws object in the direction the player is facing, re-enables grav etc.
    public void ThrowObject(Vector3 direction)
    {
        rb.constraints = RigidbodyConstraints.None;

        transform.parent = null;
        Destroy(joint);
        isHeld = false;
        rb.useGravity = true;
        rb.AddForce(direction.normalized * 300);
    }

    //public void EnablePickUp()
    //{
    //    gameObject.GetComponent<Outline>().OutlineWidth = 5f;
    //    canPickUp = true;
    //}


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

                tooltipNameText = "Requires grab ability upgrade";
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
}