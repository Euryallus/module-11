using UnityEngine;

// ||=======================================================================||
// || AutoSaveArea: A trigger area that saves the game if the player        ||
// ||   enters it. Can be disabled after being used once.                   ||
// ||=======================================================================||
// || Used on prefab: Joe/Environment/AutoSaveArea                          ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

[RequireComponent(typeof(BoxCollider))]
public class AutoSaveArea : MonoBehaviour, ISavePoint, IPersistentObject
{
    #region InspectorVariables
    // Variables in this region are set in the inspector. See tooltips for more info.

    [SerializeField] [Header("Important: Set unique id")]
    [Tooltip("Unique id for this save point. Important: all save points should use a different id.")]
    private string id;

    [SerializeField]
    private bool disableWhenUsed = true;    //Whether the trigger should be permenantly disabled after being used once

    #endregion

    #region Properties

    public string Id { get { return id; } }

    #endregion

    private bool        colliderDisabled;   // Whether the collider is currently disabled
    private BoxCollider boxCollider;        // The collider for player detection

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        colliderDisabled = !boxCollider.enabled;
    }

    private void Start()
    {
        // Subscribe to save/load events so colliderDisabled is saved/loaded with the game
        SaveLoadManager.Instance.SubscribeSaveLoadEvents(OnSave, OnLoadSetup, OnLoadConfigure);

        if (string.IsNullOrEmpty(id))
        {
            // Warning if an id has not been set
            Debug.LogWarning("IMPORTANT: AutoSaveArea exists without id. All save points require a *unique* id for saving/loading data. Click this message to view the problematic GameObject.", gameObject);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from save/load events if the GameObject is destroyed to prevent null reference errors
        SaveLoadManager.Instance.UnsubscribeSaveLoadEvents(OnSave, OnLoadSetup, OnLoadConfigure);
    }

    public void OnSave(SaveData saveData)
    {
        Debug.Log("Saving data for AutoSaveArea: " + id);

        // Save whether the collider is disabled
        saveData.AddData("saveColliderDisabled_" + id, colliderDisabled);
    }

    public void OnLoadSetup(SaveData saveData)
    {
        Debug.Log("Loading data for AutoSaveArea: " + id);

        // Load whether the collider was disabled, and enable/disable it based on this

        bool disableOnLoad = saveData.GetData<bool>("saveColliderDisabled_" + id);

        if (disableOnLoad)
        {
            DisableCollider();
        }
        else
        {
            EnableCollider();
        }
    }

    public void OnLoadConfigure(SaveData saveData) { } // Nothing to configure

    private void OnTriggerEnter(Collider other)
    {
        if (disableWhenUsed)
        {
            // The collider is set to be disabled on use, disable it
            DisableCollider();
        }

        Debug.Log("Attempting to save game at save auto save point: " + id);

        // Store the UsedSavePointId so the player can be restored to the save area when the game is next loaded
        WorldSave.Instance.UsedSavePointId = id;

        // Try to save the game
        bool saveSuccess = SaveLoadManager.Instance.SaveGame();

        // Show a notification to tell the player is the save was successful
        if (saveSuccess)
        {
            NotificationManager.Instance.AddNotificationToQueue(NotificationMessageType.AutoSaveSuccess);
        }
        else
        {
            NotificationManager.Instance.AddNotificationToQueue(NotificationMessageType.SaveError);
        }
    }

    public void DisableCollider()
    {
        // Disable the collider and mark is as such
        colliderDisabled    = true;
        boxCollider.enabled = false;
    }

    public void EnableCollider()
    {
        // Enable the collider and mark is as such
        colliderDisabled = false;
        boxCollider.enabled = true;
    }

    public string GetSavePointId()
    {
        return id;
    }

    public Vector3 GetRespawnPosition()
    {
        return transform.position;
    }
}
