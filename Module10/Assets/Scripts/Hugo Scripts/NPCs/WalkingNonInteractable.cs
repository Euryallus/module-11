using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Manages NPC movement & associated animations
// Development window:  Production phase
// Inherits from:       MonoBehaviour

public class WalkingNonInteractable : MonoBehaviour
{
    public enum MovementType    // Possible movement patterns
    {
        noMovement,             // NPC does not move at all (just idle anim)
        cycleInOrder,           // NPC visits each point in-order, once finished returns to [0] position & starts again
        pingPong,               // NPC goes back and forth between points - once finished, goes back the way they came
        oneOff                  // Moves along path once (ususally after player interaction) and stays in end position
    }

    [Header("Behaviour options")]
    [SerializeField]    private MovementType moveSet;               // Saves movement type of NPC
    [SerializeField]    private bool walkFromBeginning = true;      // Flags if NPC should walk from Start() call
    [SerializeField]    private bool enableQuestsWhenStationary;    // Flags if NPC should be interactable when stationary
    
                        private bool isMoving = false;              // Flags if NPC is currently in walk cycle

    [Header("Determines if NPC will START walking when dialogue finishes (not needed if walk from beginning=true)")]
    [SerializeField]    private bool walkAfterAnyInteraction = false;   // Flags if NPC should start walking after dialogue interaction

                        private Collider NPCcollider;                   // Ref. to collider attached to NPC

    [SerializeField]    private Animator animator;                      // Ref. to animator attached to NPC

    [Header("Points to visit (will be followed in-order if 'cycleInOrder' was selected")]
    [SerializeField]    private List<Transform> movementPoints = new List<Transform>(); // List of points NPC will walk to

                        private int currentPoint = -1;  // Initial pointer (++ on start, means first point is [0])
                        private int listMod = 1;        // Amount pointer changes by when current point is met (either +1 or -1 depending on movement type)

    [Header("Speed, time stamps, etc.")]
    [SerializeField]    private float speed = 2f;           // Speed of NPC (1.75 matches walk anim)
    [SerializeField]    private float rotationSpeed = 7f;   // Speed NPC rotates to face walk direction
    [SerializeField]    private float waitTimeAtPoint = 3f; // Time spent at each point before progressing to next
                        private WaitForSeconds wait;        // WaitForSeconds object (defined using waitTimeAtPoint)

  

    private void Start()
    {
        // Flags moving as false and generates WaitForSeconds object using waitTimeAtPoint (used in co-routine)
        isMoving = false;
        wait = new WaitForSeconds(waitTimeAtPoint);

        // If the NPC is flagged as walking from the start, begin walking (w/ correct anim)
        if(moveSet != MovementType.noMovement && walkFromBeginning)
        {
            animator.SetBool("IsWalking", true);
            NewPoint();
        }

        // Collects ref. to NPC's collider
        NPCcollider = gameObject.GetComponent<Collider>();

        // Enables NPC collider if NPC is not walking yet but should be interactable
        if(enableQuestsWhenStationary)
        {
            if(!walkFromBeginning)
            {
                NPCcollider.enabled = true;
            }
        }
    }

    // TODO: 
    // 1. find next position to go to
    // 2. set walking anim
    // 3. lerp rotate npc to face new direction
    // 4. move [x] distance to new pos
    // 5. once there, wait [y] seconds in idle before moving on

    public void NewPoint()
    {
        // Generates new point based on movement type assigned
        switch(moveSet)
        {
            case MovementType.cycleInOrder:

                // Increases currentPoint by 1 - if end point is reached, pointer goes back to 0
                currentPoint += 1;
                if(currentPoint == movementPoints.Count)
                {
                    currentPoint = 0;
                }

                isMoving = true;
                animator.SetBool("IsWalking", true);

                break;


            case MovementType.oneOff:

                // Increases currentPoint by 1 - if end point is reached, stop moving completely (else just move to next point in list)
                currentPoint += 1;
                if (currentPoint == movementPoints.Count)
                {
                    animator.SetBool("IsWalking", false);

                    if(enableQuestsWhenStationary)
                    {
                        //NPCcollider.enabled = true;

                        isMoving = false;
                        currentPoint = -1;
                    }
                }
                else
                {
                    isMoving = true;
                    animator.SetBool("IsWalking", true);
                }


                break;


            case MovementType.pingPong:

                // Increase currentPoint by [listMod], if end is reached switch movement direction (listMod *= -1)
                if(currentPoint == movementPoints.Count - 1)
                {
                    listMod = -1;
                }
                
                if(currentPoint == 0)
                {
                    listMod = 1;
                }
                currentPoint += listMod;

                isMoving = true;
                animator.SetBool("IsWalking", true);

                break;
        }
    }

    private void Update()
    {

        // If the NPC is flagged as mobile, check position & destination
        if(moveSet != MovementType.noMovement)
        {        
            if(isMoving)
            {
                // Checks if NPC has reached destination - if not, continue walking in destination direction & adjust rotation to face destination
                if (Vector3.Distance(movementPoints[currentPoint].position, transform.position) >= 0.5f)
                {
                    Vector3 direction = movementPoints[currentPoint].position - transform.position;

                    Quaternion rotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);

                    

                    transform.position = transform.position + (direction.normalized * speed * Time.deltaTime);
                }
                else
                {
                    // If destiation is reached, stop walking anim & wait for [x] seconds
                    isMoving = false;
                    animator.SetBool("IsWalking", false);

                    StartCoroutine(WaitAtPoint());
                }
            }
            else if(currentPoint != -1)
            {
                // If the NPC isn't currently moving and the currentPoint isn't reset to -1, rotate to face [forward] of destination point
                Vector3 direction = movementPoints[currentPoint].forward;

                Quaternion rotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);
            }
        
        }
    }

    // Allows NPC to wait at a given point before moving on
    private IEnumerator WaitAtPoint()
    {
        // Enables NPC interactions when stationary (if designer flags)
        if (enableQuestsWhenStationary)
        {
            NPCcollider.enabled = true;
        }

        // Waits for [waitTimeAtPoint] seconds before disabling collider & finding new point
        yield return wait;

        NPCcollider.enabled = false;

        NewPoint();
    }

    // Called when player interacts with NPC - prevents them from walking off
    public void InteruptWait()
    {
        StopAllCoroutines();
    }

    // Flags player interaction as over - if npc is meant to move, they'll start moving after [x] time
    public void StartMovingAgain()
    {
        if(moveSet != MovementType.noMovement && walkAfterAnyInteraction)
        {
            StartCoroutine(WaitAtPoint());
        }
    }
}
