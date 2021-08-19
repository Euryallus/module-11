using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// ||=======================================================================||
// || HazardQTE: Handles quick time events that occur when the player       ||
// ||    enters a set trigger area (unused in the final game).              ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the production phase (Module 11).                                 ||
// ||=======================================================================||

// Note: This code predates the creation of the CutsceneTriggerer class, hence why it is not used.
//  The code was not worth updating to inherit from CutsceneTriggerer because it was already
//  decided before that code was written that QTE's would not be used in the final game.

public class HazardQTE : MonoBehaviour, IExternalTriggerListener
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [Space]
    [Header("(See tooltips for info)")]
    [Header("Hazard QTE")]

    [SerializeField] private bool           isLoseable;                 // Whether the QTE can be failed if time runs out
    [SerializeField] private KeyCode        keyToPress;                 // The key the player needs to press to complete the QTE
    [SerializeField] private float          timeScale = 0.05f;          // The speed of time to use when the QTE cutscene occurs
    [SerializeField] private float          timeBeforeFail = 2.0f;      // Number of seconds the player has before they fail (if isLoseable = true)
    [SerializeField] private float          successCameraDelay = 1.0f;  // Number of seconds to keep the cutscene camera active for after the QTE is completed 

    [SerializeField] private Transform      playerSnapPoint;        // Used to snap the player to a certain position when the QTE starts
    [SerializeField] private SoundClass     triggerSound;           // Sound that plays when the cutscene starts
    [SerializeField] private UnityEvent[]   failEvents;             // Events triggered when the QTE is failed, if it's loseable
    [SerializeField] private UnityEvent[]   successEvents;          // Events triggered when the QTE is completed successfully 
    [SerializeField] private GameObject     cutsceneCamera;         // Camera used for a 3rd person view in the cutscene
    [SerializeField] private CameraShake    cutsceneCameraShake;    // Handles the camera shake effect on the cutscene camera
    [SerializeField] private GameObject     qtePromptPrefab;        // The prompt that is instantiated to show the player what key to press

    [SerializeField] private ExternalTrigger qteTrigger;            // Detects when the player enters an area that should trigger the QTE

    #endregion

    private GameObject          mainCameraGameObj;  // Reference to the player camera so it can be re-enabled when the cutscene is over
    private bool                qteTriggered;       // Whether the QTE has been triggered
    private float               qteTimer;           // How long the QTE has been happening for once triggered (seconds)
    private QTEPrompt           qtePromptUI;        // The instantiated qtePromptPrefab

    private void Awake()
    {
        // Add this object as a listener of the trigger that detects when the player enters the QTE trigger area
        qteTrigger.AddListener(this);
    }

    private void Start()
    {
        // Hide the cutscene camera by default
        cutsceneCamera.SetActive(false);
    }

    private void Update()
    {
        if(qteTriggered)
        {
            // The QTE was triggered

            // Slow down time to the target time scale
            Time.timeScale = Mathf.Lerp(Time.timeScale, timeScale, Time.unscaledDeltaTime * 20.0f);

            // Increment the QTE timer (using unscaled time since Time.timeScale will
            //  probably be significantly slowed down and not accurate to realtime)
            qteTimer += Time.unscaledDeltaTime;

            if(isLoseable)
            {
                // The QTE can be failed

                if (qteTimer > timeBeforeFail)
                {
                    // The player fails if the timer reaches/surpasses timeBeforeFail
                    QTEFailed();
                }
                else
                {
                    // Update the UI indicator that shows how long the player has until they fail
                    qtePromptUI.SetIndicatorProgress(qteTimer / timeBeforeFail);
                }
            }

            if(Input.GetKeyDown(keyToPress))
            {
                // Correct key pressed, the player succeeded

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
                // The player entered the trigger area, trigger the QTE cutscene
                TriggerQTE();
            }
        }
    }

    public void OnExternalTriggerExit(string triggerId, Collider other) { }

    public void OnExternalTriggerStay(string triggerId, Collider other) { }

    private void TriggerQTE()
    {
        // Switch to the cutscene camera view
        SwitchToQTEView();

        // Play a trigger sound if one was set
        if (triggerSound != null)
        {
            AudioManager.Instance.PlaySoundEffect2D(triggerSound);
        }

        // Reset the timer and mark the QTE as triggered
        qteTimer     = 0.0f;
        qteTriggered = true;
    }

    private void SwitchToQTEView()
    {
        // Disable the main/player camera
        if (Camera.main != null)
        {
            mainCameraGameObj = Camera.main.gameObject;
            mainCameraGameObj.SetActive(false);
        }

        // Enable the cutscene camera
        cutsceneCamera.SetActive(true);

        GameSceneUI gameUI = GameSceneUI.Instance;
        
        // Hide the main game UI and show the canvas used for cinematics
        gameUI.SetUIShowing(false);
        gameUI.ShowCinematicsCanvas();

        PlayerMovement playerMovement = PlayerInstance.ActivePlayer.PlayerMovement;

        // Don't allow the player to move while the QTE is happening
        playerMovement.StopMoving();

        if (playerSnapPoint != null)
        {
            // Move the player to the set snap point, disabling then re-enabling the
            //   character controller because otherwise the position change may be ignored

            playerMovement.Controller.enabled = false;

            playerMovement.gameObject.transform.position = playerSnapPoint.position;

            playerMovement.Controller.enabled = true;
        }

        // Show the UI prompt that tells the player which key to press
        qtePromptUI = Instantiate(qtePromptPrefab, gameUI.GetActiveCinematicsCanvas().transform).GetComponent<QTEPrompt>();
        qtePromptUI.SetKeyText(keyToPress.ToString());
    }

    private IEnumerator EndQTEViewCoroutine(float delay)
    {
        // Switches back to the player camera/stops the cutscene after a delay

        // Wait for the given delay
        yield return new WaitForSeconds(delay);

        // Disable the cutscene camera and re-enable the main player camera
        cutsceneCamera.SetActive(false);
        mainCameraGameObj.SetActive(true);

        GameSceneUI gameUI = GameSceneUI.Instance;

        // Re-show game UI and hide the cinematics canvas
        gameUI.SetUIShowing(true);
        gameUI.HideCinematicsCanvas();

        // Allow the player to move again
        PlayerInstance.ActivePlayer.PlayerMovement.StartMoving();
    }

    private void QTEDone(float endViewDelay)
    {
        // Called when the QTE is done, regardless of win/fail state

        // Reset timeScale back to its default value
        Time.timeScale = 1.0f;

        // The QTE is no longer triggered
        qteTriggered = false;

        // Show that progress is complete and play a short 'done' animation on the UI indicator
        qtePromptUI.SetIndicatorProgress(1.0f);
        qtePromptUI.PlayPressAnimation();

        // Shake the camera for added impact
        cutsceneCameraShake.ShakeCameraForTime(0.3f, CameraShakeType.ReduceOverTime);

        // End the cutscene after the given delay
        StartCoroutine(EndQTEViewCoroutine(endViewDelay));
    }

    private void QTEFailed()
    {
        // Instantly end the QTE
        QTEDone(0.0f);

        // Invoke all fail events if not null
        foreach (UnityEvent failEvent in failEvents)
        {
            failEvent?.Invoke();
        }
    }

    private void QTESuccess()
    {
        // End the QTE cutscene after the set success delay
        QTEDone(successCameraDelay);

        // Invoke all success events if not null
        foreach (UnityEvent successEvent in successEvents)
        {
            successEvent?.Invoke();
        }
    }
}