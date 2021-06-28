using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [Header("Connected Portal")]
    [SerializeField] [Tooltip("The portal that will appear when the torch is lit")]
    private Portal connectedPortal;

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
    }

    // Called by an animation event
    private void EndCutscene()
    {
        cutsceneCamera.SetActive(false);
        mainCameraGameObj.SetActive(true);

        GameSceneUI gameUI = GameSceneUI.Instance;

        gameUI.SetUIShowing(true);
        gameUI.HideCinematicsCanvas();
    }

    // Called during cutscene by an animation event
    private void CutsceneFadeOut()
    {
        GameSceneUI.Instance.GetActiveCinematicsCanvas().FadeToBlack();
    }

    // Called during cutscene by an animation event
    private void FocusCutsceneOnPortal()
    {
        if(connectedPortal != null)
        {
            cutsceneCameraParent.transform.position = connectedPortal.MainPortalTransform.position;
            cutsceneCameraParent.transform.rotation = connectedPortal.MainPortalTransform.rotation;

            CameraShake cutsceneCameraShake = cutsceneCameraParent.GetComponent<CameraShake>();

            cutsceneCameraShake.UpdateBasePosition(-connectedPortal.MainPortalTransform.localPosition);
            cutsceneCameraShake.ShakeCameraForTime(4.0f, CameraShakeType.ReduceOverTime, 0.02f);

            if (connectedPortal != null)
            {
                connectedPortal.ShowWithAnimation();
            }
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

        StartCoroutine(LightFireCoroutine());
    }

    private IEnumerator LightFireCoroutine()
    {
        lightIndicator.SetActive(false);

        yield return new WaitForSecondsRealtime(0.1f);

        AudioManager.Instance.PlayLoopingSoundEffect("fireLoop", "fireLoop_" + GetUniquePositionId(), true, transform.position);

        fireParticles.Play();
    }

    private string GetUniquePositionId()
    {
        return transform.position.x + "_" + transform.position.y + "_" + transform.position.z;
    }
}
