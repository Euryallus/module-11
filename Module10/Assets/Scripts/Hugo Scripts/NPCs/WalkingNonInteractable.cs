using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkingNonInteractable : MonoBehaviour
{
    public bool isWalking = true;
    [SerializeField] private Animator animator;

    [SerializeField] private List<Transform> movementPoints = new List<Transform>();
    private int currentPoint = 0;
    private int listMod = 1;

    [SerializeField] private float speed = 2f;
    [SerializeField] private float rotationSpeed = 7f;
    [SerializeField] private float waitTimeAtPoint = 3f;
    private WaitForSeconds wait;

    private bool isMoving = false;
    


    private void Start()
    {
        if(isWalking)
        {
            animator.SetBool("IsWalking", true);
            wait = new WaitForSeconds(waitTimeAtPoint);

            NewPoint();
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
    }

    private void Update()
    {
        if(isWalking)
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
        yield return wait;

        NewPoint();
    }
}
