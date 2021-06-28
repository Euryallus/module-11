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

    [Header("Manual Save Point")]

    [SerializeField] private Transform spawnPlatformTransform;  // Transform used to position the player on respawn at this point

    #endregion

    public override void Interact()
    {
        base.Interact();

        Debug.Log("Attempting to save game at point: " + GetSavePointId());

        // Store the UsedSavePointId so the player can be restored to the save point when the game is next loaded
        WorldSave.Instance.UsedSavePointId = GetSavePointId();

        // Try to save the game
        bool saveSuccess = SaveLoadManager.Instance.SaveGameData();

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
        return "manualSavePoint_" + transform.position.x + "_" + transform.position.y + "_" + transform.position.z;
    }

    public Vector3 GetRespawnPosition()
    {
        return spawnPlatformTransform.position;
    }
}