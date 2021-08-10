using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Functionality for the ice projectile used by FreezeAbility
// Development window:  Production phase
// Inherits from:       MonoBehaviour

public class IceLanceObject : MonoBehaviour
{

    [SerializeField]    private float speed = 50f;              // Controls how fast projectile moves (recommended default of 50 but can be adjusted by Noah)
    [SerializeField]    private GameObject impactParticles;     // Ref. to particles to spawn on impact
    [SerializeField]    private float maxLifetime = 5f;         // Max. time proj. can exist for before being destroyed (if doesnt hit anything stops it from going forever)
    [HideInInspector]   public bool move = false;               // Flags if projectile should move yet or is being charged up

    private float currentLifetime;                              // Records current lifetime of projectile (used w/ maxLifetime to cull)
    private FreezeAbility parent;                               // Ref. to FreezeAbility that spawned the projectile

    private void Awake()
    {
        // Removes own box collider - stops collision with player
        gameObject.GetComponent<BoxCollider>().enabled = false;
    }

    public void Launch(Vector3 direction, FreezeAbility spawner)
    {
        // Removes self from being a child of player
        transform.parent = null;

        // Adjusts forward vector to face target (direction calculated in FreezeAbility) and flags move as true
        transform.forward = direction;
        move = true;

        // Saves ref. to FreezeAbility that spawned it
        parent = spawner;

        // Removes all RB constraints and fires forward at [speed] velocity
        gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        gameObject.GetComponent<Rigidbody>().velocity = transform.forward * speed;

        // Re-enables box collider so can now collide w/ environment
        gameObject.GetComponent<BoxCollider>().enabled = true;
    }

    private void Update()
    {
        // If move is flagged as true, increase currentLifetime
        if(move)
        {
            currentLifetime += Time.deltaTime;

            // If lifetime is > max, destroy self
            if(currentLifetime >= maxLifetime)
            {
                Destroy(gameObject);
            }
        }
    }

    // Called when projectile collides with another object
    private void OnCollisionEnter(Collision collision)
    {
        // Creates new instance of impact particles & places at impact point
        GameObject particles = Instantiate(impactParticles, collision.GetContact(0).point, Quaternion.identity);

        // Sets particle forward to face direction fired in (ensures effect looks realistic)
        particles.transform.forward = -transform.forward;

        // Plays effect using ParticleGroup component
        particles.GetComponent<ParticleGroup>().PlayEffect();
        
        //If projectile collided with an enemy, run FreezeAbilty.FreezeEnemy( enemy hit )
        if (collision.transform.gameObject.GetComponent<EnemyBase>())
        {
            parent.FreezeEnemy(collision.transform.gameObject.GetComponent<EnemyBase>());
        }

        // Creates layer mask for collision - only collides with enemies (Enemy = layer 6)
        int mask = 1 << 6;

        // If ability can "chain" to more than 1 enemy, grab all enemies within [chainDistance] and also freeze them
        if (parent.chainEnemyCount > 0)
        {
            RaycastHit[] surrounding = Physics.SphereCastAll(transform.position, parent.chainDistance, collision.transform.forward, parent.chainDistance, mask, QueryTriggerInteraction.Ignore);

            // If there are enemies close by and ability can chain, freeze them
            if(surrounding.Length != 0)
            {
                for (int i = 0; i < parent.chainEnemyCount; i++)
                {
                    parent.FreezeEnemy(surrounding[i].transform.gameObject.GetComponent<EnemyBase>());
                }
            }
        }

        // Destroys self on impact after all other operations
        Destroy(gameObject);
    }
}
