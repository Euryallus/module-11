using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

// ||=======================================================================||
// || FireMonument: A large torch that, when lit, triggers a cutscene       ||
// ||    where a portal to a new area is revealed (or ends the game).       ||
// ||=======================================================================||
// || Used on prefab: Joe/Environment/FireMonument                          ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the production phase (Module 11).                                 ||
// ||=======================================================================||

public class FireMonument : CutsceneTriggerer, IPersistentSceneObject, ISavePoint, IExternalTriggerListener
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [Header("Main")]
    [Header("Fire Monument")]

    [SerializeField] private FireMonumentInteraction    interaction;                // The object the player interacts with to light this monument
    [SerializeField] private ParticleSystem             fireParticles;              // Particles shown when the monument is lit
    [SerializeField] private GameObject                 lightIndicator;             // The indicator that hovers above the monument when it hasn't been lit
    [SerializeField] private GameObject                 portalUnlockPanelPrefab;    // The panel instantiated during the cutscene that tells the player which portal was unlocked
    [SerializeField] private string                     unlockAreaName;             // The name of the area being unlocked that will be displayed during the cutscene
    [SerializeField] private Transform                  respawnTransform;           // Determines where the player will respawn if the game was last saved after the monument was lit
    [SerializeField] private ExternalTrigger            fireTrigger;                // The trigger that detects if/when the player stands in the lit fire
    [SerializeField] private bool                       isLastMonument;             // If true, this monument will end the game when lit instead of revealing a portal

    [Header("For a portal in this scene:")]
    [Space]
    [Header("Connected Portal")]

    // See tooltips for comments

    [SerializeField] [Tooltip("The portal that will appear in the active scene when the torch is lit")]
    private Portal localConnectedPortal;

    [Header("For a portal in the village scene:")]
    [SerializeField] [Tooltip("The id of the portal that will appear in the village when the torch is lit")]
    private string villagePortalId;

    [SerializeField] [Tooltip("The video clip to be shown during the lighting cutscene, as an alternative to showing a portal animation in-engine")]
    private VideoClip unlockVideoClip;

    #endregion

    private bool lit; // Whether the monument is currently lit

    protected override void Start()
    {
        base.Start();

        // Add this class as a listener of the ExternalTrigger that detects when the player steps into fire
        fireTrigger.AddListener(this);

        // Subscribe to save/load events so the fire monument's data will be saved/loaded with the game
        SaveLoadManager.Instance.SubscribeSceneSaveLoadEvents(OnSceneSave, OnSceneLoadSetup, OnSceneLoadConfigure);
    }

    private void OnDestroy()
    {
        // Unsubscribe from save/load events to prevent null ref errors if the monument is destroyed
        SaveLoadManager.Instance.UnsubscribeSceneSaveLoadEvents(OnSceneSave, OnSceneLoadSetup, OnSceneLoadConfigure);
    }

    public void OnExternalTriggerEnter(string triggerId, Collider other)
    {
        if (lit && other.gameObject.CompareTag("Player") && triggerId == "fire")
        {
            // The player entered the fire trigger when the monument was lit - remove a huge amount of health to kill them
            PlayerInstance.ActivePlayer.PlayerStats.DecreaseHealth(10000.0f, PlayerDeathCause.Fire);
        }
    }

    public void OnExternalTriggerStay(string triggerId, Collider other) { }

    public void OnExternalTriggerExit(string triggerId, Collider other) { }

    public void OnSceneSave(SaveData saveData)
    {
        // Save whether the fire is lit
        saveData.AddData("fireMonumentLit_" + GetSavePointId(), lit);
    }

    public void OnSceneLoadSetup(SaveData saveData)
    {
        // Light the fire if it was lit when the game was last saved
        if (saveData.GetData<bool>("fireMonumentLit_" + GetSavePointId()))
        {
            LightFire();
        }
    }

    public void OnSceneLoadConfigure(SaveData saveData) { } // Nothing to configure

    public bool OnInteract()
    {
        if(!PlayerInstance.ActivePlayer.PlayerMovement.OnPlatform)
        {
            // Only accept interactions if the player isn't still on the moving platform 
            //   leading to the torch - prevents errors caused by the player being a
            //   child of the platform while the cutscene plays

            // Light the fire
            LightFire();

            // Start the portal unlock cutscene (or ending cutscene if isLastMonument = true)
            StartCutscene();

            if (!string.IsNullOrEmpty(villagePortalId))
            {
                // This monument unlocks a portal in the village, set the portal as showing
                PortalsSave.Instance.SetPortalShowing(villagePortalId, true);
            }

            // An interaction occured
            return true;
        }

        // No interaction occured
        return false;
    }

    private void LightFire()
    {
        lit = true;

        // Disable interactions once the fire is lit and hide the light indicator
        interaction.CanInteract = false;

        lightIndicator.SetActive(false);

        // Start the coroutine that handles particle and audio effects
        StartCoroutine(LightFireEffectsCoroutine());
    }

    private IEnumerator LightFireEffectsCoroutine()
    {
        // Show fire particles and play the lighting sound effect after a small delay rather
        //   than triggering everything instantly which would make it difficult to notice
        //   the lighting process in the cutscene

        yield return new WaitForSecondsRealtime(0.2f);

        AudioManager.Instance.PlayLoopingSoundEffect("fireLoop", "fireLoop_" + GetSavePointId(), true, false, transform.position);
        fireParticles.Play();
    }

    public override void StartCutscene()
    {
        base.StartCutscene();

        // Move the cutscene camera to its default position
        cutsceneCameraParent.transform.localPosition = Vector3.zero;
        cutsceneCameraParent.transform.rotation = Quaternion.identity;

        // Fade music/sounds out over 0.2 seconds
        AudioManager.Instance.FadeGlobalVolumeMultiplier(0.0f, 0.2f);

        // Play either the standard fire cutscene music, or the ending music if this is the last monument
        AudioManager.Instance.PlayMusicalSoundEffect(isLastMonument ? "fireLightEndCutscene" : "fireLightCutscene");

        // Get the active cinematics canvas that is shown during cutscenes
        CinematicsCanvas cinematicsCanvas = GameSceneUI.Instance.GetActiveCinematicsCanvas();

        if(unlockVideoClip != null)
        {
            // Setup the unlock video if one was set in the inspector
            cinematicsCanvas.SetupVideoPlayer(unlockVideoClip);
        }

        PlayerMovement playerMovement = PlayerInstance.ActivePlayer.PlayerMovement;

        // Snap the player to the respawn position/rotation in case they are stood in the fire or elsewhere when the cutscene starts
        playerMovement.gameObject.transform.position = respawnTransform.position + Vector3.up;
        playerMovement.gameObject.transform.rotation = respawnTransform.rotation;
    }

    // Called during the fire lighting cutscene by an animation event
    private void CutsceneFadeOut()
    {
        GameSceneUI.Instance.GetActiveCinematicsCanvas().FadeToBlack();
    }

    // Called during the fire lighting cutscene by an animation event during standard cutscenes (when isLastMonument = false)
    private void FocusCutsceneOnPortal()
    {
        // Get the active cinematics canvas that is shown during cutscenes
        CinematicsCanvas cinematicsCanvas = GameSceneUI.Instance.GetActiveCinematicsCanvas();

        // Show the UI panel that tells the player a portal was unlocked
        PortalUnlockPanel unlockPanel = Instantiate(portalUnlockPanelPrefab, cinematicsCanvas.transform).GetComponent<PortalUnlockPanel>();
        unlockPanel.Setup(unlockAreaName, true);
        unlockPanel.transform.SetSiblingIndex(2);

        if (localConnectedPortal != null)
        {
            // The monument is connected to a portal in the same scene - animate the portal appearing in-engine
            localConnectedPortal.ShowWithAnimation();

            // Move the cutscene camera to the base of the portal being revealed
            cutsceneCameraParent.transform.position = localConnectedPortal.MainPortalTransform.position - localConnectedPortal.MainPortalTransform.localPosition;

            // Rotate the cutscene camera to face the poartal
            cutsceneCameraParent.transform.rotation = localConnectedPortal.MainPortalTransform.rotation;

            // Shake the camera as the portal emerges
            CameraShake cutsceneCameraShake = cutsceneCameraParent.GetComponent<CameraShake>();

            cutsceneCameraShake.UpdateBasePosition(Vector3.zero);
            cutsceneCameraShake.ShakeCameraForTime(4.3f, CameraShakeType.ReduceOverTime, 0.05f, 0.025f);

        }
        else
        {
            // There is no local portal that can be animated, play a video of the portal emerging instead
            cinematicsCanvas.PlayVideo();
        }
    }

    // Called at the end of the fire lighting cutscene by an animation event
    protected override void EndCutscene()
    {
        base.EndCutscene();

        // Fade back in background music
        AudioManager.Instance.FadeGlobalVolumeMultiplier(1.0f, 1.0f);

        // Set this as the last used save point
        SetAsUsed();

        // Save the game now the monument is lit and a portal has been activated so
        //   the player will respawn at the monument if they die before saving elsewhere
        SaveLoadManager.Instance.SaveGameData();
    }

    // Called at the end of the final monument cutscene by an animation event
    private void LastMonumentEvents()
    {
        StartCoroutine(LastMonumentEventsCoroutine());
    }

    private IEnumerator LastMonumentEventsCoroutine()
    {
        // The final cutscene is over - cut to black
        GameSceneUI.Instance.GetActiveCinematicsCanvas().CutToBlack();

        // Set this as the last used save point
        SetAsUsed();

        // Save the game as standard so the player's state in the
        //   active scene can be restored next time they visit
        SaveLoadManager.Instance.SaveGameData();

        yield return null;

        // Save a second time, but set the returnToScene to be the village, since the first time loading
        //   the game after completion, the player should be spawned in the village scene
        SaveLoadManager.Instance.SaveGameData("The Village");

        // Wait a few seconds for the cutscene music to end, and so the credits don't instantly start
        yield return new WaitForSecondsRealtime(4.0f);

        // Re-show the player's cursor now the cutscene is done
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Load the credits scene
        SceneManager.LoadScene("Credits");
    }

    public string GetSavePointId()
    {
        // Returns a unique id to use for saving the monument based on its position
        return transform.position.x + "_" + transform.position.y + "_" + transform.position.z;
    }

    public Vector3 GetRespawnPosition()
    {
        // Returns the position to spawn the player at if they last saved at this monument
        return respawnTransform.position;
    }

    public void SetAsUsed()
    {
        WorldSave.Instance.UsedSceneSavePointId = GetSavePointId();

        // Set this monument to be the last save point that was used
        SaveLoadManager.SetLastUsedSavePoint(this);
    }

    public void SetAsUnused()
    {
    }
}
