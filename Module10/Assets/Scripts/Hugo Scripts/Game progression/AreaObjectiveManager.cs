using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class AreaObjectiveManager : MonoBehaviour
{
    [SerializeField]    private int noOfWaves = 3;
                        private int currentWave = 0;

    [SerializeField]    private float graceTimeBetweenRounds = 0.5f;

    [SerializeField]    private float chargeTime = 5.0f;
    [SerializeField]    private float objectiveCharge = 0.0f;
                        private bool isCharging;
                        private bool spawningNew = false;

    [SerializeField]    private int initialDifficulty;
    [SerializeField]    private int difficultyIncreasePerWave = 0;
    [SerializeField]    private EnemyCampManager enemyManager;

    private WaitForSeconds wait;
    

    void Start()
    {
        gameObject.GetComponent<Rigidbody>().isKinematic = true;
        gameObject.GetComponent<BoxCollider>().isTrigger = true;

        wait = new WaitForSeconds(graceTimeBetweenRounds);

        isCharging = false;
    }
    
    void Update()
    {
        if(isCharging)
        {
            objectiveCharge += Time.deltaTime / chargeTime;
            if(objectiveCharge > 1.0f)
            {
                objectiveCharge = 1.0f;
            }
        }
        else
        {
            objectiveCharge -= Time.deltaTime / chargeTime;
            if(objectiveCharge < 0.0f)
            {
                objectiveCharge = 0.0f;
            }
        }

        if(enemyManager.hasBeenDefeated && !spawningNew && currentWave < noOfWaves)
        {
            spawningNew = true;
            currentWave += 1;
            StartCoroutine(waitAndStartNextRound());
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.transform.CompareTag("Player"))
        {
            isCharging = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.CompareTag("Player"))
        {
            isCharging = false;
        }
    }

    IEnumerator waitAndStartNextRound()
    {
        yield return wait;
        enemyManager.SpawnUnits(initialDifficulty + (currentWave * difficultyIncreasePerWave));
        spawningNew = false;
    }
}
