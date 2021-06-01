using UnityEngine;

// ||=======================================================================||
// || InventoryOpenerInteractable: An InteractableWithOutline that opens    ||
// ||    the player's inventory when interacted with                        ||
// ||=======================================================================||
// || Used on prefabs: Joe/Environment/Crafting & Chests/Crafting Table     ||
// ||                  Joe/Environment/Crafting & Chests/Customising Table  ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public class InventoryOpenerInteractable : InteractableWithOutline
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private InventoryShowMode inventoryShowMode; // The type of inventory UI to show on interaction

    #endregion

    private InventoryPanel inventoryPanel;  // The player's inventory panel

    protected override void Start()
    {
        base.Start();

        // Get a reference to the player's inventory panel
        inventoryPanel = GameObject.FindGameObjectWithTag("Inventory").GetComponent<InventoryPanel>();
    }

    public override void Interact()
    {
        base.Interact();

        if (!inventoryPanel.Showing)
        {
            // Show the inventory panel on interaction
            inventoryPanel.Show(inventoryShowMode);
        }
    }
}
