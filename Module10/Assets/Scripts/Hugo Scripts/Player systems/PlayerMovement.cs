using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Handles player movement & movement states 
// Development window:  Prototype phase
// Inherits from:       MonoBehaviour

public class PlayerMovement : MonoBehaviour
{
    [Header("General refs")]

    [SerializeField]    private GameObject playerCamera;    // Reference to player camera (used for forward vect)
    [SerializeField]    private Volume postProcessing;      // Post processing volume (used for water post processing)

    [Header("Player movement speeds")]

    [SerializeField]    [Range(1, 10)]    private float walkSpeed           = 5f;   // Default walk speed
    [SerializeField]    [Range(1, 10)]    private float runSpeed            = 8f;   // Default run speed
    [SerializeField]    [Range(1, 10)]    private float crouchSpeed         = 3f;   // Default crouch speed
    [SerializeField]    [Range(1, 20)]    private float defaultGlideSpeed   = 2f;   // Default glide speed
    [SerializeField]    [Range(1, 10)]    private float swimSpeed           = 5f;   // Default swim speed
    [SerializeField]    [Range(1, 30)]    private float jumpVelocity        = 3f;   // Default jump velocity
    [SerializeField]    [Range(1, 150)]   private float TerminalVelocity    = 100f; // Max value for VelocityY

    private Dictionary<MovementStates, float> speedMap = new Dictionary<MovementStates, float> { }; // Dictionary mapping movement states to speeds

    [Header("Gravity (normal & glider)")]
    [SerializeField]    [Range(0.5f, 20)]    private float gravity         = 9.81f; // Default downward force

    [Header("Mouse input")]
    [SerializeField]    [Range(0.5f, 8)]    private float mouseSensitivity = 400f;  // Sensitivity of mouse

    [Header("Glider elements")]

    [Tooltip("Rate at which player falls when gliding, default is 9.81 (normal grav.)")]
    [SerializeField]    [Range(0.5f, 12)] private float gliderFallRate                  = 3f;   // Default rate player falls when gliding 
    [SerializeField]    [Range(0.5f, 4)]  private float gliderSensitivity               = 2.0f; // Turn sensitivity of glideer
    [SerializeField]    [Range(0.01f, 1)] private float gliderTiltAmount                = 0.5f; // Amount glider tilts when in use (clamps after [x] amount)
    [SerializeField]    [Range(1, 10)]    private float gliderOpenDistanceFromGround    = 5.0f; // Distance from the ground player must be to open glider


    private float mouseX;           // x component of raw mouse movement
    private float mouseY;           // y component of raw mouse movement
    private float rotateY = 0;      // y Rotation of camera (gets clamped to ~85 degrees to avoid total spinning)
    private float inputX;           // x component of raw keyboard input
    private float inputY;           // y component of raw keyboard input
    private float velocityY = 0;    // Downward velocity acting on playerz

    private CharacterController controller; // Ref to player's character controller

    private DepthOfField dof;   // Ref. to Depth of Field post processing effect
    private Vignette v;         // Ref. to  Vignette post processing effect

    private Vector3 moveTo;         // Vector3 position player moves towards (calculated each frame & passed to CharacterController via Move() func)
    [SerializeField] private Vector2 glideVelocity;  // x and z components of glider movement (y comp. is calculated seperately)

    private bool inWater    = false;    // Flags if player is currently underwater
    private bool canMove    = true;     // Flags if player is able to move (changed to prevent moving during dialogue etc.)
    private bool canGlide   = false;    // Flags if player is able to glide (if > [gliderOpenDistanceFromGround] meters off ground)

    private InventoryPanel inventory;   // Ref. to player inventory
    public Item glider;                 // Ref. to glider object

    public enum MovementStates      // Possible states player can be in when moving
    {
        walk,
        run,
        crouch,
        glide,
        dive,
        swim,
        ladder
    }

    private enum CrouchState    // Possible states of "crouching" player can be in
    {
        standing,
        gettingDown,
        crouched,
        gettingUp
    }

    
                        private CrouchState currentCrouchState;         // Saves player's current crouch state
    [HideInInspector]   public MovementStates currentMovementState;     // Saves player's current movement state



