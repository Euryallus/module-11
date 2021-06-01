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
    public override void PerformMainAbility()
    {
        if(cooldown >= cooldownTime)
        {
            if (Physics.Raycast(playerTransform.position, playerTransform.forward, out RaycastHit weaponHit, reachLength))
            {
                // If weapon cooldown has ended & player can be hit by weapon, deal damage
                if (weaponHit.transform.GetComponent<EnemyHealth>())
                {
                    float damage = CalculateDamage();
                    weaponHit.transform.GetComponent<EnemyHealth>().DoDamage(damage);

                    // Outputs damage
                    Debug.Log(damage);
                }
            }

            // If the weapon has attached animation, play it
            if(animator != null)
            {
                animator.SetTrigger("Swing");
            }
            cooldown = 0f;
            base.PerformMainAbility();
        }
    }
}
