using UnityEngine;

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
    private Animator            animator;

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

    [Header("Please use a 512x512 sprite for the emblem")]
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

        Material portalMaterialInstance = new Material(portalMaterial);
        portalMaterialInstance.SetColor("_Tint", portalColour);
        portalRenderer.material = portalMaterialInstance;

        portalEmblemRenderers[0].sprite = portalEmblem;
        portalEmblemRenderers[1].sprite = portalEmblem;

        portalTrigger.AddListener(this);

        SetShowing(alwaysActive);

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
            SetShowing(PortalsSave.Instance.IsPortalShowing(GetSavePointId()));
        }
    }

    public void OnExternalTriggerEnter(string triggerId, Collider other)
    {
        if(other.CompareTag("Player") && triggerId == "portal")
        {
            SetAsUsed();

            SaveLoadManager.Instance.SaveGameData(sceneToLoadName);

            SaveLoadManager.Instance.LoadGameScene(sceneToLoadName);
        }
    }

    public void OnExternalTriggerStay(string triggerId, Collider other) { }

    public void OnExternalTriggerExit(string triggerId, Collider other) { }

    public void ShowWithAnimation()
    {
        animator.SetTrigger("Enter");

        SetShowing(true);
    }

    private void SetShowing(bool show)
    {
        portalTrigger.TriggerEnabled = show;

        animator.SetBool("Showing", show);

        PortalsSave.Instance.SetPortalShowing(GetSavePointId(), show);

        if(show && !playingSound)
        {
            AudioManager.Instance.PlayLoopingSoundEffect("portalLoop", "portalLoop_" + id, true, false, transform.position, 4.0f);
            playingSound = true;
        }
    }

    public Vector3 GetRespawnPosition()
    {
        return respawnTransform.position;
    }

    public string GetSavePointId()
    {
        return id;
    }

    public void SetAsUsed()
    {
        WorldSave.Instance.UsedSceneSavePointId = GetSavePointId();

        SaveLoadManager.SetLastUsedSavePoint(this);
    }

    public void SetAsUnused()
    {
    }
}