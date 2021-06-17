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
    [SerializeField] private CameraShake    playerCameraShake;      // Reference to the script on the player that handles camera shake
    [SerializeField] private float          effectRadius = 15.0f;
    [SerializeField] private float          enemyLiftHeight = 6.0f;

    private SlamAbilityState    abilityState        = SlamAbilityState.None;
    private List<EnemyBase>     targetedEnemies     = new List<EnemyBase>();
    private List<Vector3>       enemyStartPositions = new List<Vector3>();
    private List<Vector3>       enemyLiftPositions  = new List<Vector3>();
    private float               lerpProgress;

    private const float LiftSpeed = 1.5f;
    private const float DropSpeed = 1.5f;

    protected override void AbilityStart()
    {
        if (abilityState != SlamAbilityState.None)
            return;

        base.AbilityStart();

        StartCoroutine(AbilityEffectCoroutine());

        LayerMask layerMask = LayerMask.GetMask("Enemy");

        Collider[] collidersInRadius = Physics.OverlapSphere(transform.position, effectRadius, layerMask);

        targetedEnemies     .Clear();
        enemyStartPositions .Clear();
        enemyLiftPositions  .Clear();

        abilityState = SlamAbilityState.Starting;

        foreach (Collider collider in collidersInRadius)
        {
            EnemyBase enemyScript = collider.GetComponent<EnemyBase>();

            if(enemyScript != null)
            {
                Debug.Log("Slam ability targeted enemy: " + collider.gameObject.name, collider.gameObject);

                enemyScript.StopAgentMovement();

                targetedEnemies.Add(enemyScript);

                Vector3 enemyPos = collider.gameObject.transform.position;

                enemyStartPositions.Add(enemyPos);
                enemyLiftPositions.Add(new Vector3(enemyPos.x, enemyPos.y + enemyLiftHeight, enemyPos.z));
            }
        }
    }

    private IEnumerator AbilityEffectCoroutine()
    {
        Instantiate(shockwaveEffectPrefab, transform);

        playerCameraShake.ShakeCameraForTime(1.0f, CameraShakeType.ReduceOverTime);

        yield return new WaitForSeconds(0.3f);

        Instantiate(impactEffectPrefab, transform);

        abilityState = SlamAbilityState.LiftingEnemies;

        yield return new WaitForSeconds(6.0f);

        lerpProgress = 0.0f;

        abilityState = SlamAbilityState.DroppingEnemies;
    }

    protected override void Update()
    {
        base.Update();

        switch (abilityState)
        {
            case SlamAbilityState.LiftingEnemies:
                LiftEnemiesUpdate();
                break;

            case SlamAbilityState.DroppingEnemies:
                DropEnemiesUpdate();
                break;
        }
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

    private void DropEnemiesUpdate()
    {
        if(lerpProgress < 1.0f)
        {
            for (int i = 0; i < targetedEnemies.Count; i++)
            {
                if (targetedEnemies[i] != null)
                {
                    GameObject enemyGameObj = targetedEnemies[i].gameObject;

                    // Cubing lerpProgress to give ease-in effect for drop, mimicking gravity
                    enemyGameObj.transform.position = Vector3.Lerp(enemyLiftPositions[i], enemyStartPositions[i], lerpProgress * lerpProgress * lerpProgress);
                }
            }
        }
        else
        {
            EnemyEffectsDone();
        }

        lerpProgress += Time.deltaTime * DropSpeed;
    }

    private void EnemyEffectsDone()
    {
        for (int i = 0; i < targetedEnemies.Count; i++)
        {
            if (targetedEnemies[i] != null)
            {
                targetedEnemies[i].gameObject.transform.position = enemyStartPositions[i];
                targetedEnemies[i].StartAgentMovement();
            }
        }

        abilityState = SlamAbilityState.None;
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