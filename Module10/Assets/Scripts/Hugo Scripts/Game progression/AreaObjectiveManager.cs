using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class AreaObjectiveManager : MonoBehaviour
{
    [Header("Charge times")]
    [SerializeField]    private float chargeTime = 5.0f;
    [SerializeField]    private float drainTime = 10f;
    [SerializeField]    private float objectiveCharge = 0.0f;
                        private bool isCharging;
                        private bool hasFullyCharged = false;

    [Header("Enounter vars")]
    [SerializeField]    private int encounterDifficulty;
    [SerializeField]    private EnemyCampManager enemyManager;
    [SerializeField]    private Transform searchPosition;

    [Header("UI Elements")]
    [SerializeField]    private Gradient progressBarGrad;
    [SerializeField]    private Image sliderFill;
    [SerializeField]    Slider progressSlider;

    private bool hasInteracted = false;

    [Header("Events triggered once fully charged")]
    [SerializeField] private UnityEvent onChargedEvents = new UnityEvent();
    

    void Start()
    {
        gameObject.GetComponent<Rigidbody>().isKinematic = true;
        gameObject.GetComponent<BoxCollider>().isTrigger = true;

        //progressSlider.gameObject.SetActive(false);
        isCharging = false;
        hasFullyCharged = false;
    }
    
    void Update()
    {
        if (!hasFullyCharged && hasInteracted)
        {
            if (isCharging)
            {
                progressSlider.gameObject.SetActive(true);

                objectiveCharge += Time.deltaTime / chargeTime;
                if (objectiveCharge > 1.0f)
                {
                    objectiveCharge = 1.0f;
                    hasFullyCharged = true;
                    onChargedEvents.Invoke();
                    hasFullyCharged = true;
                }
            }
            else
            {
                objectiveCharge -= Time.deltaTime / drainTime;

                if (objectiveCharge < 0.0f)
                {
                    objectiveCharge = 0.0f;
                    progressSlider.gameObject.SetActive(false);
                }
            }

            sliderFill.color = progressBarGrad.Evaluate(objectiveCharge);
            progressSlider.value = objectiveCharge;
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

    public void StartEnounter()
    {
        hasInteracted = true;
        enemyManager.SpawnUnits(encounterDifficulty, searchPosition.position);
    }
}
