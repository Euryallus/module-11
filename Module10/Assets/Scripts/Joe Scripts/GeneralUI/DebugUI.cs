using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// ||=======================================================================||
// || DebugUI: Contains useful debug options, for now just allows any       ||
// ||   item to be spawned at any time.                                     ||
// ||=======================================================================||
// || Used on prefab: Joe/UI/DebugUI                                        ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

[RequireComponent(typeof(CanvasGroup))]
public class DebugUI : MonoBehaviour
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private TMP_InputField itemSpawnInputField; // Input field for entering an item id to spawn

    #endregion

    private CanvasGroup canvGroup;
    private bool        showing;

    private void Start()
    {
        canvGroup = GetComponent<CanvasGroup>();

        // Hide debug options by default when not in the unity editor
        #if !UNITY_EDITOR
            SetShowing(false);
        #else
            SetShowing(true);
        #endif
    }

    private void Update()
    {
        // Ctl + Alt + D toggles debug options
        if(!InputFieldSelection.AnyFieldSelected && Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.D))
        {
            SetShowing(!showing);
        }
    }

    //Called when the 'Spawn Item' button is pressed
    public void ButtonSpawnItem()
    {
        // Find an item with the entered id
        Item itemToSpawn = ItemManager.Instance.GetItemWithId(itemSpawnInputField.text);

        if(itemToSpawn != null)
        {
            // The item id was valid, add the item to the player's inventoryS
            GameSceneUI.Instance.PlayerInventory.AddOrDropItem(itemToSpawn, true, true);
        }
    }

    public void ForceSave()
    {
        SaveLoadManager.Instance.SaveGameData();
    }

    public void ForceLoad()
    {
        SaveLoadManager.Instance.LoadGameScene(SceneManager.GetActiveScene().name);
    }

    private void SetShowing(bool show)
    {
        canvGroup.alpha = show ? 1.0f : 0.0f;
        canvGroup.blocksRaycasts = show;
        canvGroup.interactable = show;

        showing = show;
    }
}