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

    public override void Interact()
    {
        base.Interact();

        InventoryPanel inventoryPanel = GameSceneUI.Instance.PlayerInventory;

        if (!inventoryPanel.Showing)
        {
            // Show the inventory panel on interaction
            inventoryPanel.Show(inventoryShowMode);
        }
    }
}
