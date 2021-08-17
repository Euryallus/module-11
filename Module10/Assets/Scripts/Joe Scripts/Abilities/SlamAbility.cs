using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ||=======================================================================||
// || SlamAbiltiy: A child class of PlayerAbility that handles the slam     ||
// ||    ability (aka the levitate ability) specifically.                   ||
// ||=======================================================================||
// || Used on prefab: Hugo/Player                                           ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the production phase (Module 11).                                 ||
// ||=======================================================================||

public enum SlamAbilityState
{
    None,

    Starting,       // Charging the slam
    LiftingEnemies, // Lifting enemies within radius into the air
    DroppingEnemies // Dropping enemies back to the ground
}

public class SlamAbility : PlayerAbility
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [Header("Slam Ability")]
    [SerializeField] private GameObject     shockwaveEffectPrefab;  // GameObject instantiated when the ability is used
    [SerializeField] private GameObject     impactEffectPrefab;     // GameObject instantiated when the ability is used
    [SerializeField] private GameObject     frozenImpactParticles;  // Particles spawned when a frozen enemy hits the ground
    [SerializeField] private CameraShake    playerCameraShake;      // Reference to the script on the player that handles camera shake
    [SerializeField] private float          effectRadius = 15.0f;   // Enemies within this radius of the player will be affected by the ability
    [SerializeField] private float          enemyLiftHeight = 6.0f; // How high the enemies are lifted above the ground
    [SerializeField] private float          enemyDropDamage = 0.5f; // How much damage is applied to each enemy when dropped back to the ground

    #endregion

    private SlamAbilityState    abilityState        = SlamAbilityState.None;    // The current state of the ability

    private List<EnemyBase>     targetedEnemies     = new List<EnemyBase>();    // All enemies currently being targeted by the ability
    private List<Vector3>       enemyStartPositions = new List<Vector3>();      // Start positions (position before being lifted) for each targeted enemy
    private List<Vector3>       enemyLiftPositions  = new List<Vector3>();      // Lift positions (position to move towards when being lifted) for each targeted enemy

    private const float LiftSpeed = 1.5f;           // How quickly enemies are lifted
    private const float DropSpeed = 1.5f;           // How quickly enemies are dropped
    private const float MaxRandomDropDelay = 0.2f;  // Maximum amount of time (seconds, where min = 0) to be used when calculating a random delay before dropping an enemy

    protected override void AbilityStart()
    {
        // If the ability state is not none, and hence enemies are already being
        //   affected by the ability, don't allow it to be started again yet

        if (abilityState == SlamAbilityState.None)
        {
            // Start the ability and its effects

            base.AbilityStart();

            StartCoroutine(AbilityEffectCoroutine());
        }
    }

    private IEnumerator AbilityEffectCoroutine()
    {
        // 1. Starting (Find enemies + initial VFX)
        //=========================================

        abilityState = SlamAbilityState.Starting;

        // Target enemies in radius
        ApplyEffectToEnemiesInRadius();

        // Create a shockwave effect
        Instantiate(shockwaveEffectPrefab, transform);

        // Shake the player camera to add impact
        playerCameraShake.ShakeCameraForTime(1.2f, CameraShakeType.ReduceOverTime, 0.07f);

        // Wait for a short amount of time before lifting enemies
        yield return new WaitForSeconds(0.3f);

        // 2. Lifting + secondary VFX
        //===========================

        // Start lifting targeted enemies
        abilityState = SlamAbilityState.LiftingEnemies;

        // Create an impact effect
        Instantiate(impactEffectPrefab, transform);

        // Wait before dropping enemies back down
        yield return new WaitForSeconds(6.0f);

        // 3. Dropping
        //============

        abilityState = SlamAbilityState.DroppingEnemies;

        // Drop any enemies that were not already dropped due to external factors (e.g. freezing a suspended enemy)
        DropRemainingEnemies();

        // Wait until all enemies have been dropped
        yield return new WaitForSeconds(DropSpeed + MaxRandomDropDelay);

        // Reset state ready for the ability to be used again
        abilityState = SlamAbilityState.None;
    }

    protected override void Update()
    {
        base.Update();

        if(abilityState == SlamAbilityState.LiftingEnemies)
        {
            // Lift targeted enemies while in the LiftingEnemies state
            LiftEnemiesUpdate();
        }
    }

    private void ApplyEffectToEnemiesInRadius()
    {
        // Layer mask for collision - only detect enemies
        LayerMask layerMask = LayerMask.GetMask("Enemy");

        // Find all enemies within a spherical radius of the player
        Collider[] collidersInRadius = Physics.OverlapSphere(transform.position, effectRadius, layerMask);

        // Clear arrays from any previous uses of the ability
        targetedEnemies.Clear();
        enemyStartPositions.Clear();
        enemyLiftPositions.Clear();

        foreach (Collider collider in collidersInRadius)
        {
            // Get the EnemyBase script of each enemy in the radius
            EnemyBase enemyScript = collider.GetComponent<EnemyBase>();

            if (enemyScript != null)
            {
                if (enemyScript.AgentEnabled)
                {
                    // Only apply the ability effect to enemies that don't already have their agents disabled
                    //   (e.g. enemies that are frozen will not be affected)

                    ApplyEffectToEnemy(enemyScript, collider);
                }
            }
        }
    }

    private void ApplyEffectToEnemy(EnemyBase enemy, Collider enemyCollider)
    {
        // Stop the enemy from moving and mark it as suspended (allows for synergy with the freeze ability)
        enemy.StopAgentMovement();
        enemy.Suspended = true;

        // Add the enemy to the list of targeted enemies
        targetedEnemies.Add(enemy);

        // Add the enemy start position and calculate a lift position
        //   by adding enemyLiftHeight to the start position

        Vector3 enemyPos = enemyCollider.gameObject.transform.position;

        enemyStartPositions.Add(enemyPos);
        enemyLiftPositions.Add(new Vector3(enemyPos.x, enemyPos.y + enemyLiftHeight, enemyPos.z));
    }

    private void LiftEnemiesUpdate()
    {
        for (int i = 0; i < targetedEnemies.Count; i++)
        {
            if(targetedEnemies[i] != null)
            {
                // Move all targeted enemy gameobjects towards their respective lift positions

                GameObject enemyGameObj = targetedEnemies[i].gameObject;

                // Using deltatime for framerate independent movement and to create an ease-out effect
                enemyGameObj.transform.position = Vector3.Lerp(enemyGameObj.transform.position, enemyLiftPositions[i], Time.deltaTime * LiftSpeed);
            }
        }
    }

    private void DropRemainingEnemies()
    {
        for (int i = 0; i < targetedEnemies.Count; i++)
        {
            // Start dropping all targeted enemies that haven't already dropped
            StartCoroutine(DropEnemyCoroutine(i, Random.Range(0.0f, MaxRandomDropDelay)));
        }
    }

    public void DropAndKillSuspendedEnemy(EnemyBase enemy, bool frozen)
    {
        // Drops and kills a single targeted and suspended enemy

        if(enemy.Suspended && targetedEnemies.Contains(enemy))
        {
            // Drop the enemy and apply a huge amount of damage to kill it instantly once it lands
            StartCoroutine(DropEnemyCoroutine(targetedEnemies.IndexOf(enemy), 0.0f, frozen, 10000.0f));
        }
    }

    private IEnumerator DropEnemyCoroutine(int enemyIndex, float startDelay, bool frozen = false, float damageOverride = -1.0f)
    {
        if(targetedEnemies[enemyIndex] != null)
        {
            // Get the EnemyBase script of enemy script to drop and mark it as no longer suspended
            EnemyBase enemyToDrop = targetedEnemies[enemyIndex];
            enemyToDrop.Suspended = false;

            // Wait for the randomised start delay, if any, that was given
            yield return new WaitForSeconds(startDelay);

            float dropProgress = 0.0f;

            while(dropProgress < 1.0f)
            {
                // Move the enemy towards its original start position, cubing dropProgress to create a strong ease-in
                //   effect (rather than standard linear movement) to give the illusion of gravity being applied.
                enemyToDrop.gameObject.transform.position = Vector3.Lerp(enemyLiftPositions[enemyIndex], enemyStartPositions[enemyIndex], dropProgress * dropProgress * dropProgress);

                dropProgress += Time.deltaTime * DropSpeed;
                yield return null;
            }

            if(frozen)
            {
                // If the enemy was frozen before being dropped, play an ice break sound and spawn some impact particles
                AudioManager.Instance.PlaySoundEffect3D("iceBreak", enemyToDrop.gameObject.transform.position);
                Instantiate(frozenImpactParticles, enemyToDrop.gameObject.transform.position, Quaternion.identity);
            }

            // Apply the standard enemyDropDamage, or the damage override value if one was given
            float enemyDamage = (damageOverride == -1.0f ? enemyDropDamage : damageOverride);

            enemyToDrop.gameObject.GetComponent<EnemyHealth>().DoDamage(enemyDamage, frozen);

            // Allow the enemy to move again
            enemyToDrop.StartAgentMovement();

            // The enemy is no longer targeted by the ability
            targetedEnemies[enemyIndex] = null;
        }
    }

    protected override void FindUIIndicator()
    {
        // Finds the UI indicator for the slam ability
        uiIndicator = GameObject.FindGameObjectWithTag("SlamIndicator").GetComponent<AbilityIndicator>();
    }

    protected override PlayerAbilityType GetAbilityType()
    {
        return PlayerAbilityType.Slam_Levitate;
    }
}