using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

// ||=======================================================================||
// || InventoryPanel: The panel that displays the contents of the           ||
// ||   player's inventory, also displays inventory weight and handles      ||
// ||   showing/hiding crafting and customisation UI.                       ||
// ||=======================================================================||
// || Used on prefab: Joe/UI/Inventory/InventoryPanel                       ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

// EDITED FOR MODULE 11:
// - Combined hotbar and inventory slotsUI lists to both be linked with a single itemContainer
// - Added item stack dropping

// InventoryShowMode defines different ways that the inventory panel can be displayed
public enum InventoryShowMode
{
    InventoryOnly,  // Just show the player's inventory slots
    Customise,      // Show the inventory and customisation UI
    Craft           // Show the inventory and crafting UI
}

public class InventoryPanel : UIPanel
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private List<ContainerSlotUI>  slotsUI;                // Slots that make up the inventory
    [SerializeField] private List<ContainerSlotUI>  abilitySlotsUI;         // Slots used to store ability items
    [SerializeField] private ItemContainer          mainContainer;          // ItemContainer that handles adding/removing/storing items in the hotbar and inventory
    [SerializeField] private ItemContainer          abilitiesContainer;     // ItemContainer that handles adding/removing/storing ability unlock items

    [SerializeField] private LayoutElement          customiseLayoutElement; // Layout element attached to the customisation UI panel that is a child of the main inventory panel
    [SerializeField] private CanvasGroup            customiseCanvasGroup;   // Canvas group attached to the panel described above
    [SerializeField] private CraftingPanel          craftingPanel;          // The panel that allows the player to craft items, also a child of the main inventory panel

    [SerializeField] private TextMeshProUGUI        weightText;             //  Text displaying how full the inventory is
    [SerializeField] private Slider                 weightSlider;           //  Slider that shows how close the inventory is to holding its max weight
    [SerializeField] private Image                  sliderFillImage;        //  Image used on the slider to show how full the inventory is
    [SerializeField] private Color                  sliderStandardColour;   //  Default colour of the slider image
    [SerializeField] private Color                  sliderFullColour;       //  Colour of the slider image when the inventory is full

    [SerializeField] private float                  maxWeight;              //  Maximum amount of weight this inventory can hold

    [SerializeField] private GameObject             itemStackPickupPrefab;

    #endregion

    #region Properties

    public ItemContainer     MainContainer       { get { return mainContainer; } }
    public ItemContainer     AbilitiesContainer  { get { return abilitiesContainer; } }

    private ContainerSlot    LaunchAbilitySlot   { get { return abilitySlotsUI[0].Slot; } }
    private ContainerSlot    FreezeAbilitySlot   { get { return abilitySlotsUI[1].Slot; } }
    private ContainerSlot    SlamAbilitySlot     { get { return abilitySlotsUI[2].Slot; } }
    private ContainerSlot    GrabAbilitySlot     { get { return abilitySlotsUI[3].Slot; } }

    #endregion

    private PlayerMovement  playerMovement;      // Reference to the PlayerMovement script attached to the player character
    private float           totalWeight = 0.0f;  // The weight of all items in the inventory combined
    private HandSlotUI      handSlotUI;          // The slot that allows the player to hold/move items
    private HotbarPanel     hotbarPanel;         // The script on the hotbar

    protected override void Awake()
    {
        base.Awake();

        playerMovement  = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        handSlotUI      = GameObject.FindGameObjectWithTag("HandSlot").GetComponent<HandSlotUI>();
        hotbarPanel     = GameObject.FindGameObjectWithTag("Hotbar").GetComponent<HotbarPanel>();

        // Get a combined list of ContainerSlotUI for the hotbar and main inventory space
        List<ContainerSlotUI> allSlotsUI = hotbarPanel.SlotsUI.Concat(slotsUI).ToList();

        // Link all of the UI slot elements to the slot objects in the main (inventory/hotbar) item container
        //   The item container is shared between the hotbar and main inventory space
        mainContainer.LinkSlotsToUI(allSlotsUI);

        // Link UI slot elements that hold ability unlock items to the slot objects in the abilities item container
        abilitiesContainer.LinkSlotsToUI(abilitySlotsUI);
    }

    protected override void Start()
    {
        base.Start();

        // (No longer used) Subscribe to the ContainerStateChangedEvent event to update inventory weight when an item is added/removed/moved
        //itemContainer.ContainerStateChangedEvent += UpdateTotalInventoryWeight;

        // (No longer used) Update the inventory weight values/UI on start to show the inventory is empty
        //UpdateTotalInventoryWeight();

        //Hide the UI panel by default
        Hide();
    }

    private void Update()
    {
        //Check if the player pressed a key that should cause the panel to be shown/hidden
        CheckForShowHideInput();

        if (handSlotUI.Slot.ItemStack.StackSize > 0)
        {
            //When there are items in the hand slot, lerp its position to the mouse pointer
            handSlotUI.transform.position = Vector3.Lerp(handSlotUI.transform.position, Input.mousePosition, Time.unscaledDeltaTime * 20.0f);

            //Don't allow the item info popup to be shown when items are in the player's hand
            mainContainer.ItemInfoPopup.SetCanShowItemInfo(false);
        }
        else
        {
            //No items are in the player's hand - allow the ItemInfoPopup to show info about each item in the inventory
            mainContainer.ItemInfoPopup.SetCanShowItemInfo(true);
        }
    }

    public bool AddOrDropItem(Item item, bool showDropNotification, bool allowInstantPickup)
    {
        bool addedItem = TryAddItem(item);

        if(!addedItem)
        {
            DropItemGroup(new ItemGroup(item, 1), showDropNotification, allowInstantPickup);
        }

        return addedItem;
    }

    public bool TryAddItem(Item item)
    {
        if(!string.IsNullOrEmpty(item.SpecialSlotId))
        {
            switch (item.SpecialSlotId)
            {
                case "launchAbility":
                    LaunchAbilitySlot.ReplaceItemInSlot(item);
                    return true;

                case "freezeAbility":
                    FreezeAbilitySlot.ReplaceItemInSlot(item);
                    return true;

                case "slamAbility":
                    SlamAbilitySlot.ReplaceItemInSlot(item);
                    return true;

                case "grabAbility":
                    GrabAbilitySlot.ReplaceItemInSlot(item);
                    return true;
            }

            Debug.LogError("Trying to add item to inventory with unknown SpecialSlotId: " + item.UIName + ", " + item.SpecialSlotId);
            return false;
        }
        else
        {
            // For standard items: Attempts to add an item to the item container if there is enough space
            return mainContainer.TryAddItemToContainer(item);
        }
    }

    public bool TryRemoveItem(Item item)
    {
        // Attempts to remove an item to the item container if one can be found
        return mainContainer.TryRemoveItemFromContainer(item.Id);
    }

    public bool ContainsQuantityOfItem(ItemGroup itemGroup, out List<ContainerSlot> containingSlots)
    {
        // Checks if the inventory/hotbar container has a certain quantity of an item
        return mainContainer.ContainsQuantityOfItem(itemGroup, out containingSlots);
    }

    public int CheckForQuantityOfItem(Item item)
    {
        // Returns the number of the given item in the inventory/hotbar item container
        return mainContainer.CheckForQuantityOfItem(item); 
    }

    public Item GetPlayerAbilityItem(PlayerAbilityType abilityType)
    {
        ContainerSlot abilitySlot = LaunchAbilitySlot;

        switch (abilityType)
        {
            case PlayerAbilityType.Freeze:
                abilitySlot = FreezeAbilitySlot;
                break;
            case PlayerAbilityType.Slam:
                abilitySlot = SlamAbilitySlot;
                break;
            case PlayerAbilityType.Grab:
                abilitySlot = GrabAbilitySlot;
                break;
        }

        // Check if the slot used for storint launch ability items contains an item
        if (abilitySlot.ItemStack.StackSize > 0)
        {
            // The player has obtained a launch ability item, return it
            return ItemManager.Instance.GetItemWithId(abilitySlot.ItemStack.StackItemsID);
        }

        // The player has not obtained a launch ability item
        return null;
    }

    public void DropItemGroup(ItemGroup groupToDrop, bool showDropNotification, bool allowInstantPickup)
    {
        if(groupToDrop.Quantity > 0)
        {
            Debug.Log("Dropping " + groupToDrop.Quantity + " " + groupToDrop.Item.UIName);

            const float dropPosOffset = 0.4f;

            ItemStackPickup stackPickup = Instantiate(itemStackPickupPrefab, playerMovement.transform.position
                                                        - new Vector3(Random.Range(-dropPosOffset, dropPosOffset), 1.25f, Random.Range(-dropPosOffset, dropPosOffset)),
                                                        Quaternion.identity).GetComponent<ItemStackPickup>();

            stackPickup.Setup(groupToDrop, this, allowInstantPickup);

            if(showDropNotification)
            {
                NotificationManager.Instance.AddNotificationToQueue(NotificationMessageType.NoSpaceItemsDropped);
            }
        }
    }

    public void DropItemGroups(List<ItemGroup> groupsToDrop, bool showDropNotification, bool allowInstantPickup)
    {
        for (int i = 0; i < groupsToDrop.Count; i++)
        {
            DropItemGroup(groupsToDrop[i], showDropNotification, allowInstantPickup);
        }
    }

    public void DropItemsInHand(bool allowInstantPickup)
    {
        // Get the number of items in the player's hand
        int handStackSize = handSlotUI.Slot.ItemStack.StackSize;

        // The hand stack contains at least one item, remove all items from it
        for (int i = 0; i < handStackSize; i++)
        {
            handSlotUI.Slot.ItemStack.TryRemoveItemFromStack();
        }

        // Update hand slot UI to show the player they are no longer holding items
        handSlotUI.UpdateUI();

        Item itemTypeToDrop = ItemManager.Instance.GetItemWithId(handSlotUI.Slot.ItemStack.StackItemsID);

        // Drop items
        DropItemGroup(new ItemGroup(itemTypeToDrop, handStackSize), false, allowInstantPickup);

        AudioManager.Instance.PlaySoundEffect2D("throw");
    }

    private void CheckForShowHideInput()
    {
        // Block keyboard input if an input field is selected
        if (!InputFieldSelection.AnyFieldSelected)
        {
            if (!showing && (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.Tab)) && CanShowUIPanel())
            {
                // Show the inventory if the player presses I when it's not already showing and they can move (i.e. not in another menu)
                Show(InventoryShowMode.InventoryOnly);

                AudioManager.Instance.PlaySoundEffect2D("buttonClickMain1");
            }
            else if (showing && (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Escape)))
            {
                // Hide the inventory if the player presses I/Esc when it's showing
                Hide();

                AudioManager.Instance.PlaySoundEffect2D("buttonClickMain2");
            }
        }
    }

    public void Show(InventoryShowMode showMode, float yOffset = 30.0f)
    {
        // Set the panel's position to the centre of the screen, plus the given y offset
        GetComponent<RectTransform>().anchoredPosition = new Vector2(0.0f, yOffset);

        // The customise panel should ignore the UI layout unless in InventoryShowMode.Customise so it doesn't take up space when it's not visible
        customiseLayoutElement.ignoreLayout = (showMode != InventoryShowMode.Customise);

        // Show/hide crafting and customisation panels depending on showMode
        if (showMode == InventoryShowMode.InventoryOnly)
        {
            //Hide customise/crafting

            customiseCanvasGroup.alpha = 0.0f;
            craftingPanel.Hide();
        }
        else if(showMode == InventoryShowMode.Craft)
        {
            //Show crafting menu

            customiseCanvasGroup.alpha = 0.0f;
            craftingPanel.Show();
        }
        else //Customise
        {
            //Show customise menu

            customiseCanvasGroup.alpha = 1.0f;
            craftingPanel.Hide();
        }

        // Also show the inventory panel itself, which contains all other panels
        base.Show();

        //Stop the player from moving and unlock/show the cursor so they can interact with the inventory
        playerMovement.StopMoving();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public override void Show()
    {
        // The default UIPanel Show() function should not be used for InventoryPanel as a show mode (and optional y-offset) is required, see above function

        Debug.LogError("Should not use default Show function for InventoryPanel - use overload that takes InventoryShowMode and y-offset instead");
    }

    public override void Hide()
    {
        base.Hide();

        if(!handSlotUI.Slot.IsEmpty())
        {
            // The player is holding some items while closing the inventory panel - drop items
            DropItemsInHand(true);
        }

        //Allow the player to move and lock their cursor to screen centre
        playerMovement.StartMoving();

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void UpdateTotalInventoryWeight()
    {
        // NOTE: The inventory weight functionality was a planned feature that has since been scrapped,
        //   hence why this function is no longer called. It has been left here to show what progress was made.

        float weight = 0.0f;

        // Add the weight of each stack of items in the inventory
        for (int i = 0; i < mainContainer.Slots.Length; i++)
        {
            weight += mainContainer.Slots[i].ItemStack.StackWeight;
        }

        // Set the total weight to the calculated weight
        totalWeight = weight;

        // Weight as a fraction of the maximum allowed weight
        float weightVal = totalWeight / maxWeight;

        // Set the weight slider to the fractional value, clamping since total weight may be higher than max weight
        weightSlider.value = Mathf.Clamp(weightVal, 0.0f, 1.0f);

        // Display the weight as a percentage of the max weight
        weightText.text = "Weight Limit (" + Mathf.FloorToInt(weightVal * 100.0f) + "%)";

        if (totalWeight >= maxWeight)
        {
            // The maximum weight was reached - no gameplay impact for now, this will come later

            // Set the slider to a different colour to indicate the weight is an issue
            sliderFillImage.color = sliderFullColour;
            Debug.LogWarning("MAX INVENTORY WEIGHT REACHED!");
        }
        else
        {
            // Inventory is not at max weight, use standard slider colour
            sliderFillImage.color = sliderStandardColour;
        }
    }
}