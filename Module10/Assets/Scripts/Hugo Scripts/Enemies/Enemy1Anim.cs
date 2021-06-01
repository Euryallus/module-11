using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Adapted version of Enemy1 - used to test enemy animations & visuals
// Development window:  Prototype phase
// Inherits from:       Enemy1

// ## NOTE! ##
// This class is being used to test animations - it will not be used as the final version
// Once changes are approved & animations are finalised, we will migrate this all to the base Enemy1 class

public class Enemy1Anim : Enemy1
{
    
    [SerializeField]    private Animator anim;  // Reference to enemies animator

    // Test version of an altered EngagedUpdate function allowing for animations
    public override void EngagedUpdate()
    {
        // Increase time since enemy last attacked
        attackCooldown += Time.deltaTime;

       
        if (CheckForPlayer())
        {
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

                // Stop running forwards animation
                anim.SetBool("Run Forward", false); 
            }
            else
            {
                // If player is visible but cannot be "reached" for attack, move towards player & animate running 
                anim.SetBool("Run Forward", true);
                // Sets enemy destination to player position
                GoTo(playerLastSeen);
            }
        }
        else
        {
            // If player cannot be seen, switch to searching for them
            StartSearching(playerLastSeen);
        }
    }

    // Altered version of StartPatrolling() allowing for animations
    public override void StartPatrolling()
    {
        // When patrolling starts, ensure enemy animation is set to "running"
        anim.SetBool("Walk Forward", true);
        anim.SetBool("Run Forward", false);
        // Call base StartPatrolling()
        base.StartPatrolling();
    }

    // Altered version of Engage() allowing for animations
    public override void Engage()
    {
        // When enemy engages with player, ensure enemy animation is set to "running"
        anim.SetBool("Walk Forward", false);
        anim.SetBool("Run Forward", true);
        // Call base Engage()
        base.Engage();
    }

    // Altered version of Attack() allowing for animations
    public override void Attack()
    {
       if(!HasSplit)
       {
            // If enemy is attacking and hasn't yet split, play the "Cast Spell" animation to show enemy is about to clone itself
           anim.SetTrigger("Cast Spell");
       }
       else
       {
            // If enemy is melee attacking, play one of two attack animations randomly
           if(Random.Range(0, 2) == 0)
           {
               anim.SetTrigger("Stab Attack");
           }
           else
           {
               anim.SetTrigger("Smash Attack");
           }
       }
       // Run base melee attack
       base.Attack();
 
    }

    // Altered version of WaitAndMove (could not use base.WaitAndMove() due to issues with "yield return...", hence it's duplicated to use for testing)
    protected override IEnumerator WaitAndMove(float maxDistance, Vector3 newPointOrigin, float maxWaitTime)
    {
        // Stops enemy from moving
        agent.SetDestination(transform.position);

        // Stops all walking / running animations
        anim.SetBool("Walk Forward", false);
        anim.SetBool("Run Forward", false);

        // Stop for random amount of time (using variables in EnemyBase)
        yield return new WaitForSeconds(Random.Range(0f, maxWaitTime));

        // Flags enemy as "done waiting at point" & choses new point to go to
        findingNewPos = false;
        GoToRandom(maxDistance, newPointOrigin);

        //Starts walking forwards animation
        anim.SetBool("Walk Forward", true);
    }
}
