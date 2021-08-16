using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Ranged bow weapon
// Development window:  Prototype phase & production phase
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

    [SerializeField] private float bowDetectRange = 100f;   // Range arrow can hit to while still being accurate

    private GameObject playerCam;   // Ref. to player's camera

    public override void StartSecondardAbility()
    {
        if(cooldown >= cooldownTime && !isHeld)
        {

            AudioManager.Instance.PlaySoundEffect2D("bowCharge");

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
        playerCam = GameObject.FindGameObjectWithTag("MainCamera");
        base.EndSecondaryAbility();

        InventoryPanel inventory = GameSceneUI.Instance.PlayerInventory;

        //Checks inventory for required arrows - if found, remove arrows from inventory & fire arrow
        if (inventory.ContainsQuantityOfItem(arrowRequired, out _))
        {
            // Creates new instance of arrow
            GameObject newArrow = Instantiate(arrowPrefab, transform.position, Quaternion.identity);

            // Raycasts forwards from camera - if it hits something, that's the arrow's target
            RaycastHit hit;
            Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out hit, bowDetectRange);

            // Calculate directional vector for arrow to go in - defaults to camera's forward
            Vector3 direction = playerCam.transform.forward;

            // If the raycast hit something, calculate direction arrow must travel in to hit that point
            if (hit.transform != null)
            {
                direction = -((newArrow.transform.position - hit.point).normalized);
            }

            // Fire arrow in direction given
            newArrow.GetComponent<Arrow>().Fire(direction, arrowReleaseVelocity);

            // Reset values now arrow has been shot
            isHeld = false;
            cooldown = 0f;

            // Attempt to remove arrow item from inventory
            for (int i = 0; i < arrowRequired.Quantity; i++)
            {
                inventory.TryRemoveItem(arrowRequired.Item);
            }

            AudioManager.Instance.PlaySoundEffect2D("bowRelease");
        }
        else
        {
            // If no arrows in inventory reset scale
            transform.localScale = new Vector3(1, 1, 1);
            // No arrows are available
            Debug.LogWarning("No arrow in inventory or hotbar!");

        }

        // Resets bow scale
        transform.localScale = new Vector3(1, 1, 1);
    }

}