    void Start()
    {
        // Assigns references to components
        controller = gameObject.GetComponent<CharacterController>();
        inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<InventoryPanel>();

        // Locks cursor to centre of screen
        Cursor.lockState = CursorLockMode.Locked;

        // Sets movement & crouch states to their "default"
        currentCrouchState = CrouchState.standing;
        currentMovementState = MovementStates.walk;

        // Maps speed float to movement enum
        speedMap[MovementStates.walk]   = walkSpeed;
        speedMap[MovementStates.run]    = runSpeed;
        speedMap[MovementStates.crouch] = crouchSpeed;
        speedMap[MovementStates.glide]  = defaultGlideSpeed;
        speedMap[MovementStates.dive]   = swimSpeed;
        speedMap[MovementStates.swim]   = swimSpeed;
        speedMap[MovementStates.ladder] = walkSpeed;

        // Ensures glider begins with 0 velocity
        glideVelocity = new Vector2(0, 0);

        // Attempts to gets post processing effects (water effect)
        postProcessing.profile.TryGet<Vignette>(out v);
        postProcessing.profile.TryGet<DepthOfField>(out dof);
    }

    void Update()
    {
       
        // Checks if player is [x] m above ground or not
        if (Physics.Raycast(transform.position, -transform.up, gliderOpenDistanceFromGround))
        {
            canGlide = false;
        }
        else
        {
            canGlide = true;
        }

        // Switches state to "run" when shift is pressed
        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            if(currentMovementState == MovementStates.walk && controller.isGrounded)
            {
                currentMovementState = MovementStates.run;
            }

        }

