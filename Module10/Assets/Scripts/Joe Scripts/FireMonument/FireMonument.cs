using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireMonument : MonoBehaviour, IPersistentObject
{
    [Header("Main")]
    [Header("Fire Monument")]
    [SerializeField] private FireMonumentInteraction    interaction;
    [SerializeField] private ParticleSystem             fireParticles;
    [SerializeField] private GameObject                 lightIndicator;
    [SerializeField] private SpriteRenderer[]           portalEmblemRenderers;
    [SerializeField] private GameObject                 cutsceneCamera;
    [SerializeField] private Animator                   cutsceneAnimator;

    [Header("Portal (Appears When Lit)")]
    [SerializeField]  [Tooltip("The portal GameObject")]
    private GameObject     portalGameObj;

    [SerializeField] [Tooltip("The MeshRenderer component on the portal")]
    private MeshRenderer   portalRenderer;

    [SerializeField] [Tooltip("The material used for the portal (base material before applying tint colour)")]
    private Material       portalMaterial;

    [SerializeField] [Tooltip("Colour used to tint the portal material")]
    private Color32        portalColour;

    [Header("Please use 512x512 sprite")]
    [SerializeField] [Tooltip("The sprite shown on the portal")]
    private Sprite         portalEmblem;

    private bool lit;
    private GameObject mainCameraGameObj;

    private void Start()
    {
        cutsceneAnimator.enabled = false;
        cutsceneCamera.SetActive(false);

        portalEmblemRenderers[0].sprite = portalEmblem;
        portalEmblemRenderers[1].sprite = portalEmblem;

        // Subscribe to save/load events so the fire monument's data will be saved/loaded with the game
        SaveLoadManager.Instance.SubscribeSaveLoadEvents(OnSave, OnLoadSetup, OnLoadConfigure);

        Material portalMaterialInstance = new Material(portalMaterial);
        portalMaterialInstance.SetColor("_Tint", portalColour);
        portalRenderer.material = portalMaterialInstance;
    }

    private void OnDestroy()
    {
        // Unsubscribe from save/load events to prevent null ref errors if the monument is destroyed
        SaveLoadManager.Instance.UnsubscribeSaveLoadEvents(OnSave, OnLoadSetup, OnLoadConfigure);
    }

    public void OnInteract()
    {
        LightFire();

        StartCutscene();
    }

    private void StartCutscene()
    {
        if(Camera.main != null)
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

    public void OnSave(SaveData saveData)
    {
        saveData.AddData("fireMonumentLit_" + GetUniquePositionId(), lit);
    }

    public void OnLoadSetup(SaveData saveData)
    {
        lit = saveData.GetData<bool>("fireMonumentLit_" + GetUniquePositionId());

        if(lit)
        {
            LightFire();

            // Show portal
            portalGameObj.transform.localPosition = new Vector3(3.05f, 0.0f, 0.0f);
        }
    }

    public void OnLoadConfigure(SaveData saveData) { }

    private void LightFire()
    {
        lit = true;

        AudioManager.Instance.PlayLoopingSoundEffect("fireLoop", "fireLoop_" + GetUniquePositionId(), true, transform.position);

        fireParticles.Play();

        lightIndicator.SetActive(false);

        interaction.CanInteract = false;
    }

    private string GetUniquePositionId()
    {
        return transform.position.x + "_" + transform.position.y + "_" + transform.position.z;
    }
}
