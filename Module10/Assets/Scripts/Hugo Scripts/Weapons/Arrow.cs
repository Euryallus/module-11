using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Projectiles used by the Bow
// Development window:  Prototype phase
// Inherits from:       MonoBehaviour
public class Arrow : MonoBehaviour
{
    public float damageDone;        // Amount of damage arrows inflict
    private bool hasHit = false;    // Stores if arrow has hit something

    private Rigidbody rb;           // Ref. to own RigidBody component

    private void Awake()
    {
        // Assigns ref. to rigidbody
        rb = gameObject.GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Checks if arrow hasn't already hit & that it hasnt hit the player
        if(!hasHit && !collision.transform.CompareTag("Player"))
        {
            // Flags hasHit as true & removes resitual velocity from arrow
            hasHit = true;
            rb.velocity = Vector3.zero;

            if (collision.transform.gameObject.isStatic)
            {
                // Checks if object collided with is static - if so, child arrow to collided object & freeze in place
                gameObject.transform.parent = collision.gameObject.transform;
                rb.constraints = RigidbodyConstraints.FreezeAll;
                return;
            }

            if(collision.gameObject.GetComponent<EnemyHealth>())
            {
                // If object has EnemyHealth component, deal damage & destroy self
                gameObject.transform.parent = collision.gameObject.transform;
                rb.constraints = RigidbodyConstraints.FreezeAll;

                collision.gameObject.GetComponent<EnemyHealth>().DoDamage(damageDone);
                Destroy(gameObject);
            }
        }
    }
}
