using UnityEngine;

public class CutsceneTriggerer : MonoBehaviour
{
    [Header("Cutscene Triggerer")]

    [SerializeField] protected  Transform  cutsceneCameraParent;
    [SerializeField] private    GameObject cutsceneCamera;
    [SerializeField] private    Animator   cutsceneAnimator;

    protected   PlayerMovement  playerMovement;
    private     GameObject      mainCameraGameObj;

    protected virtual void Start()
    {
        // Hide/disable the cutscene camera and animator by default
        cutsceneAnimator.enabled = false;
        cutsceneCamera.SetActive(false);

        playerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
    }

    protected virtual void StartCutscene()
    {
        // Stop the player from moving and disable the character controller to avoid
        //   collisions/trigger events form occuring during the cutscene
        playerMovement.StopMoving();
        playerMovement.Controller.enabled = false;

        // Hide the main (player view) camera
        mainCameraGameObj = playerMovement.GetPlayerCamera();
        mainCameraGameObj.SetActive(false);

        // Show/enable the camera used to show the cutscene
        cutsceneCamera.SetActive(true);
        cutsceneAnimator.enabled = true;

        // Hide the main UI and show the cinematics canvas (an overlay containing cinematic black bars)
        GameSceneUI gameUI = GameSceneUI.Instance;

        gameUI.SetUIShowing(false);
        gameUI.ShowCinematicsCanvas();
    }

    protected virtual void EndCutscene()
    {
        // Disable the cutscene camera and re-enable the main/player camera
        cutsceneCamera.SetActive(false);
        mainCameraGameObj.SetActive(true);

        // Re-show the main game UI and hide the cinematics canvas
        GameSceneUI gameUI = GameSceneUI.Instance;

        gameUI.SetUIShowing(true);
        gameUI.HideCinematicsCanvas();

        // Allow the player to move again
        playerMovement.Controller.enabled = true;
        playerMovement.StartMoving();
    }
}
