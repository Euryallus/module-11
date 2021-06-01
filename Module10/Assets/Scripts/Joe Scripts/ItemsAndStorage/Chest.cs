using UnityEngine;

// ||=======================================================================||
// || Chest: Stores items that the player puts into it and displays them in ||
// ||   a UI menu similar to the inventory.                                 ||
// ||=======================================================================||
// || Used on prefabs: Joe/Environment/Crafting & Chests/Chest              ||
// ||                  Joe/Environment/Crafting & Chests/Linked Chest       ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public class Chest : InteractableWithOutline
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [Header("Chest Properties")]
    [SerializeField] protected  ItemContainer  itemContainer;  // The ItemContainer that handles adding/removing/storing items

    #endregion

    protected ChestPanel      chestPanel;      // The UI panel that displays the contents of the chest
    protected InventoryPanel  inventoryPanel;  // Reference to the player's inventory panel which is shown when a chest is open

    private void Awake()
    {
        //Find the nventory panel and chest panel in the scene
        inventoryPanel  = GameObject.FindGameObjectWithTag("Inventory").GetComponent<InventoryPanel>();
        chestPanel      = GameObject.FindGameObjectWithTag("ChestPanel").GetComponent<ChestPanel>();
    }

    public override void Interact()
    {
        base.Interact();

        // Link the slot UI elements to the slot objects in the chest's ItemContainer
        itemContainer.LinkSlotsToUI(chestPanel.SlotsUI);

        // Setup the chest to show any items it contains
        SetupChest();

        // Show the chest UI panel that displays items. The panel will be labelled 'Linked Chest' if the chest has the id: linkedChest

        //   Note: linked chests work the same as standard chests, except the itemContainer variable is set
        //   to a shared container rather than one that is specific to the individual chest

        chestPanel.Show(itemContainer.ContainerId == "linkedChest");

        // Also show the inventory panel with an offset to it doesn't overlap with the chest UI
        //   This allows the player to drag items between the chest and their inventory
        inventoryPanel.Show(InventoryShowMode.InventoryOnly, 150.0f);
    }

    protected virtual void SetupChest()
    {
        // Update the chest UI to show its contents
        UpdateChestUI();
    }

    protected void UpdateChestUI()
    {
        // Update the UI of each slot to show any contained items
        for (int i = 0; i < chestPanel.SlotsUI.Count; i++)
        {
            chestPanel.SlotsUI[i].UpdateUI();
        }
    }
}