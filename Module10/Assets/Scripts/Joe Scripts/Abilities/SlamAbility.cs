using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SlamAbilityState
{
    None,

    Starting,
    LiftingEnemies,
    DroppingEnemies
}

public class SlamAbility : PlayerAbility
{
    [Header("Slam Ability")]
    [SerializeField] private GameObject     shockwaveEffectPrefab;  // GameObject instantiated when the ability is used
    [SerializeField] private GameObject     impactEffectPrefab;     // GameObject instantiated when the ability is used
    [SerializeField] private GameObject     frozenImpactParticles;  // Particles spawned when a frozen enemy hits the ground
    [SerializeField] private CameraShake    playerCameraShake;      // Reference to the script on the player that handles camera shake
    [SerializeField] private float          effectRadius = 15.0f;
    [SerializeField] private float          enemyLiftHeight = 6.0f;
    [SerializeField] private float          enemyDropDamage = 0.5f;

    private SlamAbilityState    abilityState        = SlamAbilityState.None;
    private List<EnemyBase>     targetedEnemies     = new List<EnemyBase>();
    private List<Vector3>       enemyStartPositions = new List<Vector3>();
    private List<Vector3>       enemyLiftPositions  = new List<Vector3>();
    //private float               lerpProgress;

    private const float LiftSpeed = 1.5f;
    private const float DropSpeed = 1.5f;
    private const float MaxRandomDropDelay = 0.2f;

    protected override void AbilityStart()
    {
        if (abilityState != SlamAbilityState.None)
            return;

        base.AbilityStart();

        StartCoroutine(AbilityEffectCoroutine());
    }

    private IEnumerator AbilityEffectCoroutine()
    {
        // Starting (Find enemies + initial VFX)
        //=======================================

        abilityState = SlamAbilityState.Starting;

        ApplyEffectToEnemiesInRadius();

        Instantiate(shockwaveEffectPrefab, transform);

        playerCameraShake.ShakeCameraForTime(1.2f, CameraShakeType.ReduceOverTime, 0.07f);

        yield return new WaitForSeconds(0.3f);

        // Lifting + secondary VFX
        //=========================

        abilityState = SlamAbilityState.LiftingEnemies;

        Instantiate(impactEffectPrefab, transform);

        yield return new WaitForSeconds(6.0f);

        // Dropping
        //==========

        //lerpProgress = 0.0f;

        abilityState = SlamAbilityState.DroppingEnemies;

        // Drop any enemies that were not already dropped due to external factors (e.g. freezing a suspended enemy)
        DropRemainingEnemies();

        yield return new WaitForSeconds(DropSpeed + MaxRandomDropDelay);

        abilityState = SlamAbilityState.None;
    }

    protected override void Update()
    {
        base.Update();

        if(abilityState == SlamAbilityState.LiftingEnemies)
        {
            LiftEnemiesUpdate();
        }
    }

    private void ApplyEffectToEnemiesInRadius()
    {
        LayerMask layerMask = LayerMask.GetMask("Enemy");

        Collider[] collidersInRadius = Physics.OverlapSphere(transform.position, effectRadius, layerMask);

        targetedEnemies.Clear();
        enemyStartPositions.Clear();
        enemyLiftPositions.Clear();

        foreach (Collider collider in collidersInRadius)
        {
            EnemyBase enemyScript = collider.GetComponent<EnemyBase>();

            if (enemyScript != null)
            {
                Debug.Log("Slam ability targeted enemy: " + collider.gameObject.name, collider.gameObject);

                if (enemyScript.AgentEnabled)
                {
                    // Only apply effect to enemies that don't already have their agents disabled
                    //   (e.g. enemies that are frozen will not be affected)

                    ApplyEffectToEnemy(enemyScript, collider);
                }
            }
        }
    }

    private void ApplyEffectToEnemy(EnemyBase enemy, Collider enemyCollider)
    {
        enemy.StopAgentMovement();

        enemy.Suspended = true;

        targetedEnemies.Add(enemy);

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
                GameObject enemyGameObj = targetedEnemies[i].gameObject;

                enemyGameObj.transform.position = Vector3.Lerp(enemyGameObj.transform.position, enemyLiftPositions[i], Time.deltaTime * LiftSpeed);
            }
        }
    }

    //private void DropEnemiesUpdate()
    //{
    //    if(lerpProgress < 1.0f)
    //    {
    //        for (int i = 0; i < targetedEnemies.Count; i++)
    //        {
    //            if (targetedEnemies[i] != null)
    //            {
    //                GameObject enemyGameObj = targetedEnemies[i].gameObject;

    //                // Cubing lerpProgress to give ease-in effect for drop, mimicking gravity
    //                enemyGameObj.transform.position = Vector3.Lerp(enemyLiftPositions[i], enemyStartPositions[i], lerpProgress * lerpProgress * lerpProgress);
    //            }
    //        }
    //    }
    //    else
    //    {
    //        EnemyEffectsDone();
    //    }

    //    lerpProgress += Time.deltaTime * DropSpeed;
    //}

    private void DropRemainingEnemies()
    {
        for (int i = 0; i < targetedEnemies.Count; i++)
        {
            StartCoroutine(DropEnemyCoroutine(i, Random.Range(0.0f, MaxRandomDropDelay)));
        }
    }

    public void DropAndKillSuspendedEnemy(EnemyBase enemy, bool frozen)
    {
        if(enemy.Suspended && targetedEnemies.Contains(enemy))
        {
            // Drop the enemy and apply a huge amount of damage to kill it once it lands
            StartCoroutine(DropEnemyCoroutine(targetedEnemies.IndexOf(enemy), 0.0f, frozen, 100.0f));
        }
    }

    private IEnumerator DropEnemyCoroutine(int enemyIndex, float startDelay, bool frozen = false, float damageOverride = -1.0f)
    {
        if(targetedEnemies[enemyIndex] != null)
        {
            EnemyBase enemyToDrop = targetedEnemies[enemyIndex];
            
            enemyToDrop.Suspended = false;

            yield return new WaitForSeconds(startDelay);

            float dropProgress = 0.0f;

            while(dropProgress < 1.0f)
            {
                enemyToDrop.gameObject.transform.position = Vector3.Lerp(enemyLiftPositions[enemyIndex], enemyStartPositions[enemyIndex], dropProgress * dropProgress * dropProgress);

                dropProgress += Time.deltaTime * DropSpeed;

                yield return null;
            }

            if(frozen)
            {
                AudioManager.Instance.PlaySoundEffect3D("iceBreak", enemyToDrop.gameObject.transform.position);
                Instantiate(frozenImpactParticles, enemyToDrop.gameObject.transform.position, Quaternion.identity);
            }

            float enemyDamage = (damageOverride == -1.0f ? enemyDropDamage : damageOverride);

            enemyToDrop.gameObject.GetComponent<EnemyHealth>().DoDamage(enemyDamage, frozen);

            enemyToDrop.StartAgentMovement();

            targetedEnemies[enemyIndex] = null;
        }
    }

    protected override void FindUIIndicator()
    {
        uiIndicator = GameObject.FindGameObjectWithTag("SlamIndicator").GetComponent<AbilityIndicator>();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, effectRadius);
    }
}