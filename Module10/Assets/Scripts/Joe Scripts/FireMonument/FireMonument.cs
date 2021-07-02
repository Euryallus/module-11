using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class FireMonument : MonoBehaviour, IPersistentSceneObject
{
    [Header("Main")]
    [Header("Fire Monument")]

    [SerializeField] private FireMonumentInteraction    interaction;
    [SerializeField] private ParticleSystem             fireParticles;
    [SerializeField] private GameObject                 lightIndicator;
    [SerializeField] private Transform                  cutsceneCameraParent;
    [SerializeField] private GameObject                 cutsceneCamera;
    [SerializeField] private Animator                   cutsceneAnimator;
    [SerializeField] private GameObject                 portalUnlockPanelPrefab;
    [SerializeField] private string                     unlockAreaName;

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

    private bool lit;
    private GameObject mainCameraGameObj;

    private void Start()
    {
        cutsceneAnimator.enabled = false;
        cutsceneCamera.SetActive(false);

        // Subscribe to save/load events so the fire monument's data will be saved/loaded with the game
        SaveLoadManager.Instance.SubscribeSceneSaveLoadEvents(OnSceneSave, OnSceneLoadSetup, OnSceneLoadConfigure);
    }

    private void OnDestroy()
    {
        // Unsubscribe from save/load events to prevent null ref errors if the monument is destroyed
        SaveLoadManager.Instance.UnsubscribeSceneSaveLoadEvents(OnSceneSave, OnSceneLoadSetup, OnSceneLoadConfigure);
    }

    public void OnInteract()
    {
        LightFire();

        StartCutscene();

        if(!string.IsNullOrEmpty(villagePortalId))
        {
            PortalsSave.Instance.SetPortalShowing(villagePortalId, true);
        }
    }

    private void StartCutscene()
    {
        cutsceneCameraParent.transform.localPosition = Vector3.zero;
        cutsceneCameraParent.transform.rotation = Quaternion.identity;

        if (Camera.main != null)
        {
            mainCameraGameObj = Camera.main.gameObject;
            mainCameraGameObj.SetActive(false);
        }

        cutsceneCamera.SetActive(true);
        cutsceneAnimator.enabled = true;

        GameSceneUI gameUI = GameSceneUI.Instance;

        gameUI.SetUIShowing(false);
        gameUI.ShowCinematicsCanvas();

        // Fade music/sounds out over 0.2 seconds
        AudioManager.Instance.FadeGlobalVolumeMultiplier(0.0f, 0.2f);

        AudioManager.Instance.PlayMusicInterlude("fireLightCutscene");

        CinematicsCanvas cinematicsCanvas = gameUI.GetActiveCinematicsCanvas();

        if(unlockVideoClip != null)
        {
            cinematicsCanvas.SetupVideoPlayer(unlockVideoClip);
        }
    }

    // Called by an animation event
    private void EndCutscene()
    {
        cutsceneCamera.SetActive(false);
        mainCameraGameObj.SetActive(true);

        GameSceneUI gameUI = GameSceneUI.Instance;

        gameUI.SetUIShowing(true);
        gameUI.HideCinematicsCanvas();

        AudioManager.Instance.FadeGlobalVolumeMultiplier(1.0f, 1.0f);
    }

    // Called during cutscene by an animation event
    private void CutsceneFadeOut()
    {
        GameSceneUI.Instance.GetActiveCinematicsCanvas().FadeToBlack();
    }

    // Called during cutscene by an animation event
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

    public void OnSceneSave(SaveData saveData)
    {
        saveData.AddData("fireMonumentLit_" + GetUniquePositionId(), lit);
    }

    public void OnSceneLoadSetup(SaveData saveData)
    {
        lit = saveData.GetData<bool>("fireMonumentLit_" + GetUniquePositionId());

        if(lit)
        {
            LightFire();
        }
    }

    public void OnSceneLoadConfigure(SaveData saveData) { }

    private void LightFire()
    {
        lit = true;

        interaction.CanInteract = false;

        lightIndicator.SetActive(false);

        StartCoroutine(LightFireEffectsCoroutine());
    }

    private IEnumerator LightFireEffectsCoroutine()
    {
        yield return new WaitForSecondsRealtime(0.2f);

        AudioManager.Instance.PlayLoopingSoundEffect("fireLoop", "fireLoop_" + GetUniquePositionId(), true, transform.position);

        fireParticles.Play();
    }

    private string GetUniquePositionId()
    {
        return transform.position.x + "_" + transform.position.y + "_" + transform.position.z;
    }
}
