using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   Joe Allen (see comments)
// Description:         Allows player to launch an ice projectile at a target
// Development window:  Production phase
// Inherits from:       PlayerAbility

public class FreezeAbility : PlayerAbility
{
    [Header("Freeze details")]

    [SerializeField]    private GameObject iceLancePrefab;      // Ref. to projectile prefab
                        private GameObject spawnedIceLance;     // Ref. to instance of projectile once spawned

    [SerializeField]    private float range = 50f;              // Distance projectile can hit (used in targeting functionality)
    [SerializeField]    private float FreezeDuration = 5f;      // Length of time target is frozen for
    [SerializeField]    private Material freezeMaterial;        // Material enemy switches to when frozen (e.g. ice material)

                        private GameObject playerCam;           // Ref. to player camera



    [Header("Upgrade vars")]
    public int chainEnemyCount = 1;     // Number of enemies in area ice affects
    public float chainDistance = 5f;    // Distance enemies can be hit from initial impact when "chain" is > 1
    
    protected override void Start()
    {
        base.Start();
        
        // Assigns ref. to player camera
        playerCam = gameObject.GetComponent<PlayerMovement>().playerCamera;
    }

    protected override void ChargeStart()
    {
        base.ChargeStart();

        // Creates new instance of the ice projectile as a child of the player
        spawnedIceLance = Instantiate(iceLancePrefab, transform);

        // Sets projectile forward to same as player's forward
        spawnedIceLance.transform.forward = transform.forward;

        // Sets projectile location to just right of player camera
        spawnedIceLance.transform.localPosition = new Vector3(1, 1, 1);
    }

    protected override void ChargeEnd()
    {
        base.ChargeEnd();
        
        // When charge ends, destroy spawned instance of projectile
        Destroy(spawnedIceLance);

    }

    protected override void AbilityStart()
    {
        base.AbilityStart();

        // When ability is fully charged, ChargeEnd() is still called, so re-initialise a new (functional) projectile in the same spot as before
        spawnedIceLance = Instantiate(iceLancePrefab, transform);
        spawnedIceLance.transform.forward = transform.forward;
        spawnedIceLance.transform.localPosition = new Vector3(1, 1, 1);

        // Raycast forwards from the player camera, can hit anything within [range] meters
        RaycastHit hit;
        Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out hit, range);

        // Default direction for launch = straight forwards
        Vector3 direction = playerCam.transform.forward;

        if (hit.transform != null)
        {
            // If the Raycast hit something, the launch direction is actually facing the point of intersection (NOT straight forward)
            direction = -((spawnedIceLance.transform.position - hit.point).normalized);
        }

        // Launch projectile in direction calculated
        spawnedIceLance.GetComponent<IceLanceObject>().Launch(direction, gameObject.GetComponent<FreezeAbility>());
    }

    public void FreezeEnemy(EnemyBase enemyObj)
    {
        // Added by Joe
        if(enemyObj.Suspended)
        {
            // The targeted enemy is currently suspended in the air - apply an ice material, then drop and kill it
            enemyObj.MeshRenderer.material = freezeMaterial;

            GetComponent<SlamAbility>().DropAndKillSuspendedEnemy(enemyObj, true);

            AudioManager.Instance.PlaySoundEffect3D("iceSmash", enemyObj.gameObject.transform.position);
        }
        else if (enemyObj.AgentEnabled)
        {
            // Only freeze enemies that don't already have their agents disabled
            //   Prevents multiple effects that disable enemy movement from being applied at once

            enemyObj.Freeze();

            StartCoroutine(UnFreezeEnemy(enemyObj));
        }
    }

    IEnumerator UnFreezeEnemy(EnemyBase enemyUnfreeze)
    {
        // Enemy to unfreeze is passed - wait [x] seconds then attempt to un-freeze (allows adjustments to freeze time from ability script)
        yield return new WaitForSeconds(FreezeDuration);

        // If enemy is not dead yet, enable movement again
        if(enemyUnfreeze != null)
        {
            enemyUnfreeze.UnFreeze();
        }   
    }

    // Added by Joe - used to display ability cooldown in UI
    protected override void FindUIIndicator()
    {
        uiIndicator = GameObject.FindGameObjectWithTag("FreezeIndicator").GetComponent<AbilityIndicator>();
    }

    // Added by Joe - returns ability type
    protected override PlayerAbilityType GetAbilityType()
    {
        return PlayerAbilityType.Freeze;
    }
}
