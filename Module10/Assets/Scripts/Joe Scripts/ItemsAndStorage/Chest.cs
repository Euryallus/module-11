using UnityEngine;

// ||=======================================================================||
// || Chest: Stores items that the player puts into it and displays them in ||
// ||   a UI menu similar to the inventory.                                 ||
// ||=======================================================================||
// || Used on prefabs: Joe/Environment/Crafting & Chests/Chest              ||
// ||                  Joe/Environment/Crafting & Chests/Linked Chest       ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || originally for the prototype phase.                                   ||
// ||                                                                       ||
// || Changes made during the production phase (Module 11):                 ||
// ||                                                                       ||
// || - Now handles chest open/close animations                             ||
// ||=======================================================================||

public class Chest : InteractableWithOutline
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [Header("Chest Properties")]
    [SerializeField] protected  ItemContainer   itemContainer;  // The ItemContainer that handles adding/removing/storing items

    [SerializeField] private Animator           animator;       // Handles chest open/close animations
    [SerializeField] private SoundClass         openSound;      // Sound played when the chest is opened
    [SerializeField] private SoundClass         closeSound;     // Sound played when the chest is closed

    #endregion

    protected ChestPanel      chestPanel;      // The UI panel that displays the contents of the chest

    private void Awake()
    {
        //Find the chest panel (UI panel that displays chest contents) in the scene
        chestPanel = GameObject.FindGameObjectWithTag("ChestPanel").GetComponent<ChestPanel>();
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
        GameSceneUI.Instance.PlayerInventory.Show(InventoryShowMode.InventoryOnly, 140.0f);

        // Trigger OnChestUIClosed when the chest UI panel is closed
        chestPanel.UIPanelHiddenEvent += OnChestUIClosed;

        // Animate the chest opening
        if (animator != null)
        {
            animator.SetBool("Open", true);
        }

        // Play the open sound
        if(openSound != null)
        {
            AudioManager.Instance.PlaySoundEffect3D(openSound, transform.position);
        }
    }

    private void OnChestUIClosed()
    {
        if(!SaveLoadManager.Instance.LoadingSceneData)
        {
            // Animate the chest closing when chest UI is closed
            if (animator != null)
            {
                animator.SetBool("Open", false);
            }

            // Play the chest closed sound
            if (closeSound != null)
            {
                AudioManager.Instance.PlaySoundEffect3D(closeSound, transform.position);
            }
        }

        // Unsubscribe from the UIPanelHiddenEvent event so this funtion will not be triggered when a different chest is closed
        chestPanel.UIPanelHiddenEvent -= OnChestUIClosed;
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