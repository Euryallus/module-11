using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Adapted version of Enemy2 - used to test enemy animations & visuals
// Development window:  Production phase
// Inherits from:       Enemy2

public class Enemy2Anim : Enemy2
{
    [SerializeField] private Animator anim; // Stores ref. to animator component of enemy

    public override void StartEvade()
    {
        base.StartEvade();
        // Starts enemy walking anim when evading the player
        anim.SetBool("IsWalking", true);
    }

    public override void StartPatrolling()
    {
        base.StartPatrolling();
        // Starts enemy walking when patrolling an area
        anim.SetBool("IsWalking", true);
    }

    public override void Engage()
    {
        base.Engage();
        // Starts enemy walking when engaging with player
        anim.SetBool("IsWalking", true);
    }

    protected override void Release()
    {
        base.Release();
        // Runs attack animation when enemy launches projectile
        anim.SetTrigger("Attack");
    }

    protected override IEnumerator WaitAndMove(float maxDistance, Vector3 newPointOrigin, float maxWaitTime)
    {
        // Stops agent from moving
        agent.SetDestination(transform.position);

        anim.SetBool("IsWalking", false);

        // Waits for x time 
        yield return new WaitForSeconds(Random.Range(0f, maxWaitTime));
        // Flags bool as false now waitForSeconds is over
        findingNewPos = false;
        anim.SetBool("IsWalking", true);
        // Tells agent to go to random position around origin 
        GoToRandom(maxDistance, newPointOrigin);
    }

    public override void EngagedUpdate()
    {
        // Increase time since enemy last attacked
        attackCooldown += Time.deltaTime;


        if (CheckForPlayer())
        {
            timeSinceLastSeen = 0f;
            if (Vector3.Distance(transform.position, player.transform.position) < attackDistance)
            {
                // If player is visible & within the attack distance defined in EnemyBase, STOP moving & turn to face the player
                agent.SetDestination(transform.position);
                TurnTowards(player);

                // If the time since last attack is over the time flagged as cooldown, run Attack() & reset cooldown
                if (attackCooldown > timeBetweenAttacks)
                {
                    Attack();
                    attackCooldown = 0f;
                    return;
                }

                // If enemy has reached destination, stop walking
                if (anim.GetBool("IsWalking"))
                {
                    anim.SetBool("IsWalking", false);
                }
            }
            else
            {
                // If player is visible but cannot be "reached" for attack, move towards player & animate running 
                if (!anim.GetBool("IsWalking"))
                {
                    anim.SetBool("IsWalking", true);
                }

                // Sets enemy destination to player position
                GoTo(playerLastSeen);
            }
        }
        else
        {
            timeSinceLastSeen += Time.deltaTime;
            if (timeSinceLastSeen < argoTime)
            {
                GoTo(playerLastSeen);
            }
            else
            {
                // If player has been lost while engaged, switch to searching the area
                StartSearching(playerLastSeen);
            }
        }
    }
}
