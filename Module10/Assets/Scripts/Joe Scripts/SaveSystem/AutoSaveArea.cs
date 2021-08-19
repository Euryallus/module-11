using UnityEngine;

// ||=======================================================================||
// || AutoSaveArea: A trigger area that saves the game if the player        ||
// ||   enters it. Can be disabled after being used once.                   ||
// ||=======================================================================||
// || Used on prefab: Joe/Environment/AutoSaveArea                          ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || originally for the prototype phase.                                   ||
// ||                                                                       ||
// || Changes made during the production phase (Module 11):                 ||
// ||                                                                       ||
// || - Implemented new functions from the ISavePoint interface             ||
// ||=======================================================================||

[RequireComponent(typeof(BoxCollider))]
public class AutoSaveArea : MonoBehaviour, ISavePoint, IPersistentSceneObject
{
    #region InspectorVariables
    // Variables in this region are set in the inspector. See tooltips for more info.

    [Header("Auto Save Area")]

    [SerializeField]
    private Transform respawnPointTransform;    // The player will respawn at the position of this transform if they reload after saving here

    [SerializeField]
    private bool disableWhenUsed = true;        //Whether the trigger should be permenantly disabled after being used once

    #endregion

    private bool        colliderDisabled;       // Whether the collider is currently disabled
    private BoxCollider boxCollider;            // The collider for player detection

    private void Awake()
    {
        // Get the box collider that detects the player and update colliderDisabled
        //   to reflect if the collider is disabled by default
        boxCollider = GetComponent<BoxCollider>();
        colliderDisabled = !boxCollider.enabled;
    }

    private void Start()
    {
        // Subscribe to save/load events so colliderDisabled is saved/loaded with the game
        SaveLoadManager.Instance.SubscribeSceneSaveLoadEvents(OnSceneSave, OnSceneLoadSetup, OnSceneLoadConfigure);
    }

    private void OnDestroy()
    {
        // Unsubscribe from save/load events if the GameObject is destroyed to prevent null reference errors
        SaveLoadManager.Instance.UnsubscribeSceneSaveLoadEvents(OnSceneSave, OnSceneLoadSetup, OnSceneLoadConfigure);
    }

    public void OnSceneSave(SaveData saveData)
    {
        Debug.Log("Saving data for AutoSaveArea: " + GetSavePointId());

        // Save whether the collider is disabled
        saveData.AddData("saveColliderDisabled_" + GetSavePointId(), colliderDisabled);
    }

    public void OnSceneLoadSetup(SaveData saveData)
    {
        Debug.Log("Loading data for AutoSaveArea: " + GetSavePointId());

        // Load whether the collider was disabled, and enable/disable it based on this

        bool disableOnLoad = saveData.GetData<bool>("saveColliderDisabled_" + GetSavePointId());

        if (disableOnLoad)
        {
            DisableCollider();
        }
        else
        {
            EnableCollider();
        }
    }

    public void OnSceneLoadConfigure(SaveData saveData) { } // Nothing to configure

    private void OnTriggerEnter(Collider other)
    {
        // Ignore player interactions if the game is still being loaded
        if (!SaveLoadManager.Instance.LoadingSceneData)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                // The player entered the save area

                if (disableWhenUsed)
                {
                    // The collider is set to be disabled on use, disable it
                    DisableCollider();
                }

                Debug.Log("Attempting to save game at save auto save point: " + GetSavePointId());

                // This is now the last save point that was used
                SetAsUsed();

                // Try to save the game
                bool saveSuccess = SaveLoadManager.Instance.SaveGameData();

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
        // Returns a unique id based on the save area's position, used for saving
        return "autoSaveArea_" + transform.position.x + "_" + transform.position.y + "_" + transform.position.z;
    }

    public Vector3 GetRespawnPosition()
    {
        // Returns the position to spawn the player at if they last saved at this area
        return respawnPointTransform.position;
    }

    public void SetAsUsed()
    {
        WorldSave.Instance.UsedSceneSavePointId = GetSavePointId();

        // Set this area to be the last save point that was used
        SaveLoadManager.SetLastUsedSavePoint(this);
    }

    public void SetAsUnused()
    {
    }
}
