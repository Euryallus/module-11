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

    [SerializeField] [Tooltip("How much each upgraded damage level adds to the base damage this weapon does")]
    private float damageUpgadeMultiplier = 0.3f;

    [SerializeField] [Tooltip("How much each upgraded attack speed level decreases to cooldown time of the weapon")]
    private float cooldownUpgadeMultiplier = 0.2f;

    private void Start()
    {
        // Decrease the cooldown time value based on how much the player has upgraded attack speed
        //   (subtracting 1.0 so nothing is decreased if the value was not upgraded)
        cooldownTime -= (item.GetCustomFloatPropertyWithName("attackSpeed").Value - 1.0f) * cooldownUpgadeMultiplier;

        Debug.Log("Cooldown time is " + cooldownTime);
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
                float damage = CalculateDamage();

                Debug.Log("Sword swung, did " + damage + " damage");

                weaponHit.transform.GetComponent<EnemyHealth>().DoDamage(damage);
            }
            else if(weaponHit.transform.GetComponent<TestDummy>())
            {
                weaponHit.transform.GetComponent<TestDummy>().TakeHit();
            }
        }        
    }

    // Added by Joe, adds player upgraded damage level to the base damage:
    public override float CalculateDamage()
    {
        float baseDamage = base.CalculateDamage();

        // Get the customised damage value, subtracting 1 because the base (non-upgraded) value is 1.0f
        float damageUpgrade = (item.GetCustomFloatPropertyWithName("damage").Value - 1.0f);

        // Return the modified damage value, multiplying by damageUpgadeMultiplier
        //   so the amount of damage added for each upgrade level can be balanced
        return baseDamage + damageUpgrade * damageUpgadeMultiplier;
    }
}