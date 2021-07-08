using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class FireMonument : CutsceneTriggerer, IPersistentSceneObject, ISavePoint, IExternalTriggerListener
{
    [Header("Main")]
    [Header("Fire Monument")]

    [SerializeField] private FireMonumentInteraction    interaction;
    [SerializeField] private ParticleSystem             fireParticles;
    [SerializeField] private GameObject                 lightIndicator;
    [SerializeField] private GameObject                 portalUnlockPanelPrefab;
    [SerializeField] private string                     unlockAreaName;
    [SerializeField] private Transform                  respawnTransform;           // Where the player will respawn if the game was last saved after the monument was lit
    [SerializeField] private ExternalTrigger            fireTrigger;

    [Header("For a portal in this scene:")]
    [Space]
    [Header("Connected Portal")]
    [SerializeField] [Tooltip("The portal that will appear when the torch is lit")]
    private Portal localConnectedPortal;

    [Header("For a portal in the village scene:")]
    [SerializeField] [Tooltip("The id of the portal that will appear in the village when the torch is lit")]
    private string villagePortalId;

    [SerializeField] [Tooltip("The video clip to be shown during the lighting cutscene. This should be a video of the portal appearing in the Village Scene")]
    private VideoClip unlockVideoClip;

    private bool            lit;
    private PlayerStats     playerStats;

    protected override void Start()
    {
        base.Start();

        GameObject playerGameObj = GameObject.FindGameObjectWithTag("Player");

        playerStats     = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>();

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
        // Kill the player if they enter the fire trigger when the monument is lit
        if (lit && other.gameObject.CompareTag("Player") && triggerId == "fire")
        {
            playerStats.DecreaseHealth(100.0f, PlayerDeathCause.Fire);
        }
    }

    public void OnExternalTriggerStay(string triggerId, Collider other) { }

    public void OnExternalTriggerExit(string triggerId, Collider other) { }

    public void OnSceneSave(SaveData saveData)
    {
        // Save whether the fire is lit
        saveData.AddData("fireMonumentLit_" + GetUniquePositionId(), lit);
    }

    public void OnSceneLoadSetup(SaveData saveData)
    {
        // Light the fire if it was lit when the game was last saved
        if (saveData.GetData<bool>("fireMonumentLit_" + GetUniquePositionId()))
        {
            LightFire();
        }
    }

    public void OnSceneLoadConfigure(SaveData saveData) { } // Nothing to configure

    public void OnInteract()
    {
        LightFire();

        StartCutscene();

        if(!string.IsNullOrEmpty(villagePortalId))
        {
            PortalsSave.Instance.SetPortalShowing(villagePortalId, true);
        }
    }

    private void LightFire()
    {
        lit = true;

        interaction.CanInteract = false;

        lightIndicator.SetActive(false);

        StartCoroutine(LightFireEffectsCoroutine());
    }

    private IEnumerator LightFireEffectsCoroutine()
    {
        // Show fire particles and play the lighting sound effect after a small delay
        yield return new WaitForSecondsRealtime(0.2f);

        AudioManager.Instance.PlayLoopingSoundEffect("fireLoop", "fireLoop_" + GetUniquePositionId(), true, transform.position);
        fireParticles.Play();
    }

    protected override void StartCutscene()
    {
        base.StartCutscene();

        cutsceneCameraParent.transform.localPosition = Vector3.zero;
        cutsceneCameraParent.transform.rotation = Quaternion.identity;

        // Fade music/sounds out over 0.2 seconds
        AudioManager.Instance.FadeGlobalVolumeMultiplier(0.0f, 0.2f);

        AudioManager.Instance.PlayMusicInterlude("fireLightCutscene");

        CinematicsCanvas cinematicsCanvas = GameSceneUI.Instance.GetActiveCinematicsCanvas();

        if(unlockVideoClip != null)
        {
            cinematicsCanvas.SetupVideoPlayer(unlockVideoClip);
        }
    }

    // Called during the fire lighting cutscene by an animation event
    private void CutsceneFadeOut()
    {
        GameSceneUI.Instance.GetActiveCinematicsCanvas().FadeToBlack();
    }

    // Called during the fire lighting cutscene by an animation event
    private void FocusCutsceneOnPortal()
    {
        CinematicsCanvas cinematicsCanvas = GameSceneUI.Instance.GetActiveCinematicsCanvas();

        PortalUnlockPanel unlockPanel = Instantiate(portalUnlockPanelPrefab, cinematicsCanvas.transform).GetComponent<PortalUnlockPanel>();
        unlockPanel.Setup(unlockAreaName, localConnectedPortal == null);
        unlockPanel.transform.SetSiblingIndex(2);

        if (localConnectedPortal != null)
        {
            // The monument is connected to a portal in the same scene - animate the portal appearing in-engine
            
            localConnectedPortal.ShowWithAnimation();

            cutsceneCameraParent.transform.position = localConnectedPortal.MainPortalTransform.position - localConnectedPortal.MainPortalTransform.localPosition;
            cutsceneCameraParent.transform.rotation = localConnectedPortal.MainPortalTransform.rotation;

            CameraShake cutsceneCameraShake = cutsceneCameraParent.GetComponent<CameraShake>();

            cutsceneCameraShake.UpdateBasePosition(Vector3.zero);
            cutsceneCameraShake.ShakeCameraForTime(4.3f, CameraShakeType.ReduceOverTime, 0.05f, 0.025f);

        }
        else
        {
            // The connected portal is in the Village Scene, play a video of the portal emerging instead
            cinematicsCanvas.PlayVideo();
        }
    }

    // Called at the end of the fire lighting cutscene by an animation event
    protected override void EndCutscene()
    {
        base.EndCutscene();

        // Fade back in background music
        AudioManager.Instance.FadeGlobalVolumeMultiplier(1.0f, 1.0f);

        // Save the game now the monument is lit and a portal has been activated
        WorldSave.Instance.UsedSavePointId = GetSavePointId();
        SaveLoadManager.Instance.SaveGameData();
    }

    private string GetUniquePositionId()
    {
        return transform.position.x + "_" + transform.position.y + "_" + transform.position.z;
    }

    public string GetSavePointId()
    {
        return GetUniquePositionId();
    }

    public Vector3 GetRespawnPosition()
    {
        return respawnTransform.position;
    }
}
