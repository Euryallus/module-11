using UnityEngine;

public class CutsceneTriggerer : MonoBehaviour
{
    [Header("Cutscene Triggerer")]

    [SerializeField] protected  Transform  cutsceneCameraParent;
    [SerializeField] private    GameObject cutsceneCamera;
    [SerializeField] private    Animator   cutsceneAnimator;

    protected   PlayerMovement  playerMovement;
    private     GameObject      mainCameraGameObj;

    private bool returnToPlayerMovement;

    protected virtual void Start()
    {
        // Hide/disable the cutscene camera and animator by default
        if(cutsceneAnimator != null)
        {
            cutsceneAnimator.enabled = false;
        }
        if(cutsceneCamera != null)
        {
            cutsceneCamera.SetActive(false);
        }

        playerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
    }

    public virtual void StartCutscene()
    {
        returnToPlayerMovement = playerMovement.PlayerCanMove();

        // Stop the player from moving and disable the character controller to avoid
        //   collisions/trigger events form occuring during the cutscene
        playerMovement.StopMoving();
        playerMovement.Controller.enabled = false;

        mainCameraGameObj = playerMovement.GetPlayerCamera();

        if (cutsceneCamera != null)
        {
            // Hide the main (player view) camera
            mainCameraGameObj.SetActive(false);

            // Show/enable the camera used to show the cutscene
            cutsceneCamera.SetActive(true);
        }

        if(cutsceneAnimator != null)
        {
            cutsceneAnimator.enabled = true;
        }

        // Hide the main UI and show the cinematics canvas (an overlay containing cinematic black bars)
        GameSceneUI gameUI = GameSceneUI.Instance;

        gameUI.SetUIShowing(false);
        gameUI.ShowCinematicsCanvas();
    }

    protected virtual void EndCutscene()
    {
        // Disable the cutscene camera and re-enable the main/player camera

        if(cutsceneCamera != null)
        {
            cutsceneCamera.SetActive(false);
        }

        mainCameraGameObj.SetActive(true);

        // Re-show the main game UI and hide the cinematics canvas
        GameSceneUI gameUI = GameSceneUI.Instance;

        gameUI.SetUIShowing(true);
        gameUI.HideCinematicsCanvas();

        // Re-enable the player controller
        playerMovement.Controller.enabled = true;

        // Allow the player to move again if they could move before triggering the cutscene
        if(returnToPlayerMovement)
        {
            playerMovement.StartMoving();
        }
    }
}
