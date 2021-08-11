using UnityEngine;

// ||=======================================================================||
// || CutsceneTriggerer: When added to a GameObject, allows it to trigger   ||
// ||   a cutscene that temporarily hides UI/disabled player control.       ||
// ||=======================================================================||
// || Used on various prefabs                                               ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the production phase (Module 11).                                 ||
// ||=======================================================================||

public class CutsceneTriggerer : MonoBehaviour
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [Header("Cutscene Triggerer")]

    [SerializeField] protected  Transform  cutsceneCameraParent;    // Parent transform of the camera below
    [SerializeField] private    GameObject cutsceneCamera;          // The camera used for the cutscene
    [SerializeField] private    Animator   cutsceneAnimator;        // Animator that controls all movement in the cutscene

    #endregion

    private GameObject  mainCameraGameObj;      // Reference to the player's camera GameObject
    private bool        returnToPlayerMovement; // Whether player movement should be enabled when the cutscene is over

    protected virtual void Start()
    {
        // Hide/disable the cutscene animator and camera by default

        if(cutsceneAnimator != null)
        {
            cutsceneAnimator.enabled = false;
        }

        if(cutsceneCamera != null)
        {
            cutsceneCamera.SetActive(false);
        }
    }

    public virtual void StartCutscene()
    {
        PlayerMovement playerMovement = PlayerInstance.ActivePlayer.PlayerMovement;

        // Only allow the player to move when the cutscene is done if they can before it starts
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

        // Hide game UI and show the cinematics canvas

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

        // Re-enable the player character controller

        PlayerMovement playerMovement = PlayerInstance.ActivePlayer.PlayerMovement;

        playerMovement.Controller.enabled = true;

        // Allow the player to move again if they could move before triggering the cutscene
        if(returnToPlayerMovement)
        {
            playerMovement.StartMoving();
        }
    }
}