using UnityEngine;

// PlatformButtonBehaviour: defines how moving platforms react to
//   button presses/releases when connected to a PuzzleButton
public enum PlatformButtonBehaviour
{
    TriggerOnPress, // Starts moving when the button is pressed, returns to start point when released
    PauseOnPress    // Moves as normal by default but pauses in place while the button is pressed
}

// PlatformMovementType: defines the movement pattern of a platform
public enum PlatformMovementType
{
    OutAndBack,     // The platform will move through all points from first to last, then return to the start
    Loop            // The platform will move through all points from first to last, then instantly start again from the first
}

// PlatformMoveDirection: Possible movement directions, forwards, backwards or none/static
public enum PlatformMoveDirection
{
    Forwards,
    Backwards,
    None
}

// ||=======================================================================||
// || MovingPlatform: A platform that moves on a set path, can be triggered ||
// ||   by a PuzzleButton or move automatically.                            ||
// ||=======================================================================||
// || Used on prefab: Joe/PuzzleElements/MovingPlatform                     ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public class MovingPlatform : MonoBehaviour
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    //See tooltips for comments

    [Space]
    [Header("(See tooltips for info)")]
    [Header("Moving Platform")]

    [SerializeField] [Tooltip("The points that form the movement path of the platform. Note: these points are relative to the GameObject's position in the world")]
    private Vector3[]            movePoints = new Vector3[] { Vector3.zero, new Vector3(10.0f, 0.0f, 0.0f) };

    [SerializeField] [Tooltip("OutAndBack: Move through all points from first to last, then return back in the opposite direction\n\n" +
                                "Loop: Move through all points from first to last, then instantly start again from the first\n\n"+
                                "NOTE: Movement type is ignored if connected to a button using TriggerOnPress behaviour")]
    private PlatformMovementType movementType;

    [SerializeField] [Tooltip("How quickly the platform moves between points")]
    private float                moveSpeed = 6.0f;

    #endregion

    #region Properties

    public bool Paused              { set { paused = value; } }
    public bool TriggeredByButton   { set { triggeredByButton = value; } }

    #endregion

    private int                     currentPointIndex;          // Index of the point being moved towards in the movePoints array
    private Vector3                 basePosition;               // Starting position of the platform before any movement is applied
    private bool                    paused;                     // Whether movement is paused
    private PlatformMoveDirection   moveDirection;              // The current direction of movement
    private bool                    triggeredByButton;          // Whether this platform is triggered by a PuzzleButton

    private Transform               playerReturnToTransform;    // Transform that acts as the player's parent before they step on the platform,
                                                                //   and that they should be returned to as a child after stepping off

    void Start()
    {
        // Set the base position to the platform's default position in the world
        basePosition = transform.position;

        if (!triggeredByButton)
        {
            // The platform is not triggered by a button, start moving forwards automatically
            moveDirection = PlatformMoveDirection.Forwards;
        }
        else
        {
            // The platform is triggered by a button, don't move by default
            moveDirection = PlatformMoveDirection.None;
        }
    }

    void Update()
    {
        // Calculate the distance the platform should move this frame
        float distanceToMove = moveSpeed * Time.deltaTime;

        if(!paused && moveDirection != PlatformMoveDirection.None)
        {
            // Movement is not paused and the platform is set to move forwards/backwards

            if (Vector3.Distance(transform.position, basePosition + movePoints[currentPointIndex]) <= distanceToMove)
            {
                // The platform will have reached or surpassed its target point by the next frame, find the next point to aim for
                FindNextPoint();
            }

            // Move towards the point at currentPointIndex in the movePoints array, adding the position to basePosition since
            //   positions in the movePoints array are relative to the platform's starting position
            transform.position = Vector3.MoveTowards(transform.position, basePosition + movePoints[currentPointIndex], distanceToMove);
        }
    }

    private void FindNextPoint()
    {
        if (moveDirection == PlatformMoveDirection.Forwards)
        {
            // Platform is moving forwards

            if (currentPointIndex < (movePoints.Length - 1))
            {
                // The final point has not been reached yet, move to the next one
                currentPointIndex++;
            }
            else
            {
                // The final point has been reached

                if(triggeredByButton)
                {
                    // If the platform was triggered by a button, it should stop moving after reaching the end point
                    moveDirection = PlatformMoveDirection.None;
                }
                else
                {
                    if (movementType == PlatformMovementType.OutAndBack)
                    {
                        // Start going in the reverse direction
                        moveDirection = PlatformMoveDirection.Backwards;
                    }
                    else // (movementType == Loop)
                    {
                        // Loop back to start but continue going forwards
                        currentPointIndex = 0; 
                    }
                }
            }
        }
        else if(moveDirection == PlatformMoveDirection.Backwards)
        {
            // Platform is moving backwards

            if (currentPointIndex > 0)
            {
                // The first point has not been reached yet, move to the previous point
                currentPointIndex--;
            }
            else
            {
                if(triggeredByButton)
                {
                    // If the platform was triggered by a button, it should stop moving after reaching the start point
                    moveDirection = PlatformMoveDirection.None;
                }
                else
                {
                    // Otherwise begin moving forwards again
                    moveDirection = PlatformMoveDirection.Forwards;
                }
            }
        }
    }

    public void StartMovingForwards()
    {
        // Sets moveDireciton to Forwards and finds a point so movement starts instantly in the correct direction
        moveDirection = PlatformMoveDirection.Forwards;
        FindNextPoint();
    }

    public void StartMovingBackwards()
    {
        // Sets moveDireciton to Backwards and finds a point so movement starts instantly in the correct direction
        moveDirection = PlatformMoveDirection.Backwards;
        FindNextPoint();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            // The player is on the platform

            // Make the player a child of the platform so they move with it, and keep a reference to their previous parent
            playerReturnToTransform = other.transform.parent;
            other.transform.SetParent(transform);

            // autoSyncTransforms prevents the player from sliding off the platform
            Physics.autoSyncTransforms = true;

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // The player left the platform

            // autoSyncTransforms no longer needed, revert to default value
            Physics.autoSyncTransforms = false;

            // Restore the player's original parent transform
            other.transform.SetParent(playerReturnToTransform);
        }
    }

    private void OnDrawGizmos()
    {
        #if (UNITY_EDITOR)

        // Gizmos for visualisation of the platform's path in the editor

        if (movePoints != null && movePoints.Length > 0)
        {
            for (int i = 0; i < movePoints.Length; i++)
            {
                Vector3 worldPos = transform.position + movePoints[i];

                // Line connecting the current and next points
                if (i < (movePoints.Length - 1))
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(worldPos, transform.position + movePoints[i + 1]);
                }

                Gizmos.color = Color.red;

                // Line to show downwards direction
                Gizmos.DrawLine(worldPos, worldPos - new Vector3(0.0f, 1.0f, 0.0f));

                // Sphere showing point position
                Gizmos.DrawSphere(worldPos, 0.5f);
            }
        }

        #endif
    }

}
