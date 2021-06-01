using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    [SerializeField] private List<ContainerSlotUI>  slotsUI;                // All slots that make up the inventory
    [SerializeField] private ItemContainer          itemContainer;          // ItemContainer that handles adding/removing/storing items

    [SerializeField] private LayoutElement          customiseLayoutElement; // Layout element attached to the customisation UI panel that is a child of the main inventory panel
    [SerializeField] private CanvasGroup            customiseCanvasGroup;   // Canvas group attached to the panel described above
    [SerializeField] private CraftingPanel          craftingPanel;          // The panel that allows the player to craft items, also a child of the main inventory panel

    [SerializeField] private TextMeshProUGUI        weightText;             //  Text displaying how full the inventory is
    [SerializeField] private Slider                 weightSlider;           //  Slider that shows how close the inventory is to holding its max weight
    [SerializeField] private Image                  sliderFillImage;        //  Image used on the slider to show how full the inventory is
    [SerializeField] private Color                  sliderStandardColour;   //  Default colour of the slider image
    [SerializeField] private Color                  sliderFullColour;       //  Colour of the slider image when the inventory is full

    [SerializeField] private float                  maxWeight;              //  Maximum amount of weight this inventory can hold

    #endregion

    #region Properties

    public ItemContainer    ItemContainer   { get { return itemContainer; } }

    #endregion

    private PlayerMovement  playerMovement;      // Reference to the PlayerMovement script attached to the player character
    private float           totalWeight = 0.0f;  // The weight of all items in the inventory combined
    private HandSlotUI      handSlotUI;          // The slot that allows the player to hold/move items

    protected override void Awake()
    {
        base.Awake();

        // Subscribe to the ContainerStateChangedEvent event to update inventory weight when an item is added/removed/moved
        itemContainer.ContainerStateChangedEvent += UpdateTotalInventoryWeight;
    }

    protected override void Start()
    {
        base.Start();

        playerMovement  = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        handSlotUI      = GameObject.FindGameObjectWithTag("HandSlot").GetComponent<HandSlotUI>();

        // Link all of the UI slot elements to the slot objects in the item container
        itemContainer.LinkSlotsToUI(slotsUI);

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
            itemContainer.ItemInfoPopup.SetCanShow(false);
        }
        else
        {
            //No items are in the player's hand - allow the ItemInfoPopup to show info about each item in the inventory
            itemContainer.ItemInfoPopup.SetCanShow(true);
        }
    }

    public void AddItemToInventory(Item item)
    {
        // Adds an item to the item container
        itemContainer.TryAddItemToContainer(item);
    }

    public void AddItemToInventory(string itemId)
    {
        // Same as above function, but takes a string id instead of item object
        itemContainer.TryAddItemToContainer(ItemManager.Instance.GetItemWithId(itemId));
    }

    public bool RemoveItemFromInventory(Item item)
    {
        // Removes an item to the item container
        return RemoveItemFromInventory(item.Id);
    }

    public bool RemoveItemFromInventory(string itemId)
    {
        // Same as above function, but takes a string id instead of item object
        return itemContainer.TryRemoveItemFromContainer(itemId);
    }

    public bool ContainsQuantityOfItem(ItemGroup itemGroup)
    {
        // Checks if the inventory's container has a certain quantity of an item
        return itemContainer.ContainsQuantityOfItem(itemGroup, out _);
    }

    private void CheckForShowHideInput()
    {
        // Block keyboard input if an input field is selected
        if (!InputFieldSelection.AnyFieldSelected)
        {
            if (!showing && Input.GetKeyDown(KeyCode.I) && playerMovement.GetCanMove())
            {
                // Show the inventory if the player presses I when it's not already showing and they can move (i.e. not in another menu)
                Show(InventoryShowMode.InventoryOnly);

                AudioManager.Instance.PlaySoundEffect2D("buttonClickMain1");
            }
            else if (showing && (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.Escape)))
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

        //Allow the player to move and lock their cursor to screen centre
        playerMovement.StartMoving();

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void UpdateTotalInventoryWeight()
    {
        float weight = 0.0f;

        // Add the weight of each stack of items in the inventory
        for (int i = 0; i < itemContainer.Slots.Length; i++)
        {
            weight += itemContainer.Slots[i].ItemStack.StackWeight;
        }

        // Set the total weight to the calculated weight
        totalWeight = weight;

        // Weight as a fraction of the maximum allowed weight
        float weightVal = totalWeight / maxWeight;

        // Set the weight slider to the fractional value, clamping since total weight may be higher than max weight
        weightSlider.value = Mathf.Clamp(weightVal, 0.0f, 1.0f);

        // Display the weight as a percentage of the max weight
        weightText.text = "Weight Limit (" + Mathf.FloorToInt(weightVal * 100.0f) + "%)";

        if(totalWeight >= maxWeight)
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