using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Base class used for all Enemy types within the game. Handles state behaviour & default attacks
// Development window:  Prototype phase
// Inherits from:       MonoBehaviour

public class EnemyBase : MonoBehaviour
{
    public enum EnemyState                                              // Possible states enemy can be in at any given point 
    {
        stationary,                                                     // Stationary: Enemy is not moving
        engaged,                                                        // Engaged:    Enemy is actively persuing player
        search,                                                         // Search:     Enemy is searching for player
        patrol,                                                         // Patrol:     Enemy is patrolling around camp
        evade                                                           // Evade:      Enemy is low health & fleeing
    }

    protected EnemyState currentState;                                  // current state of enemy

    [Header("Enemy general properties")]

    [SerializeField] protected float viewDistance               = 25f;  // Default distance enemy can see
    [Range(1.0f, 360f)]
    [SerializeField]    protected float viewAngle               = 120f; // View cone angle enemy can see (e.g. 60 degrees to the left & 60 degrees to the right)
    [SerializeField]    protected float stationaryTurnSpeed     = 5f;   // Speed at which enemy rotates to face player when stationary

    [Header("Enemy behaviour properties")]

    [SerializeField]    public int difficulty                = 1;    // Difficulty of enemy (assigned by designer)
    [SerializeField]    protected int noOfSearchPoints          = 8;    // Number of points enemy will search after losing track of the player before returning to patrol behaviour
    [SerializeField]    protected int searchPointsVisited       = 0;    // Number of search points already searched
    [SerializeField]    protected float patrolWanderDistance    = 15f;  // Distance enemy will patrol around centralHubPos when in patrol mode
    [SerializeField]    protected float searchDiameter          = 10f;  // Distance enemy will search for player around last seen position
                        public Vector3 centralHubPos;                // Position of central "hub" - when spawned via EnemyCamp, centralHubPos = EnemyCamp position
                        protected Vector3 playerLastSeen;               // Position player was last seen at

    [Header("Combat stuff (Ensure attack distance is > stopping distance!!)")]

    [SerializeField]    protected float baseDamage;                     // Default amount of damage enemy deals to player
    [SerializeField]    protected float timeBetweenAttacks      = 2f;   // Time between enemy attack attempts
    [SerializeField]    protected float attackDistance;                 // Distance enemy can attack from
                        protected float attackCooldown;                 // Time since enemy last attacked (internal cooldown)
                        protected float dot;                            // Angle between enemy forward vect & player (calculated from dot prod.)
                        protected NavMeshAgent agent;                   // Ref to enemy AI agent
                        protected GameObject player;                    // Ref to player
                        protected PlayerStats playerStats;              // Ref to player stats
                        protected float distToPlayer;                   // Distance from player to enemy
                        public EnemyCampManager manager;             // Enemy Camp manager (used to alert other enemies in camp, spawn units etc.)

    [Header("Behaviours")]

    [Range(0f, 10f)]
    [SerializeField]    protected float maxAtEachPatrolPoint;           // Maximum time enemy will spent at a patrol point
    [SerializeField]    protected float patrolSpeed;                    // Speed enemy patrols at
    [SerializeField]    protected float defaultSpeed;                   // Speed enemy moves when searching / engaged
                        protected bool findingNewPos = false;           // Flags if enemy is waiting at point before selecting a new one

    public bool canSee = false;


    public virtual void Start()
    {
        // Stores references to own NavMeshAgent, player GameObject, and PlayerStats component of player
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player");
        playerStats = player.GetComponent<PlayerStats>();

        // Sets default speed to that set by NavMeshAgent
        defaultSpeed = agent.speed;

        // Initial state is patrolling
        StartPatrolling();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        // Update functions depend on which state the enemy is in - each is detailed in ___Update()
        switch (currentState)
        {
            case EnemyState.stationary:
                // Runs stationary state update func.
                StationaryUpdate();

                break;

            case EnemyState.engaged:
                // Runs engaged state update func.
                EngagedUpdate();

                break;

            case EnemyState.search:
                // Runs search state update func.
                SearchUpdate();

                break;

            case EnemyState.patrol:
                // Runs patrol state update func.
                PatrolUpdate();

                break;

            case EnemyState.evade:
                // Runs evade state update func.
                EvadeUpdate();

                break;

            default:
                // If none of the above are triggered, the enemy does not have a state assigned - something is very wrong, so warn dev.
                Debug.LogWarning("Something's wrong with " + name);
                break;
        }

    }