        // Switches state to "walk" when shift is released
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            if(currentMovementState == MovementStates.run)
            {
                currentMovementState = MovementStates.walk;
            }
        }

        // Toggles crouch
        if(Input.GetKeyDown(KeyCode.LeftControl))
        {
                currentMovementState = (currentMovementState == MovementStates.crouch ? MovementStates.walk : MovementStates.crouch);
        }

        // Crouch grow / shrink movement
        switch (currentCrouchState)
        {
            // Kick starts crouch (movement state is crouch but crouch state is "standing"
            case CrouchState.standing:
                if(currentMovementState == MovementStates.crouch)
                {
                    currentCrouchState = CrouchState.gettingDown;
                }
                break;

            case CrouchState.gettingDown:

                // Lerps Y scale over 0.3 seconds to make character crouch
                gameObject.transform.localScale = Vector3.Lerp(gameObject.transform.localScale, new Vector3(1, 0.6f, 1), 3 * Time.deltaTime);

                // If scale has reached "crouch" scale, set state to crouched
                if(gameObject.transform.localScale.y - 0.6f <= 0.01f)
                {
                    gameObject.transform.localScale = new Vector3(1, 0.6f, 1);
                    currentCrouchState = CrouchState.crouched;
                }

                // If player has stopped crouching, switch to "getting up" state
                if (currentMovementState == MovementStates.walk)
                {
                    currentCrouchState = CrouchState.gettingUp;
                }

                break;


            case CrouchState.crouched:

                // If player has stopped crouching, switch to "getting up" state
                if (currentMovementState == MovementStates.walk)
                {
                    currentCrouchState = CrouchState.gettingUp;
                }
                break;

            case CrouchState.gettingUp:

                // Lerps Y scale over 0.3 seconds to make character stand up
                gameObject.transform.localScale = Vector3.Lerp(gameObject.transform.localScale, Vector3.one, 3 * Time.deltaTime);

                if (1 - gameObject.transform.localScale.y <= 0.01f)
                {
                    gameObject.transform.localScale = Vector3.one;
                    currentCrouchState = CrouchState.standing;
                }
                break;
            default:
                break;
        }

        // Layer mask ("exposes" only 4th layer, water layer)
        int mask = 1 << 4;

        // Raycast upwards from player on water layer exposes if player is underwater
        if (Physics.Raycast(transform.position - new Vector3(0,0.5f,0), transform.up, 100f, mask ))
        {
            // If hits water layer above & mode isnt swimming, enable post processing effects
            if(currentMovementState != MovementStates.dive)
            {
                dof.active = true;
                v.active = true;
            }

            // Added by Joe: plays a sound when the player first enters water and starts looping underwater sound
            if (!inWater)
            {
                AudioManager.Instance.PlaySoundEffect2D("splash");
                AudioManager.Instance.PlayLoopingSoundEffect("underwaterLoop", "playerInWater");
            }

            // Flags water bool
            inWater = true;
            // Sets current movement mode to diving
            currentMovementState = MovementStates.dive;
        }
        else 
        {
            // If doesnt hit but currently in water
            if (inWater == true)
            {
                // Disable post processing effects
                dof.active = false;
                v.active = false;

                // Change state to "swim" on top of water
                currentMovementState = MovementStates.swim;
            }
        }

        if(canMove)
        {
            // Raw input from mouse / keyboard (X & Y)
            moveTo = new Vector3(0, 0, 0);

            mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;// * Time.deltaTime;
            mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;// * Time.deltaTime;

            rotateY -= mouseY;
            rotateY = Mathf.Clamp(rotateY, -75f, 85f);

            inputX = Input.GetAxis("Horizontal");
            inputY = Input.GetAxis("Vertical");

            // Camera & capsule rotation
            transform.Rotate(Vector3.up * mouseX);
            playerCamera.transform.localRotation = Quaternion.Euler(rotateY, 0, 0f);

            // Pos. to move to (by default it's current pos + raw input)
            moveTo = transform.right * inputX + transform.forward * inputY;

            // Switch according to movement state
            switch(currentMovementState)
            {
                case MovementStates.swim:
                    // If case = swimming but player is grounded, player is now walking
                    if(controller.isGrounded)
                    {
                        // Added by Joe: plays a sound when the player leaves water and stops looping underwater sound
                        if (inWater)
                        {
                            AudioManager.Instance.PlaySoundEffect2D("waterExit");
                            AudioManager.Instance.StopLoopingSoundEffect("playerInWater");
                        }

                        currentMovementState = MovementStates.walk;
                        inWater = false;
                    }

                    // Sdds gravity 
                    velocityY -= gravity * gravity * Time.deltaTime;
                    
                    // If space is pressed use that as upward velocity rather than cam. forward Y component
                    if(Input.GetKeyDown(KeyCode.Space))
                    {
                        velocityY = jumpVelocity;
                    }

                    break;

                case MovementStates.ladder:

                    // Switches movement controls so only direction of movement is up & down (determined by inputY)
                    moveTo = new Vector3(0, 0, 0);

                    velocityY = inputY * 2f;

                    break;

                case MovementStates.glide:
                    // Switches to walking if player hits ground while gliding
                    if (controller.isGrounded)
                    {
                        currentMovementState = MovementStates.walk;
                        glideVelocity = new Vector2(0, 0);
                        break;
                    }

                    // 
                    if (inputY == 0)
                    {
                        glideVelocity.y -= glideVelocity.y * 0.25f;
                    }
                    // If y input, increase forward velocity
                    else
                    {
                        glideVelocity.y += inputY * (gliderSensitivity / 2) * Time.deltaTime;
                    }

                    // Checks if player is approaching the ground
                    if (Physics.Raycast(transform.position, -transform.up, gliderOpenDistanceFromGround * 2))
                    {
                        if (glideVelocity.x > 0.01f || glideVelocity.x < -0.01f)
                        {
                            // Lerps x velocity & tilt back towards 0 to avoid issues w/ landing
                            if (glideVelocity.x < 0)
                            {
                                glideVelocity.x += 3 * Time.deltaTime;
                            }
                            else
                            {
                                glideVelocity.x -= 3 * Time.deltaTime;
                            }
                        }

                    }
                    else
                    {
                        // If player isn't headed for ground & no X axis input, lerp towards 0 l / r movement 
                        if (inputX == 0)
                        {
                            if (glideVelocity.x > 0.01f || glideVelocity.x < -0.01f)
                            {
                                if (glideVelocity.x < 0)
                                {
                                    glideVelocity.x += 3 * Time.deltaTime;
                                }
                                else
                                {
                                    glideVelocity.x -= 3 * Time.deltaTime;
                                }
                            }
                        }
                        else
                        {
                            // If there is X axis input, add it to the gliderVelocity
                            glideVelocity.x += inputX * gliderSensitivity * Time.deltaTime;
                        }
                    }

                    // Creates target rotation for player camera using current rotation as a benchmark
                    Quaternion target = playerCamera.transform.localRotation;
                    // Adjusts target rotation by [glide velocity]
                    target.z = Mathf.Deg2Rad * -glideVelocity.x;

                    // If target is not yet fully "tilted" at clamped [gliderTiltAmount], apply new rotation to camera
                    if (target.z < gliderTiltAmount && target.z > -gliderTiltAmount)
                    {
                        playerCamera.transform.localRotation = target;

                    }
                    else
                    {
                        // If target rotation is close enough to [gliderTiltAmount], clamp to + or - [gliderTiltAmount] 
                        if (target.z < 0)
                        {
                            target.z = -gliderTiltAmount;

                        }
                        else
                        {
                            target.z = gliderTiltAmount;
                        }

                        // Apply rotation to camera
                        playerCamera.transform.localRotation = target;
                    }

                    // Clamps X velovity to ( speedMap[currentMovementState] / 2 ) in either direction
                    glideVelocity.x = Mathf.Clamp(glideVelocity.x, -speedMap[currentMovementState] / 2, speedMap[currentMovementState] / 2);

                    //Clamps Z velocity (stored as Y component of glider velocity) to ( speedMap[currentMovementState] )
                    glideVelocity.y = Mathf.Clamp(glideVelocity.y, -0.2f, speedMap[currentMovementState]);

                    // Calculate new MoveTo vect based on gliderVelocity
                    moveTo = transform.right * glideVelocity.x + transform.forward * glideVelocity.y;

                    // Adjust downward velocity based on gliderFallRate
                    velocityY -= gliderFallRate * gliderFallRate * Time.deltaTime;

                    break;

                case MovementStates.dive:
                    // Adjusts moveTo vect based on keyboard input
                    moveTo = transform.right * inputX + transform.forward * inputY;

                    // Downward velocity decided by camera forward vect (can look upwards to swim upwards)
                    velocityY = playerCamera.transform.forward.y * 4f * inputY;

                    // Downward velocity also changable using SPACE key
                    if (Input.GetKey(KeyCode.Space))
                    {
                        velocityY = 4f;
                    }
                    break;

                case MovementStates.run:
                case MovementStates.walk:
                case MovementStates.crouch:
                    // Default behaviour for walk, run and crouch 

                    // If player's in the air, increase downwards velocity (up to terminal velocity)
                    if (!controller.isGrounded)
                    {
                        if(velocityY < TerminalVelocity)
                        {
                            velocityY -= gravity * gravity * Time.deltaTime;
                        }
                    }

                    break; 
            }         

            // Checks if player attempts to jump / glide
            if (Input.GetKeyDown(KeyCode.Space) && currentMovementState != MovementStates.crouch && currentMovementState != MovementStates.swim)
            {
                // If player is grounded, first go to jumping state (add jump velocity)
                if (controller.isGrounded)
                {
                    velocityY = jumpVelocity;

                    // Play a jump sound
                    AudioManager.Instance.PlaySoundEffect2D("jump");
                }
                // If player is already in the air, isn't already gliding but is far enough off ground to glide, start gliding
                else if (currentMovementState != MovementStates.glide && canGlide)
                {
                    currentMovementState = MovementStates.glide;

                    glideVelocity = new Vector2(inputX, inputY).normalized;

                    velocityY = 0.1f;
                }
                // If player is already gliding & hits space again, close glider
                else if(currentMovementState == MovementStates.glide)
                {
                    currentMovementState = MovementStates.walk;
                }
                
            }

            // Normalises movement if mag. is over 1
            if (moveTo.magnitude > 1)
            {
                moveTo.Normalize();
            }

            // Adjusts moveTo vector based on current state's speed
            Vector3 moveVect = moveTo * speedMap[currentMovementState];

            // Finally assins Y velocity to moveVect
            moveVect.y = velocityY;

            // Tells controller to move
            controller.Move(moveVect * Time.deltaTime); //applies movement to player
        }
    }

    // Returns if player is on the ground
    public bool PlayerIsGrounded()
    {
        return controller.isGrounded;
    }

    // Returns velocity of the character controller, or 0,0,0 if it's not enabled
    public Vector3 GetVelocity()
    {
        if(controller.enabled)
        {
            return controller.velocity;
        }
        else
        {
            return Vector3.zero;
        }
    }

    // Changes velocityY to velocity passed as param
    public void SetJumpVelocity(float velocity)
    {
        if(controller.isGrounded)
        {
            velocityY = velocity;
        }
    }

    // Prevents player from moving (used during dialogue / menus)
    public void StopMoving()
    {
        canMove = false;
    }

    // Flags player as able to move again
    public void StartMoving()
    {
        canMove = true;
    }

    // Returns if player can move or not
    public bool GetCanMove()
    {
        return canMove;
    }

    // Checks if player is currently moving
    public bool PlayerIsMoving()
    {
        if(canMove && (Mathf.Abs(controller.velocity.x) > 1.0f || Mathf.Abs(controller.velocity.z) > 1.0f))
        {
            return true;
        }
        return false;
    }

    // Returns current movement state of player
    public MovementStates GetCurrentMovementState()
    {
        return currentMovementState;
    }

    // Flags player as having interacted with a ladder
    public void InteractWithLadder(Vector3 snapPos)
    {
        currentMovementState = currentMovementState == MovementStates.ladder ? MovementStates.walk : MovementStates.ladder;

        if(currentMovementState == MovementStates.ladder)
        {
            // If player has just gotten onto ladder, set position to "snap" transform position saved (see Source Engine ladders for inspiration)
            controller.enabled = false;
            transform.position = new Vector3(snapPos.x, transform.position.y, snapPos.z);
            controller.enabled = true;
        }
    }

    // Trigger detector used to get on / off ladders
    private void OnTriggerEnter(Collider other)
    {
        if(currentMovementState == MovementStates.ladder)
        {
            currentMovementState = MovementStates.walk;
        }
    }
}
