using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         "Base" class for all weapons available to player that can be held
// Development window:  Prototype phase
// Inherits from:       HeldItem

public class Weapon : HeldItem
{
    [Range(0.0f, 1.0f)]    public float critChance = 0.05f; // Chance of player getting a crit with weapon (between 0 (never) and 1 (always) )

    public float baseDamage = 0.25f;                        // Base damage done by weapon
    public float damageVariation = 0.1f;                    // Range of weapon damage (max variation in damage from base) 
    public float critDamageMultiplier = 1.5f;               // Multiplier applied to damage when player crits
    public float damageModifier = 1f;                       // Base damage modifier (can be used by effects to increase player strength)
    public float cooldownTime = 0.5f;                       // Time between attacks
    protected float cooldown = 0f;                          // internal countdown

    public Animator animator;                               // Ref. to weapon animator
    
    public virtual void Update()
    {
        // Increases cooldown clock by [Time.deltaTime]
        cooldown += Time.deltaTime;
    }

    // Generates damage done
    public float CalculateDamage()
    {
        // Generates random damage within (damageVariation / 2) of base damage, multiplied by modifier (default is 1 so no change)
        float damage = Random.Range(baseDamage - (damageVariation / 2), baseDamage + (damageVariation / 2)) * damageModifier;

        // Rolls to see if player crits
        if (Random.Range(0f, 1f) < critChance)
        {
            // If crit is flagged, multiply damage by crit mod.
            damage *= critDamageMultiplier;
        }

        return damage;
    }
}
