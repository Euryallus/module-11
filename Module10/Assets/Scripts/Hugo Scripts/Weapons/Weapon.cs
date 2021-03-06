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

    // (base damage now uses the value set on the weapon item)
    //public float baseDamage = 0.25f;                        // Base damage done by weapon

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

        if(cooldown < cooldownTime)
        {
            containerSlot.SetCoverFillAmount(1.0f - cooldown / cooldownTime);
        }
        else
        {
            containerSlot.SetCoverFillAmount(0.0f);
        }
    }

    // Generates damage done
    public float CalculateDamage(float baseDamageValue)
    {
        // Generates random damage within (damageVariation / 2) of base damage, multiplied by modifier (default is 1 so no change)
        float damage = Random.Range(baseDamageValue - (damageVariation / 2), baseDamageValue + (damageVariation / 2)) * damageModifier;

        // Rolls to see if player crits
        if (Random.Range(0f, 1f) < critChance)
        {
            // If crit is flagged, multiply damage by crit mod.
            damage *= critDamageMultiplier;
        }

        return damage;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        containerSlot.SetCoverFillAmount(0.0f);
    }
}
