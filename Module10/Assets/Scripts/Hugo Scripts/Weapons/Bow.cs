using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Ranged bow weapon
// Development window:  Prototype phase
// Inherits from:       Weapon
public class Bow : Weapon
{

    public float chargeTime             = 0.5f; // Stores time player has to hold down "use" button to release at full speed
    public float arrowDamage            = 0.2f; // Default damage arrow deals
    public float arrowReleaseVelocity   = 5f;   // Maximum release velocity of arrow
    
    public GameObject arrowPrefab;              // Prefab version of arrow
    public ItemGroup arrowRequired;             // Number of arrows required to fire bow (usually 1)

    private bool isHeld     = false;            // Flags if bow is being drawn
    private float heldTime  = 0;                // Time bow has been drawn

    public override void StartSecondardAbility()
    {
        if(cooldown >= cooldownTime && !isHeld)
        {
            // Starts player drawing back bow if ability is activated & wasn't already flagged
            isHeld = true;
            heldTime = 0f;
        }

        base.StartSecondardAbility();
    }

    public override void Update()
    {
        base.Update();

        // Checks if bow is being drawn back
        if(isHeld)
        {
            // Increases time spent drawing back
            heldTime += Time.deltaTime;
            if(heldTime > chargeTime)
            {
                heldTime = chargeTime;
            }

            // Changes scale of bow to reflect charge amount (will be replaced with something less simple in prod. phase)
            transform.localScale = new Vector3(transform.localScale.x, 1 - (heldTime / 4) , transform.localScale.z);
        }
    }

    public override void EndSecondaryAbility()
    {
        base.EndSecondaryAbility();
        InventoryPanel inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<InventoryPanel>();

        //Checks inventory for required arrows - if found, remove arrows from inventory & fire arrow
        if (inventory.ContainsQuantityOfItem(arrowRequired))
        {
            GameObject newArrow = Instantiate(arrowPrefab, transform.position, Quaternion.identity);
            // Velocity of arrow is proportional to time being drawn back (clamped to arrowReleaseVelocity)
            newArrow.GetComponent<Rigidbody>().velocity = transform.forward * (arrowReleaseVelocity * (heldTime / chargeTime));

            //Adjusts forwards vect. of arrow to line up with player forwards
            newArrow.transform.forward = transform.forward;

            isHeld = false;
            cooldown = 0f;

            inventory.RemoveItemFromInventory(arrowRequired.Item);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
            // No arrows are available
            Debug.LogWarning("No arrow in inventory!");

        }
        // Resets bow scale
        transform.localScale = new Vector3(1, 1, 1);

    }

}
