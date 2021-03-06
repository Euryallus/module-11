using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// ||=======================================================================||
// || HotbarPanel: Panel allowing the player to select/hold certain items   ||
// ||=======================================================================||
// || Used on prefab: Joe/UI/Hotbar                                         ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

// Changed for Mod11
// - Removed itemcontainer, slots are now merged with inventory container

public class HotbarPanel : UIPanel, IPersistentGlobalObject
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private List<ContainerSlotUI>  slotsUI;            // Slots that make up the hotbar
    [SerializeField] private GameObject             itemEatPanel;       // Panel that appears next to the hotbar when the player eats while their inventory is open
    [SerializeField] private CanvasGroup            parentCanvasGroup;  // Parent canvas group containing the hotbar and stat panels

    #endregion

    #region Properties

    public List<ContainerSlotUI>    SlotsUI { get { return slotsUI; } }

    #endregion

    public event Action<Item, ContainerSlotUI> HeldItemChangedEvent;    // Event that is invoked when the held item is changed

    private int             selectedSlotIndex;      // Index of the selected hotbar slot
    private HandSlotUI      handSlot;               // Player's hand slot for picking up/moving items
    private ItemContainer   inventoryItemContainer;

    protected override void Awake()
    {
        base.Awake();

        handSlot                = GameObject.FindGameObjectWithTag("HandSlot").GetComponent<HandSlotUI>();
        inventoryItemContainer  = GameObject.FindGameObjectWithTag("Inventory").GetComponent<InventoryPanel>().MainContainer;
    }

    protected override void Start()
    {
        base.Start();

        // Subscribe to save/load events so the hotbar's data will be saved/loaded with the game
        SaveLoadManager.Instance.SubscribeGlobalSaveLoadEvents(OnGlobalSave, OnGlobalLoadSetup, OnGlobalLoadConfigure);

        itemEatPanel.SetActive(false);

        // Update slot selection when an item is added/removed from the container in case a different item is being held
        inventoryItemContainer.ContainerStateChangedEvent += UpdateCurrentSlotSelection;

        // Select the first slot by default
        SelectSlot(0);

        // Show the UI panel without adding to the counter that prevents certain player input when
        //   a UI panel is open, since the hotbar should always show and not block anything
        isBlockingPanel = false;

        Show();
        ShowHotbarAndStatPanels();
    }

    private void OnDestroy()
    {
        // Unsubscribe from save/load events is the hotbar is destroyed to prevent a null reference error
        SaveLoadManager.Instance.UnsubscribeGlobalSaveLoadEvents(OnGlobalSave, OnGlobalLoadSetup, OnGlobalLoadConfigure);
    }

    private void Update()
    {
        // Check for keyboard input for switching slots
        CheckForPlayerInput();

        if(handSlot.Slot.ItemStack.StackSize > 0 &&
            ItemManager.Instance.IsItemConsumable(handSlot.Slot.ItemStack.StackItemsID))
        {
            // Show the item eat panel if the hand slot contains a consumable item

            if (!itemEatPanel.activeSelf)
            {
                itemEatPanel.SetActive(true);
            }
        }
        else if(itemEatPanel.activeSelf)
        {
            // No consumable item being held, hide the item eat panel

            itemEatPanel.SetActive(false);
        }
    }

    public void OnGlobalSave(SaveData saveData)
    {
        Debug.Log("Saving hotbar panel data");

        // Save the selected slot so it can be restored on load
        saveData.AddData("selectedSlot", selectedSlotIndex);
    }

    public void OnGlobalLoadSetup(SaveData saveData)
    {
        Debug.Log("Loading hotbar panel data");

        // Select the slot that was selected when the game was last saved
        SelectSlot(saveData.GetData<int>("selectedSlot"));
    }

    public void OnGlobalLoadConfigure(SaveData saveData) { } // Nothing to configure

    public void ShowHotbarAndStatPanels()
    {
        // Show the parent canvas group containing this hotbar and stat panels, and stop UI behind it from being interacted with
        parentCanvasGroup.alpha = 1.0f;
        parentCanvasGroup.blocksRaycasts = true;
    }

    public void HideHotbarAndStatPanels()
    {
        // Hide the parent canvas group containing this hotbar and stat panels, and allow UI behind it to be interacted with
        parentCanvasGroup.alpha = 0.0f;
        parentCanvasGroup.blocksRaycasts = false;
    }

    private void CheckForPlayerInput()
    {
        // Number keys input - only allow input if not typing in an input field
        if (!InputFieldSelection.AnyFieldSelected && !GameSceneUI.Instance.ShowingCinematicsCanvas)
        {
            for (int i = 1; i <= slotsUI.Count; i++)
            {
                // For 1 to the number of slots, check if a corresponding number key is pressed
                if (Input.GetKeyDown(i.ToString()))
                {
                    // Valid number key is pressed, select the corresponding slot
                    //   (minusing 1 since the slot list starts at 0, but the key corresponding to the first slot is 1)
                    SelectSlot(i - 1);
                    break;
                }
            }
        }

        // Check that the player is not using a scrollbar/has their pointer inside a scroll area
        if(!ScrollAreaSelection.AnyScrollAreaSelected)
        {
            // Scroll input
            if (Input.GetAxis("Mouse ScrollWheel") > 0f)
            {
                if(!AnyBlockingPanelShowing("InventoryPanel"))
                {
                    // Scrolling 'up', select the next (right) slot (looping back to slot 0 if the final one is reached)
                    SelectSlot((selectedSlotIndex + 1) % slotsUI.Count);
                }
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
            {
                if(!AnyBlockingPanelShowing("InventoryPanel"))
                {
                    // Scrolling 'down', select the previous (left) slot (looping back to rightmost slot if the first one is reached)
                    SelectSlot(selectedSlotIndex == 0 ? (slotsUI.Count - 1) : (selectedSlotIndex - 1));
                }
            }
        }
    }

    private void UpdateCurrentSlotSelection(bool loadingContainer)
    {
        // Selects the slot at the selected index - takes no parameters to be compatible with the ContainerStateChangedEvent
        SelectSlot(selectedSlotIndex);
    }

    private void SelectSlot(int slotIndex)
    {
        GameObject mainCameraGameObj = GameObject.FindGameObjectWithTag("MainCamera");

        // Only allow slot selection if the main camera isn't null, i.e. player is not talking to an npc/using an alternate camera
        if (mainCameraGameObj != null)
        {
            // Deselect the currently selected slot
            slotsUI[selectedSlotIndex].SetSelected(false);

            // Select the new slot
            selectedSlotIndex = slotIndex;
            slotsUI[selectedSlotIndex].SetSelected(true);

            // The held item was changed, invoke the held item event
            HeldItemChangedEvent?.Invoke(GetSelectedItem(), slotsUI[selectedSlotIndex]);
        }
    }

    public Item GetSelectedItem()
    {
        // Returns the item in the selected slot, or null if there is none

        ContainerSlot selectedSlot = inventoryItemContainer.Slots[selectedSlotIndex];

        if (selectedSlot.ItemStack.StackSize > 0)
        {
            // Selected slot contains an item, return it
            return ItemManager.Instance.GetItemWithId(selectedSlot.ItemStack.StackItemsID);
        }
        else
        {
            // No item in the selected slot
            return null;
        }
    }
}