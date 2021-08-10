using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;


// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Functionality for arena objectives (e.g. defeat [x] enemies before this door will open)
// Development window:  Production phase
// Inherits from:       MonoBehaviour

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class AreaObjectiveManager : MonoBehaviour
{
    private float objectiveCharge = 0.0f;   // Current objective "charge"
    private bool isCharging;                // Flags if player is within area & objective is "active"
    private bool hasFullyCharged = false;   // Flags if objective has been completed yet

    [Header("Enounter vars")]
    [SerializeField]    private int encounterDifficulty;        // Difficulty of the objective encounter
    [SerializeField]    private EnemyCampManager enemyManager;  // Ref. to EnemyCampManager that controls enemies & spawn
    [SerializeField]    private Transform searchPosition;       // Position enemies will first flock to when spawned

    [Header("UI Elements")]
    [SerializeField]    private Gradient progressBarGrad;       // Stores colour gradient for progress bar
    [SerializeField]    private Image sliderFill;               // Ref. to slider background fill
    [SerializeField]    Slider progressSlider;                  // Ref. to progress bar slider

    private bool hasInteracted = false;                         // Flags if player has interacted w/ the objective

    [Header("Events triggered once fully charged")]
    [SerializeField] private UnityEvent onChargedEvents = new UnityEvent();     // Functions to run when objective is complete

    void Start()
    {
        // Ensures trigger vol. is set up properly on start
        gameObject.GetComponent<Rigidbody>().isKinematic = true;
        gameObject.GetComponent<BoxCollider>().isTrigger = true;

        // Sets max slider value to default (1.0f) and disables UI element to 
        progressSlider.maxValue = 1f;
        progressSlider.gameObject.SetActive(false);
        
        // Ensures bools are correctly assigned from start
        isCharging = false;
        hasFullyCharged = false;
    }
    
    void Update()
    {
        // Runs update if player has interacted w/ the objective trigger
        if (hasInteracted && !hasFullyCharged)
        {
            // If objective is done, set charge to exactly 1.0f, run complete functions & set fullyCharged to true
            if (enemyManager.hasBeenDefeated)
            {

                objectiveCharge = 1.0f;
                onChargedEvents.Invoke();
                hasFullyCharged = true;
            }

            // Recalculates objectiveCharge based on # of remaining enemies
            objectiveCharge = ((float)enemyManager.totalSpawned - (float)enemyManager.remainingUnits) / (float)enemyManager.totalSpawned;

            // If area is still active adjust colour
            if(isCharging)
            {
                sliderFill.color = progressBarGrad.Evaluate(objectiveCharge);
                progressSlider.value = objectiveCharge;
            }
        }

    }

    // Calls when player enters "arena"
    private void OnTriggerEnter(Collider other)
    {
        if(other.transform.CompareTag("Player"))
        {
            // Flags charging as true & displays progress bar
            isCharging = true;
            progressSlider.gameObject.SetActive(true);
        }
    }

    // Calls when player leaves "arena"
    private void OnTriggerExit(Collider other)
    {
        if (other.transform.CompareTag("Player"))
        {
            // Flags charging as false & hides progress bar
            isCharging = false;
            progressSlider.gameObject.SetActive(false);
        }
    }

    // Starts enemy spawning
    public void StartEnounter()
    {
        // Spawns enemies using EnemyCampManager & difficulty specified above
        enemyManager.SpawnUnits(encounterDifficulty, searchPosition.position);
        hasInteracted = true;
    }
}
