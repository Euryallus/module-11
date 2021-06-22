using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HazardQTE : MonoBehaviour, IExternalTriggerListener
{
    [Space]
    [Header("(See tooltips for info)")]
    [Header("Hazard QTE")]

    [SerializeField] private bool           isLoseable;
    [SerializeField] private KeyCode        keyToPress;
    [SerializeField] private float          timeScale = 0.05f;
    [SerializeField] private float          timeBeforeFail = 2.0f;
    [SerializeField] private float          successCameraDelay = 1.0f;
    [SerializeField] private Transform      playerSnapPoint;
    [SerializeField] private SoundClass     triggerSound;
    [SerializeField] private UnityEvent[]   failEvents;             // Events triggered when the QTE is failed, if it's loseable
    [SerializeField] private UnityEvent[]   successEvents;          // Events triggered when the QTE is completed successfully 
    [SerializeField] private GameObject     cutsceneCamera;        
    [SerializeField] private CameraShake    cutsceneCameraShake;        
    [SerializeField] private GameObject     qtePromptPrefab;        

    [SerializeField] private ExternalTrigger qteTrigger;

    private GameObject          mainCameraGameObj;
    private PlayerMovement      playerMovement;
    private CharacterController playerCharControl;
    private bool                qteTriggered;
    private float               qteTimer;
    private QTEPrompt           qtePromptUI;

    private void Awake()
    {
        qteTrigger.AddListener(this);
    }

    private void Start()
    {
        cutsceneCamera.SetActive(false);

        GameObject playerGameObj = GameObject.FindGameObjectWithTag("Player");

        playerMovement      = playerGameObj.GetComponent<PlayerMovement>();
        playerCharControl   = playerGameObj.GetComponent<CharacterController>();
    }

    private void Update()
    {
        if(qteTriggered)
        {
            Time.timeScale = Mathf.Lerp(Time.timeScale, timeScale, Time.unscaledDeltaTime * 20.0f);

            qteTimer += Time.unscaledDeltaTime;

            if(isLoseable)
            {
                if (qteTimer > timeBeforeFail)
                {
                    QTEFailed();
                }
                else
                {
                    qtePromptUI.SetIndicatorProgress(qteTimer / timeBeforeFail);
                }
            }

            if(Input.GetKeyDown(keyToPress))
            {
                QTESuccess();

                AudioManager.Instance.PlaySoundEffect2D("notification2");
            }
        }
    }

    public void OnExternalTriggerEnter(string triggerId, Collider other)
    {
        if(!qteTriggered && other.CompareTag("Player"))
        {
            if (triggerId == "triggerArea")
            {
                TriggerQTE();
            }
        }
    }

    public void OnExternalTriggerExit(string triggerId, Collider other) { }

    public void OnExternalTriggerStay(string triggerId, Collider other) { }

    private void TriggerQTE()
    {
        SwitchToQTEView();

        if (triggerSound != null)
        {
            AudioManager.Instance.PlaySoundEffect2D(triggerSound);
        }

        qteTimer     = 0.0f;
        qteTriggered = true;
    }

    private void SwitchToQTEView()
    {
        if (Camera.main != null)
        {
            mainCameraGameObj = Camera.main.gameObject;
            mainCameraGameObj.SetActive(false);
        }

        cutsceneCamera.SetActive(true);

        GameSceneUI gameUI = GameSceneUI.Instance;

        gameUI.SetUIShowing(false);
        gameUI.ShowCinematicsCanvas();

        playerMovement.StopMoving();

        if (playerSnapPoint != null)
        {
            playerCharControl.enabled = false;

            playerMovement.gameObject.transform.position = playerSnapPoint.position;

            playerCharControl.enabled = true;
        }

        qtePromptUI = Instantiate(qtePromptPrefab, gameUI.GetActiveCinematicsCanvas().transform).GetComponent<QTEPrompt>();
        qtePromptUI.SetKeyText(keyToPress.ToString());
    }

    private IEnumerator EndQTEViewCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        cutsceneCamera.SetActive(false);
        mainCameraGameObj.SetActive(true);

        GameSceneUI gameUI = GameSceneUI.Instance;

        gameUI.SetUIShowing(true);
        gameUI.HideCinematicsCanvas();

        playerMovement.StartMoving();
    }

    private void QTEDone(float endViewDelay)
    {
        Time.timeScale = 1.0f;

        qteTriggered = false;

        qtePromptUI.SetIndicatorProgress(1.0f);
        qtePromptUI.PlayPressAnimation();

        cutsceneCameraShake.ShakeCameraForTime(0.3f, CameraShakeType.ReduceOverTime);

        StartCoroutine(EndQTEViewCoroutine(endViewDelay));
    }

    private void QTEFailed()
    {
        QTEDone(0.0f);

        // Invoke all fail events
        foreach (UnityEvent failEvent in failEvents)
        {
            failEvent.Invoke();
        }
    }

    private void QTESuccess()
    {
        QTEDone(successCameraDelay);

        // Invoke all success events
        foreach (UnityEvent successEvent in successEvents)
        {
            successEvent.Invoke();
        }
    }
}