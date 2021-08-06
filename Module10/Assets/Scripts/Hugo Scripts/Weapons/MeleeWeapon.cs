using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Default class for all melee weapons (anything non-ranged, e.g. sword)
// Development window:  Prototype phase
// Inherits from:       Weapon
public class MeleeWeapon : Weapon
{
    public float reachLength = 5f;  // Range of weapon (how far in front of player it reaches)

    private void Start()
    {
        CustomFloatProperty itemAttackSpeedPoperty = item.GetCustomFloatPropertyWithName("attackSpeed", true);

        if(itemAttackSpeedPoperty != null)
        {
            // Use the custom attack speed value (which can be upgraded by the player) if it was set on the weapon item
            cooldownTime = 1.0f - itemAttackSpeedPoperty.Value;
        }
    }

    public override void PerformMainAbility()
    {
        if (cooldown >= cooldownTime)
        {
            // If the weapon has attached animation, play it
            if (animator != null)
            {
                animator.SetTrigger("Swing");
                cooldown = 0f;
            }
        }
    }

    public void Swing()
    {
        if (Physics.Raycast(playerTransform.position, playerTransform.forward, out RaycastHit weaponHit, reachLength))
        {
            // If weapon cooldown has ended & player can be hit by weapon, deal damage
            if (weaponHit.transform.GetComponent<EnemyHealth>())
            {
                // Calculate damage based on the value set on the weapon item that can be upgraded by the player
                float damage = CalculateDamage(item.GetCustomFloatPropertyWithName("damage").Value);

                Debug.Log("Sword swung, did " + damage + " damage");

                weaponHit.transform.GetComponent<EnemyHealth>().DoDamage(damage);
            }
            else if(weaponHit.transform.GetComponent<TestDummy>())
            {
                weaponHit.transform.GetComponent<TestDummy>().TakeHit();
            }
        }        
    }
}