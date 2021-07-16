using System.Collections;
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

    [SerializeField] private Transform      spawnPlatformTransform;     // Transform used to position the player on respawn at this point
    [SerializeField] private SoundClass     saveSound;                  // The sound played when the manual save point is used
    [SerializeField] private MeshRenderer   glowSphereRenderer;         // Renderer for the glowing sphere that shows the player if this save point was the last one used
    [SerializeField] private Material       savePointUsedMaterial;      // Material to use on the glow sphere when this save point is currently used
    [SerializeField] private Material       savePointUnusedMaterial;    // Material to use on the glow sphere when this save point is currently unused

    #endregion


    private const float SaveCooldownTime = 5.0f;    // Amount of time to wait after using this save point before it can be used again
    
    public override void Interact()
    {
        base.Interact();

        Debug.Log("Attempting to save game at point: " + GetSavePointId());

        SetAsUsed();

        // Try to save the game
        bool saveSuccess = SaveLoadManager.Instance.SaveGameData();

        // Show a notification to tell the player is the save was successful
        if (saveSuccess)
        {
            AudioManager.Instance.PlaySoundEffect2D(saveSound);

            NotificationManager.Instance.AddNotificationToQueue(NotificationMessageType.SaveSuccess);

            // Disable saving for SaveCooldownTime
            canInteract = false;
            StartCoroutine(AllowSaveAfterCooldown());
        }
        else
        {
            NotificationManager.Instance.AddNotificationToQueue(NotificationMessageType.SaveError);
        }
    }

    private IEnumerator AllowSaveAfterCooldown()
    {
        yield return new WaitForSeconds(SaveCooldownTime);

        canInteract = true;
    }

    public string GetSavePointId()
    {
        return "manualSavePoint_" + transform.position.x + "_" + transform.position.y + "_" + transform.position.z;
    }

    public Vector3 GetRespawnPosition()
    {
        return spawnPlatformTransform.position;
    }

    public void SetAsUsed()
    {
        WorldSave.Instance.UsedSceneSavePointId = GetSavePointId();

        SaveLoadManager.SetLastUsedSavePoint(this);

        glowSphereRenderer.material = savePointUsedMaterial;
    }

    public void SetAsUnused()
    {
        glowSphereRenderer.material = savePointUnusedMaterial;
    }
}