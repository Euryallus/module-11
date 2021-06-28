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
    private Vector3 fireForward;

    [SerializeField] private Vector3 collisionPoint;

    private void Awake()
    {
        // Assigns ref. to rigidbody
        rb = gameObject.GetComponent<Rigidbody>();
    }

    public void Fire(Vector3 direction, float force)
    {
        transform.parent = null;

        transform.forward = direction;

        gameObject.GetComponent<Rigidbody>().velocity = transform.forward * force;

        gameObject.GetComponent<Collider>().enabled = true;
        fireForward = direction;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Checks if arrow hasn't already hit & that it hasnt hit the player
        if(!hasHit)
        {
            collisionPoint = collision.GetContact(0).point;
            // Flags hasHit as true & removes resitual velocity from arrow
            hasHit = true;
            Debug.LogWarning(collision.transform.name);

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
                collision.gameObject.GetComponent<TestDummy>().TakeHit();
            }
            
            if (collision.transform.gameObject.isStatic)
            {
                // Checks if object collided with is static - if so, freeze in place
                rb.velocity = Vector3.zero;
                rb.isKinematic = true;
                rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                transform.forward = fireForward;
                return;
            }
            else
            {
                rb.velocity = Vector3.zero;
            }
        }
    }
}
