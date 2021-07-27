using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkingNonInteractable : MonoBehaviour
{
    public enum MovementType
    {
        noMovement,
        cycleInOrder,
        pingPong,
        oneOff
    }

    [Header("Behaviour options")]
    [SerializeField] private MovementType moveSet;
    [SerializeField] private bool walkFromBeginning = true;
    [SerializeField] private bool enableQuestsWhenStationary;

    private Collider NPCcollider;

    [SerializeField] private Animator animator;

    [Header("Points to visit (will be followed in-order if 'cycleInOrder' was selected")]
    [SerializeField] private List<Transform> movementPoints = new List<Transform>();

    private int currentPoint = 0;
    private int listMod = 1;

    [Header("Speed, time stamps, etc.")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float rotationSpeed = 7f;
    [SerializeField] private float waitTimeAtPoint = 3f;
    private WaitForSeconds wait;

    private bool isMoving = false;

    private void Start()
    {
        if(moveSet != MovementType.noMovement && walkFromBeginning)
        {
            animator.SetBool("IsWalking", true);
            wait = new WaitForSeconds(waitTimeAtPoint);

            NewPoint();
        }

        if(enableQuestsWhenStationary)
        {
            NPCcollider = gameObject.GetComponent<Collider>();

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

    private void NewPoint()
    {

        switch(moveSet)
        {
            case MovementType.cycleInOrder:

                currentPoint += 1;
                if(currentPoint == movementPoints.Count)
                {
                    currentPoint = 0;
                }

                isMoving = true;
                animator.SetBool("IsWalking", true);

                break;


            case MovementType.oneOff:

                currentPoint += 1;
                if (currentPoint == movementPoints.Count)
                {
                    animator.SetBool("IsWalking", false);

                    if(enableQuestsWhenStationary)
                    {
                        NPCcollider.enabled = true;
                    }
                }
                else
                {
                    isMoving = true;
                    animator.SetBool("IsWalking", true);
                }


                break;


            case MovementType.pingPong:

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
        if(moveSet != MovementType.noMovement)
        {        
            if(isMoving)
            {
                if (Vector3.Distance(movementPoints[currentPoint].position, transform.position) >= 0.5f)
                {
                    Vector3 direction = movementPoints[currentPoint].position - transform.position;

                    Quaternion rotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);

                    

                    transform.position = transform.position + (direction.normalized * speed * Time.deltaTime);
                }
                else
                {
                    isMoving = false;
                    animator.SetBool("IsWalking", false);

                    StartCoroutine(WaitAtPoint());
                }
            }
            else
            {
                Vector3 direction = movementPoints[currentPoint].forward;

                Quaternion rotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);
            }
        
        }
    }

    private IEnumerator WaitAtPoint()
    {
        if (enableQuestsWhenStationary)
        {
            NPCcollider.enabled = true;
        }

        yield return wait;

        NPCcollider.enabled = false;

        NewPoint();
    }

    public void InteruptWait()
    {
        StopAllCoroutines();
    }

    public void StartMovingAgain()
    {
        if(moveSet != MovementType.noMovement)
        {
            StartCoroutine(WaitAtPoint());
        }
    }
}
