using UnityEngine;

// ||=======================================================================||
// || Portal: Allows the player to move between areas/scenes. Portals can   ||
// ||    be hidden, and generally emerge when a FireMonument is lit.        ||
// ||=======================================================================||
// || Used on prefab: Joe/Environment/Portal                                ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the production phase (Module 11).                                 ||
// ||=======================================================================||

public class Portal : MonoBehaviour, ISavePoint, IExternalTriggerListener, IPersistentSceneObject
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    // See tooltips for comments

    [Space]
    [Header("Important: Set unique id")]
    [Header("Portal")]

    [SerializeField] [Tooltip("Unique id for this portal")]
    private string              id;

    [SerializeField] [Tooltip("If true, the portal will always be visible/usable. Otherwise, it will appear after a FireMonument with this set as the connected portal is activated")]
    private bool                alwaysActive = false;

    [SerializeField] [Tooltip("Name of the scene to be loaded when this portal is entered")]
    private string              sceneToLoadName;

    [SerializeField] [Tooltip("Transform used for positioning the player after entering the scene through this portal")]
    private Transform           respawnTransform;

    [SerializeField]
    private Animator            animator; // Handles portal animations

    [SerializeField] [Tooltip("The parent transform of all portal meshes to be moved when hiding/showing the portal")]
    private Transform           mainPortalTransform;

    [SerializeField] [Tooltip("The ExternalTrigger that detects the player entering the portal")]
    private ExternalTrigger     portalTrigger;

    [SerializeField] [Tooltip("The MeshRenderer component on the portal")]
    private MeshRenderer        portalRenderer;

    [SerializeField] [Tooltip("The material used for the portal (base material before applying tint colour)")]
    private Material            portalMaterial;

    [SerializeField] [Tooltip("Colour used to tint the portal material")]
    private Color32             portalColour;

    [SerializeField] [Tooltip("The renderers that display the portal emblem sprite")]
    private SpriteRenderer[]    portalEmblemRenderers;

    [SerializeField] [Tooltip("The sprite shown on each side of the portal")]
    private Sprite              portalEmblem;

    #endregion

    #region Properties

    public Transform MainPortalTransform { get { return mainPortalTransform; } }

    #endregion

    private bool playingSound;

    private void Start()
    {
        // Subscribe to save/load events so the fire monument's data will be saved/loaded with the game
        SaveLoadManager.Instance.SubscribeSceneSaveLoadEvents(OnSceneSave, OnSceneLoadSetup, OnSceneLoadConfigure);

        // Create a new instance of the portal material, set its colour and apply it to the renderer
        Material portalMaterialInstance = new Material(portalMaterial);
        portalMaterialInstance.SetColor("_Tint", portalColour);
        portalRenderer.material = portalMaterialInstance;
        
        // Display the set emblem sprite on each side of the portal
        portalEmblemRenderers[0].sprite = portalEmblem;
        portalEmblemRenderers[1].sprite = portalEmblem;

        // Add this class as a listener of the portal trigger which detects when the player enters the portal
        portalTrigger.AddListener(this);

        // Show the portal by default if it's marked as always active, otherwise hide it by default
        SetShowing(alwaysActive);

        // Throw an error if an id was not set, which will break save/load code
        if(string.IsNullOrWhiteSpace(id))
        {
            Debug.LogError("IMPORTANT: Portal exists without id. All portals require a *unique* id for saving/loading data. Click this message to view the problematic GameObject.", gameObject);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from save/load events to prevent null ref errors if the object is destroyed
        SaveLoadManager.Instance.UnsubscribeSceneSaveLoadEvents(OnSceneSave, OnSceneLoadSetup, OnSceneLoadConfigure);
    }

    public void OnSceneSave(SaveData saveData) { }

    public void OnSceneLoadSetup(SaveData saveData) { }

    public void OnSceneLoadConfigure(SaveData saveData)
    {
        if(!alwaysActive)
        {
            // Get whether this portal should be showing from PortalSave, and show/hide it accordingly
            SetShowing(PortalsSave.Instance.IsPortalShowing(GetSavePointId()));
        }
    }

    public void OnExternalTriggerEnter(string triggerId, Collider other)
    {
        if(other.CompareTag("Player") && triggerId == "portal")
        {
            // The player entered the portal

            // Set this as the last used save point
            SetAsUsed();

            // Save the game, ensuring the player will be spawned in the scene with name: sceneToLoadName
            //   if they respawn after dying or exit the game after using the portal and reload later
            SaveLoadManager.Instance.SaveGameData(sceneToLoadName);

            // Load data for the new scene
            SaveLoadManager.Instance.LoadGameScene(sceneToLoadName);
        }
    }

    public void OnExternalTriggerStay(string triggerId, Collider other) { }

    public void OnExternalTriggerExit(string triggerId, Collider other) { }

    public void ShowWithAnimation()
    {
        // Animate the portal emerging from the ground
        animator.SetTrigger("Enter");

        // The portal is now showing
        SetShowing(true);
    }

    private void SetShowing(bool show)
    {
        // Enable the trigger if the portal is being shown, otherwise disable it
        portalTrigger.TriggerEnabled = show;
        
        // Tell the animator if the portal is showing, determines its position when idle
        animator.SetBool("Showing", show);

        // Tell the PortalsSave whether this portal is showing, so that info can be saved/loaded in any scene
        PortalsSave.Instance.SetPortalShowing(GetSavePointId(), show);

        if(show && !playingSound)
        {
            // The portal is being shown and sound is not already playing, play a looping portal sound
            AudioManager.Instance.PlayLoopingSoundEffect("portalLoop", "portalLoop_" + id, true, false, transform.position, 4.0f);
            playingSound = true;
        }
    }

    public Vector3 GetRespawnPosition()
    {
        // Returns the position to spawn the player at if they last saved at this portal
        return respawnTransform.position;
    }

    public string GetSavePointId()
    {
        // Returns the unique id used for saving
        return id;
    }

    public void SetAsUsed()
    {
        WorldSave.Instance.UsedSceneSavePointId = GetSavePointId();

        // Set this portal to be the last save point that was used
        SaveLoadManager.SetLastUsedSavePoint(this);
    }

    public void SetAsUnused()
    {
    }
}