    //goes to location passed on navmesh
    public virtual void GoTo(Vector3 targetPosition)
    {
        // ## NOTE! ##
        // Code commented out below was originally used to check for the closest spot on the navmesh to the co-ord specified, but this caused a LOT of issues with navigation
        // Instead the enemy just attempts to get to the position set, whether or not it's accessable


        /*NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, 5f, 1);
        if (hit.position.x != Mathf.Infinity)
        {
            NavMeshPath path = new NavMeshPath();
            agent.CalculatePath(targetPosition, path);
        
            if (path.status != NavMeshPathStatus.PathPartial)
            {
                enemyDestination = targetPosition;
                
                return true;
            }
        }*/

        // Sets agent destination to position passed as parameter
        agent.SetDestination(targetPosition);
    }

    //goes to random position [x] meters away from the origin specified
    public virtual void GoToRandom(float maxDistanceFromCurrent, Vector3 origin)
    {
        // Generates random point within [maxDistanceFromCurrent] meters from origin
        Vector3 randomPosition = Random.insideUnitSphere * maxDistanceFromCurrent;
        randomPosition += origin;

        // Adjusts y component of position to increase likelihood of point being accessible on navmesh
        randomPosition.y = transform.position.y;

        // Tells AI agent to go to random pos. calculated
        GoTo(randomPosition);
    }

    //stationary state update
    public virtual void StationaryUpdate()
    {
        // Unless player is spotted, do nothing
        if(CheckForPlayer())
        {
            currentState = EnemyState.engaged;
        }
    }

    public virtual void EngagedUpdate()
    {
        // Increases time since last attack
        attackCooldown += Time.deltaTime;

        // If player is spotted, act accordingly
        if (CheckForPlayer())
        {
            // Check distance between player & enemy
            if (Vector3.Distance(transform.position, player.transform.position) < attackDistance)
            {
                // If can see & player is within attack distance, stop moving & turn to face player
                agent.SetDestination(transform.position);
                TurnTowards(player);

                // Once cooldown is over, attack player & reset cooldown
                if (attackCooldown > timeBetweenAttacks)
                {

                    Attack();
                    attackCooldown = 0f;
                    return;
                }
            }
            else
            {
                // If player is visible but not reachable, follow them
                GoTo(playerLastSeen);
            }
        }
        else
        {
            // If player has been lost while engaged, switch to searching the area
            StartSearching(playerLastSeen);
        }
    }

    // Search state update
    public virtual void SearchUpdate()
    {
        // If player is seen while searching, try to alert other units
        if (CheckForPlayer())
        {
            if (manager != null)
            {
                manager.AlertUnits(playerLastSeen);
            }

            // Switch state to "engaged" and start following player
            Engage();

            return;
        }

        // If player hasnt been spotted and player is within ~3m of the destination stored, run "wait and move on" co-routine 
        if (Vector3.Distance(transform.position, agent.destination) <= agent.stoppingDistance * 1.5f && !findingNewPos)
        {
            // Search points incrimented (prevents searching forever)
            ++searchPointsVisited;

            if(searchPointsVisited < noOfSearchPoints)
            {
                // Enemy can still visit more search points, so select a new one using WaitAndMove
                StartCoroutine(WaitAndMove(searchDiameter, playerLastSeen, 0.5f));
                findingNewPos = true;
            }
            else
            {
                // If enemy has visited enough search points, stop searching & start patrolling
                StartPatrolling();
            }
        }
    }

    // Patrol state update
    public virtual void PatrolUpdate()
    {
        // If player is spotted, switch to engaged state & return
        if(CheckForPlayer())
        {

            Engage();

            return;
        }

        // If player isn't seen & enemy is within ~ 3m of destination, pick a new point to go to centred on the central hub position
        if (Vector3.Distance(transform.position, agent.destination) <= agent.stoppingDistance * 1.5f && !findingNewPos)
        {
            StartCoroutine(WaitAndMove(patrolWanderDistance, centralHubPos, maxAtEachPatrolPoint));
            // Bool flagged to prevent coroutine being run 4000 times
            findingNewPos = true;
            
        }
    }

    // Evade state update
    public virtual void EvadeUpdate()
    {

    }

    // Begins searching behaviour
    public virtual void StartSearching(Vector3 searchPos)
    {
        // Sets player last seen position to pos. passed as parameter
        playerLastSeen = searchPos;
        // Sets first search destination to be that position
        GoTo(playerLastSeen);
        // Resets points count to 0
        searchPointsVisited = 0;
        // Sets state to "search"
        currentState = EnemyState.search;

        // Resets agents speed to "max"
        agent.speed = defaultSpeed;

        // Debug help
        Debug.Log(gameObject.name + " started searching " + searchPos);
    }

