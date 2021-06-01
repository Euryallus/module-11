using UnityEngine;
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

public class DebugUI : MonoBehaviour
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private TMP_InputField itemSpawnInputField; // Input field for entering an item id to spawn

    #endregion

    //Called when the 'Spawn Item' button is pressed
    public void ButtonSpawnItem()
    {
        // Find an item with the entered id
        Item itemToSpawn = ItemManager.Instance.GetItemWithId(itemSpawnInputField.text);

        if(itemToSpawn != null)
        {
            // The item id was valid, add the item to the player's inventoryS
            InventoryPanel inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<InventoryPanel>();
            inventory.AddItemToInventory(itemToSpawn);
        }
    }
}