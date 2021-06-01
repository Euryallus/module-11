using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Adapted version of EnemyBase to develop abilities & characteristics of the 2nd enemy type
// Development window:  Prototype phase
// Inherits from:       EnemyBase
public class Enemy2 : EnemyBase
{
    [Header("Enemy 2 variables")] 
    [SerializeField]    private GameObject projectilePrefab;        // Stores prefab of projectile enemy fires at player (mechanic subject to change)
    [SerializeField]    private Transform projSpawnPoint;           // Reference to transform point that projectiles spawn at (currently above enemies head)
    [SerializeField]    private float spawnToLaunchTime = 1.5f;     // Time between spawning projectile & firing projectile (allows time for player to react)
                        private GameObject lastProjectile;          // Stores reference to last projectile created by the enemy

    public override void Start()
    {
        base.Start();
        // Calls base Start() function but adjusts timeBetweenAttacks to inclue the time between spawning & launching projectile
        timeBetweenAttacks += spawnToLaunchTime;
    }

    // Altered EngagedUpdate() to ensure enemy faces player
    public override void EngagedUpdate()
    {
        base.EngagedUpdate();

        // Added due to issue with rotating towards the player when stationary
        if(agent.destination == agent.transform.position)
        {
            // Checks if enemy has been stopped - if so, turn to face the player to keep in sight
            TurnTowards(player);
        }
    }

    // Altered Attack(), allows enemy to launch projectiles
    public override void Attack()
    {
        //Spawns projectile at point saved with self as the parent transform
        lastProjectile = Instantiate(projectilePrefab, projSpawnPoint.position, Quaternion.identity, transform);

        //Begins co-routine that leads to projectile being "fired"
        StartCoroutine("Fire");
    }

    // co-routine used to "fire" the projectile
    private IEnumerator Fire()
    {
        // Waits [spawnToLaunchTime] seconds with projectile above enemies head
        yield return new WaitForSeconds(spawnToLaunchTime);

        // Calculates direction projectile is to be fired at 
        Vector3 dir = player.transform.position - projSpawnPoint.position;

        // Un-childs projectile from self
        lastProjectile.transform.parent = null;

        // Checks if player is still in LOS - if it is, launch the projectile, and if not just destroy the projectile
        if(CheckForPlayer())
        {
            lastProjectile.GetComponent<Enemy2Projectile>().Launch(dir, player.transform);
        }
        else
        {
            Destroy(lastProjectile);
        }
    }
}
