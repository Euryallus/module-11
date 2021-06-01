using UnityEngine;

// ||=======================================================================||
// || ManualSavePoint: Allows the player to save the game and set their     ||
// ||   respawn point by interacting with an object.                        ||
// ||=======================================================================||
// || Used on prefab: Joe/Environment/SavePoint                             ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public class ManualSavePoint : InteractableWithOutline, ISavePoint
{
    #region InspectorVariables
    // Variables in this region are set in the inspector. See tooltips for more info.

    [Header("Important: Set unique id")]
    [Header("Save Point Properties")]

    [SerializeField] [Tooltip("Unique id for this save point. Important: all save points should use a different id.")]
    private string id;

    [SerializeField] private Transform spawnPlatformTransform;  // Transform used to position the player on respawn at this point

    #endregion

    public string Id { get { return id; } }

    protected override void Start()
    {
        base.Start();

        if (string.IsNullOrEmpty(id))
        {
            // Warning if an id was not set
            Debug.LogWarning("IMPORTANT: ManualSavePoint exists without id. All save points require a *unique* id for saving/loading data. Click this message to view the problematic GameObject.", gameObject);
        }
    }

    public override void Interact()
    {
        base.Interact();

        Debug.Log("Attempting to save game at point: " + id);

        // Store the UsedSavePointId so the player can be restored to the save point when the game is next loaded
        WorldSave.Instance.UsedSavePointId = id;

        // Try to save the game
        bool saveSuccess = SaveLoadManager.Instance.SaveGame();

        // Show a notification to tell the player is the save was successful
        if (saveSuccess)
        {
            NotificationManager.Instance.AddNotificationToQueue(NotificationMessageType.SaveSuccess);
        }
        else
        {
            NotificationManager.Instance.AddNotificationToQueue(NotificationMessageType.SaveError);
        }
    }

    public string GetSavePointId()
    {
        return id;
    }

    public Vector3 GetRespawnPosition()
    {
        return spawnPlatformTransform.position;
    }
}