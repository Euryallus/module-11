using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Adapted version of EnemyBase for Enemy type #3 - Similarly to Enemy2, it summons a large projectile which is thrown at player
// Development window:  Prototype phase
// Inherits from:       EnemyBase
public class Enemy3 : EnemyBase
{
    [Header("Enemy 3 additions")]
    [SerializeField]    private GameObject  projectilePrefab;           // Prefab projectile used by enemy
    [SerializeField]    private Transform   projSpawnPoint;             // Transform used to position projectile (by default it's above the enemies head)
    [SerializeField]    private float       spawnToLaunchTime = 1.5f;   //Time between spawning projectile & firing projectile(allows time for player to react)
                        private GameObject  lastProjectile;             // Stores ref. to last projectile thrown by enemy

    // ## NOTE! ##
    // Due to a changing design, Enemy 3 currently operates exactly like Enemy 2 just with altered variables for attackDistance
    // This script (Enemy3) is almost identicle to the Enemy2 script. This is expected to change during the Production phase

    public override void Start()
    {
        // Calls base Start() function but adjusts timeBetweenAttacks to inclue the time between spawning & launching projectile
        base.Start();
        timeBetweenAttacks += spawnToLaunchTime;
    }

    // Altered EngagedUpdate() to ensure enemy faces player
    public override void EngagedUpdate()
    {
        base.EngagedUpdate();

        // Added due to issue with rotating towards the player when stationary
        if (agent.destination == agent.transform.position)
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
        if (CheckForPlayer())
        {
            lastProjectile.GetComponent<Enemy3Projectile>().Launch(dir, player.transform);
        }
        else
        {
            Destroy(lastProjectile);
        }
    }
}
