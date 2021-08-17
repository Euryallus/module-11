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

    [SerializeField] private TextMeshProUGUI healthText;         // Text for showing current and max health values
    [SerializeField] private TextMeshProUGUI foodLevelText;      // Text for showing current and max food level values

    #endregion

    private CanvasGroup canvGroup;
    private bool        showing;    // Whether debug UI is being shown

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
        if (!InputFieldSelection.AnyFieldSelected && Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.D))
        {
            SetShowing(!showing);
        }

        if (showing)
        {
            if (PlayerInstance.ActivePlayer != null)
            {
                // Update text that displays the player's current health and food level

                PlayerStats playerStats = PlayerInstance.ActivePlayer.PlayerStats;

                healthText.text     = "Health: "     + System.Math.Round(playerStats.Health, 2)    + "/" + playerStats.MaxHealth;
                foodLevelText.text  = "Food Level: " + System.Math.Round(playerStats.FoodLevel, 2) + "/" + playerStats.MaxFoodLevel;
            }
        }
    }

    // Called when the 'Spawn Item' button is pressed
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

    // Called on button press, forces the game to save
    public void ForceSave()
    {
        SaveLoadManager.Instance.SaveGameData();
    }

    // Called on button press, forces the game to reload the active scene
    public void ForceLoad()
    {
        SaveLoadManager.Instance.LoadGameScene(SceneManager.GetActiveScene().name);
    }

    // Shows/hides the debug UI panel
    private void SetShowing(bool show)
    {
        canvGroup.alpha = show ? 1.0f : 0.0f;
        canvGroup.blocksRaycasts = show;
        canvGroup.interactable = show;

        showing = show;
    }
}