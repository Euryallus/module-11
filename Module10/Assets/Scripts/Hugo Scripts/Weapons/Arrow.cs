using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Projectiles used by the Bow
// Development window:  Prototype phase & production phase
// Inherits from:       MonoBehaviour
public class Arrow : MonoBehaviour
{
    public float damageDone;        // Amount of damage arrows inflict
    private bool hasHit = false;    // Stores if arrow has hit something

    private Rigidbody rb;           // Ref. to own RigidBody component
    private Vector3 fireForward;    // Ref. to direction arrow should launch in

    private void Awake()
    {
        // Assigns ref. to rigidbody
        rb = gameObject.GetComponent<Rigidbody>();
    }

    public void Fire(Vector3 direction, float force)
    {
        // Removes parent & sets forward vect to direction fired in
        transform.parent = null;
        transform.forward = direction;

        // Sets velocity to forward * arrow force (decided by designer)
        gameObject.GetComponent<Rigidbody>().velocity = transform.forward * force;

        // Enables collisions
        gameObject.GetComponent<Collider>().enabled = true;
        fireForward = direction;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Checks if arrow hasn't already hit & that it hasnt hit the player
        if(!hasHit)
        {
            // When arrow hits play sound effect
            AudioManager.Instance.PlaySoundEffect2D("arrowHit");

            // Flags hasHit as true & removes resitual velocity from arrow
            hasHit = true;

            if(collision.gameObject.GetComponent<EnemyHealth>())
            {
                // If object has EnemyHealth component, deal damage & destroy self
                gameObject.transform.parent = collision.gameObject.transform;
                collision.gameObject.GetComponent<EnemyHealth>().DoDamage(damageDone);
                Destroy(gameObject);
                return;
            }
            else if (collision.gameObject.GetComponent<TestDummy>())
            {
                // If object is a test dummy make it take damage
                collision.gameObject.GetComponent<TestDummy>().TakeHit();
            }


            if(collision.transform.CompareTag("Untagged"))
            {
                if(!collision.gameObject.GetComponent<DoorCollider>() && !collision.gameObject.GetComponent<NPC>())
                {
                    // Checks if object collided with is static - if so, freeze in place
                    rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                    rb.velocity = Vector3.zero;
                    rb.isKinematic = true;

                    transform.forward = fireForward;
                    return;
                }
            }

            // If collision has no other event associated just make arrow lose all velocity on collision
            rb.velocity = Vector3.zero;
            
        }
    }
}
