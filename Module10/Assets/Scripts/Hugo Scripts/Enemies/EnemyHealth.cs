using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Manages enemy health & conditions. Attached to all enemies
// Development window:  Prototype phase
// Inherits from:       MonoBehaviour

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 1f;   // Maximum (initial) health of an enemy
                     protected float health = 1f;      // Current health of enemy

    public bool alive = true;
    
    // Causes enemy to take [x] damage
    public virtual void DoDamage(float damageAmount)
    {
        // Reduces "health" by amount specified
        health -= damageAmount;
        if(health <= 0.0f)
        {
            // If health is then less than 0, call Die() func.
            Die();
        }
    }

    protected virtual void Die()
    {
        // ## NOTE! ##
        // This func. will be expanded as the weapons system becomes more complex and enemy animations / models are implemented

        alive = false;

        // By default, just destroys the enemy
        gameObject.GetComponent<EnemyBase>().StopAllCoroutines();

        Destroy(gameObject);
    }
}