    // Begins patrol behaviour
    public virtual void StartPatrolling()
    {
        // SearchPointsVisited = 0;
        // Sets agent speed to patrol speed (slower than engaged speed"
        agent.speed = patrolSpeed;

        // Stops any co-routine left from search func.
        StopCoroutine("WaitAndMove");

        // Calls co-routine to move in [x] seconds to random pos. centred around central hub
        WaitAndMove(patrolWanderDistance, centralHubPos, 1.0f);
        // Sets current state to "patrol"
        currentState = EnemyState.patrol;
    }

    // Checks if player is visible using view angle specified and view distance
    public virtual bool CheckForPlayer()
    {
        // Stores distance from enemy to player
        distToPlayer = Vector3.Distance(transform.position, player.transform.position);

        // If enemy can "see" player based on distance, continue evaluating
        if(distToPlayer <= viewDistance)
        {
            // Calculates dot product between enemy forward vect. and player position
            dot = Vector3.Dot(transform.transform.forward.normalized, (player.transform.position - transform.position).normalized);
            // Calculates angle from dot product
            dot = Mathf.Acos(dot);

            // If angle calculated is < view angle defined, player is within view cone
            if (dot <= Mathf.Deg2Rad * viewAngle / 2 )
            {
                // Creates raycast mask for excluding colliders on "enemies" layer
                int mask = 1 << 6;


                // Raycasts from enemy towards player - if it hits, nothing's obstructing the view
                if (Physics.Raycast(transform.position, player.transform.position - transform.position, out RaycastHit hit, viewDistance, ~mask))
                {
                    // If raycast does hit player
                    if (hit.transform.parent.gameObject.CompareTag("Player"))
                    {
                        // Draw a debug line to show connection, store position player was seen at, and return true
                        Debug.DrawLine(transform.position, hit.transform.position, Color.red);
                        playerLastSeen = player.transform.position;
                        searchPointsVisited = 0;
                        return true;
                    }
                }
            }
        }
        

        // If player is not visible, return false
        
        return false;
    }

    public virtual void TurnTowards(GameObject target)
    {
        //https://answers.unity.com/questions/351899/rotation-lerp.html

        Vector3 lookTarget = Quaternion.LookRotation(target.transform.position - transform.position).eulerAngles;
        lookTarget.x = 0;
        lookTarget.z = 0;

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(lookTarget), stationaryTurnSpeed * Time.deltaTime);

    }

    // Base attack - when within range, raycasts towards player, & if it hits player loses health (only works for melee)
    public virtual void Attack()
    {
        // Raycasts from enemy to player - if it hits, the player can be hurt
        if(Physics.Raycast(transform.position, player.transform.position - transform.position, out RaycastHit hit, attackDistance))
        {
            // Debug assist
            Debug.LogWarning("Hit " + hit.transform.name);

            // If hit player & the player has health script, do damage
            if(hit.transform.CompareTag("Player"))
            {
                if(playerStats!= null)
                {
                    playerStats.DecreaseHealth(baseDamage);
                }

            }
        }
    }

    // Checks if point passed is on navmesh (not currently used - very costly)
    public bool IsPointOnNavMesh(Vector3 pos, float maxDist)
    {
        return NavMesh.SamplePosition(pos, out NavMeshHit hit, maxDist, 1);
    }

    // Gets a random position definately on the navmesh centred around origin (not currently used - very costly)
    public Vector3 GetRandomPos(float maxDistanceFromCurrent, Vector3 origin)
    {
        Vector3 randomPosition = Random.insideUnitSphere.normalized * Random.Range(3, maxDistanceFromCurrent);

        randomPosition += origin;

        NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, maxDistanceFromCurrent, 1);
        return hit.position;

    }

    // Used by the enemy camp to signal that the enemy has been spotted
    public void AlertOfPosition(Vector3 position)
    {
        // Checks if player is already engaged with player
        if(currentState != EnemyState.engaged)
        {
            // If not, update player last seen and start searching the area
            playerLastSeen = position;

            StartSearching(position);
        }

    }

    // Called when enemy enters engaged state - resets speed, stops any co-routines, and sets state to engaged
    public virtual void Engage()
    {
        agent.speed = defaultSpeed;
        StopCoroutine("WaitAndMove");
        currentState = EnemyState.engaged;
    }

    // Used to wait for random amount of time at a position before moving on, aims to make enemies seem more "natural"
    protected virtual IEnumerator WaitAndMove(float maxDistance, Vector3 newPointOrigin, float maxWaitTime)
    {
        // Stops agent from moving
        agent.SetDestination(transform.position);

        // Waits for x time 
        yield return new WaitForSeconds(Random.Range(0f, maxWaitTime));
        // Flags bool as false now waitForSeconds is over
        findingNewPos = false;
        // Tells agent to go to random position around origin 
        GoToRandom(maxDistance, newPointOrigin);
    }
}
