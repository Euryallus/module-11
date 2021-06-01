using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy3Projectile : MonoBehaviour
{
    [Header("Launch variables")]
    [SerializeField]    private float       launchVelocity;           // Stores velocity the projectile is fired at (requested by Noah)
                        private Rigidbody   rb;                   // Stores ref. to own RigidBody

    private void Start()
    {
        // Assigns ref. to RigidBody
        rb = gameObject.GetComponent<Rigidbody>();
        // Freezes all movement by default
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    // ## NOTE! ##
    // Due to a changing design, Enemy3Projectile currently operates exactly like Enemy2Projectile
    // This script (Enemy3Projectile) is almost identicle to the Enemy2Projectile script. This is expected to change during the Production phase

    public void Launch(Vector3 direction, Transform target)
    {
        // Remomves all constraints to allow movement
        rb.constraints = RigidbodyConstraints.None;

        // Source:
        // ##################################################################################################
        // #                                                                                                #
        // # 2009. Find the angle to hit target at x,y,z. [online]                                          #
        // # Available at: <https://forum.unity.com/threads/find-the-angle-to-hit-target-at-x-y-z.33659/>   #
        // # [Accessed 23 April 2021].                                                                      #
        // #                                                                                                #
        // ##################################################################################################

        // Website above was used to calculate angle projectile should be launched at. See response by Troy-Dawson (Nov. 7 2009)

        // Rotates projectile to "face" the player as a benchmark
        transform.LookAt(target.transform.position);
        // Draws line in scene view to visualise direction
        Debug.DrawLine(transform.position, transform.position + (transform.forward * 3), Color.green, 3f);

        // Chances "direction" passed from world to local direction
        Vector3 targetVect = transform.InverseTransformDirection(direction);

        // Inverts z component of vector (prevents direction going funky when rotating around Z axis)
        targetVect.z *= -1;

        // Variables needed in calculation - derived from SUVAT equation (
        float x = targetVect.z;     // X component of vector (z vect. due to orientation on axis)
        float y = targetVect.y;     // Y component of vector
        float v = launchVelocity;   // Desired initial velocity of proj.
        float g = -9.81f;           // Gravity used

        // Pre-calculated calues of v^2, v^4, x^2 to avoid having to calculate in middle of equation
        float v2 = v * v;
        float v4 = v2 * v2;
        float x2 = x * x;

        // Calculates angle needed from Pythag. theorem and SUVAT equation
        // Mathf.Atan2 used to calc. angle of y / x

        // v2 - Mathf.Sqrt(v4 - g * (g * x2 + 2 * y * v2)) = X component of launch vector
        // g * x = Y component of launch vector

        // theta = angle adjustment needed in rads to hit target

        float theta = Mathf.Atan2(v2 - Mathf.Sqrt(v4 - g * (g * x2 + 2 * y * v2)), g * x);

        // Rotates projectile to adjust according to angle Theta
        transform.Rotate(new Vector3(-theta * Mathf.Rad2Deg, 0, 0));

        // Sets velocity of rigidbody to be forward vect. of proj. * launch velocity
        rb.velocity = launchVelocity * transform.forward;
    }

    public void Die()
    {
        // Destroys self
        Destroy(gameObject);
    }

    // Checks for collision each frame using rb
    private void OnCollisionEnter(Collision collision)
    {
        // Layer mask used to "ignore" collisions on layer specified, in this case layer 6 = "enemies" layer
        int mask = 1 << 6;
        // Checks if the collision took place on a layer other than enemies
        if (collision.transform.gameObject.layer != mask)
        {
            // Collects array of colliders within blast radius using OverlapSphere
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1);

            // Cycles each collider in array & checks if one is the player
            foreach (Collider hit in hitColliders)
            {
                // If player is in hit array, suffer damage
                if (hit.gameObject.CompareTag("Player"))
                {
                    hit.gameObject.GetComponent<PlayerStats>().DecreaseHealth(0.2f);
                }
            }

            // When proj. collides with a collider that isn't attacked to an enemy, Die();
            Die();
        }
    }


}
