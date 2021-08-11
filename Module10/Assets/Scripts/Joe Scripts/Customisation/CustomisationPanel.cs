using UnityEngine;
using TMPro;
using System.Collections.Generic;

// ||=======================================================================||
// || CustomisationPanel: UI panel with options to apply customisation to   ||
// ||   items, e.g. change item name                                        ||
// ||=======================================================================||
// || Used on prefab: Joe/UI/Customisation/CustomisationPanel               ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || originally for the prototype phase.                                   ||
// ||                                                                       ||
// || Changes made during the production phase (Module 11):                 ||
// ||                                                                       ||
// || - Added additionalRequiredCurrency (certain customisation options now ||
// ||    increase the amount of currency required).                         ||
// || - UI now allows changes to be previewed even if the player does not   ||
// ||    have the reqired amount of currency.                               ||
// || - Improved how the float value buttons respond to changes (they now   ||
// ||    stop being interactable when a limit is reached.                   ||
// || - Various minor changes/improvements to make the panel nicer to use.  ||
// ||=======================================================================||

public class CustomisationPanel : MonoBehaviour, IPersistentGlobalObject
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private ItemContainer      inventoryItemContainer;     // Reference to the item container for the player's inventory
    [SerializeField] private ContainerSlotUI    customiseSlotUI;            // Slot for the item that will be customised NOTE: SHOULD ONLY EVER ALLOW 1 ITEM
    [SerializeField] private ContainerSlotUI    currencySlotUI;             // Slot for the item(s) used as currency when customising the item in the above slot
    [SerializeField] private ContainerSlotUI    resultSlotUI;               // Slot for the resulting customised item

    [SerializeField] private GameObject         customFloatPropertyPrefab;  // Prefab GameObject for altering custom float properties
    [SerializeField] private GameObject         customStringPropertyPrefab; // Prefab GameObject for altering custom string properties
    [SerializeField] private GameObject         propertyTextPrefab;         // Prefab GameObject for showing the name of a property, used as a heading
    [SerializeField] private GameObject         customisationOptionsPanel;  // Child of the main customisation panel for displaying customisation options
    [SerializeField] private TMP_InputField     customNameInput;            // Input field for renaming an item
    [SerializeField] private TextMeshProUGUI    infoText;                   // Text displaying info about how/whether an item can be customised

    #endregion

    private ItemManager             itemManager;       // Reference to the ItemManager
    private ContainerSlot           customiseSlot;     // The slot for the item to be customised
    private ContainerSlot           currencySlot;      // The slot for the currency required to customise the item in the slot above
    private ContainerSlot           resultSlot;        // The resulting item which is based on the original item with customisation options applied
    private List<TMP_InputField>    customInputFields; // List of all input fields used for altering custom string properties

    private int additionalRequiredCurrency; // The amount of extra currency that is required on top of CurrencyItemQuantity
                                            //   depending on what item properties are being customised/upgraded

    private void Awake()
    {
        // Setup the 3 slots used for customisation. The customise slot takes a maximum of 1 item
        //   to be customised at any time, the other slots can take any number of items
        customiseSlot   = new ContainerSlot(1, inventoryItemContainer);
        currencySlot    = new ContainerSlot(0, inventoryItemContainer);
        resultSlot      = new ContainerSlot(0, inventoryItemContainer);

        // Link the slot UI elements to the slot objects created above
        customiseSlotUI .LinkToContainerSlot(customiseSlot);
        currencySlotUI  .LinkToContainerSlot(currencySlot);
        resultSlotUI    .LinkToContainerSlot(resultSlot);

        // Subscribe to item moved events for the three slots to UI can
        //   be updated whenever the state of any slot changes
        customiseSlot.ItemsMovedEvent   += OnCustomiseSlotItemsMoved;
        currencySlot .ItemsMovedEvent   += OnCurrencySlotItemsMoved;
        resultSlot   .ItemsMovedEvent   += OnResultSlotItemsMoved;

        // Hide the customisation options since there is no item being customised by default
        customisationOptionsPanel.SetActive(false);
    }

    protected void Start()
    {
        // Subscribe to save/load events so the panel's data will be saved/loaded with the game
        SaveLoadManager.Instance.SubscribeGlobalSaveLoadEvents(OnGlobalSave, OnGlobalLoadSetup, OnGlobalLoadConfigure);

        itemManager = ItemManager.Instance;

        // By default, hide the result slot cover that is used to stop the player taking an item when they don't have enough currency
        resultSlot.SlotUI.SetCoverFillAmount(0.0f);

        ShowDefaultInfoText();
    }

    private void OnDestroy()
    {
        // Unsubscribe from save/load events if for some reason the panel is destroyed to prevent null ref. errors
        SaveLoadManager.Instance.UnsubscribeGlobalSaveLoadEvents(OnGlobalSave, OnGlobalLoadSetup, OnGlobalLoadConfigure);
    }

    public void OnGlobalSave(SaveData saveData)
    {
        // Save the type of item in the customise slot (no need to save stack size as there will always be either 0 [i.e. no item] or 1)
        saveData.AddData("customiseStackItemId", customiseSlot.ItemStack.StackSize > 0 ? customiseSlot.ItemStack.StackItemsID : "");

        // Save the type and number of items in the currency slot
        saveData.AddData("currencyStackSize", currencySlot.ItemStack.StackSize);
        saveData.AddData("currencyStackItemId", currencySlot.ItemStack.StackItemsID);

        // Save the type of item in the result slot (no need to save stack size as there will always be either 0 [i.e. no item] or 1)
        saveData.AddData("resultStackItemId", resultSlot.ItemStack.StackSize > 0 ? resultSlot.ItemStack.StackItemsID : "");
    }

    public void OnGlobalLoadSetup(SaveData saveData) { } // Nothing to setup

    public void OnGlobalLoadConfigure(SaveData saveData)
    {
        // Load any item that was in the customise slot
        string customiseItemId = saveData.GetData<string>("customiseStackItemId");
        if (!string.IsNullOrEmpty(customiseItemId))
        {
            customiseSlot.ItemStack.AddItemToStack(customiseItemId, false);
        }

        // Load any items that were in the currency slot
        int currencyStackSize = saveData.GetData<int>("currencyStackSize");
        string currencyItemId = saveData.GetData<string>("currencyStackItemId");
        for (int i = 0; i < currencyStackSize; i++)
        {
            currencySlot.ItemStack.AddItemToStack(currencyItemId, false);
        }

        // Load any that that was in the result slot
        string resultItemId = saveData.GetData<string>("resultStackItemId");
        if (!string.IsNullOrEmpty(resultItemId))
        {
            resultSlot.ItemStack.AddItemToStack(resultItemId, false);
        }

        // Update all UI based on loaded items
        customiseSlotUI.UpdateUI();
        currencySlotUI.UpdateUI();
        ItemInputChanged();
    }

    private void OnCustomiseSlotItemsMoved()
    {
        // The player added/removed an item from the customise slot, update UI based on the new input
        ItemInputChanged();
    }

    private void OnCurrencySlotItemsMoved()
    {
        // The player added/removed item(s) from the currency slot, update UI based on the new input
        ItemInputChanged();
    }

    private void OnResultSlotItemsMoved()
    {
        // The player took the resulting item
        // Increment the custom item id so the next custom item that is created will have a unique id
        itemManager.IncrementUniqueCustomItemId();

        Item customiseSlotItem = itemManager.GetItemWithId(customiseSlot.ItemStack.StackItemsID);

        // Remove the origina item from the customise slot to prevent item duplication
        customiseSlot.ItemStack.TryRemoveItemFromStack();

        // Remove a certain amount of items from the currency slot based on the customisation cost
        for (int i = 0; i < (customiseSlotItem.CurrencyItemQuantity + additionalRequiredCurrency); i++)
        {
            currencySlot.ItemStack.TryRemoveItemFromStack();
        }

        // Update UI to reflect changes
        customiseSlotUI.UpdateUI();
        currencySlotUI.UpdateUI();
        ItemInputChanged();
    }

    private void ItemInputChanged()
    {
        // Remove any existing items in the result slot
        resultSlot.ItemStack.TryRemoveItemFromStack();

        // Get the unique id that will be used for the custom item being added/removed
        string customItemId = itemManager.GetUniqueCustomItemId();

        CheckForValidItemInputs(out bool validCustomiseItem);

        if (validCustomiseItem)
        {
            // The player has put a valid combination of items into the customise/currency slots, the output item should be shown

            Item baseItem = itemManager.GetItemWithId(customiseSlot.ItemStack.StackItemsID);

            // By default, the new 'result' item will be based on the item in the customise slot. However, if the item in the customise slot is already
            //   a custom item, the resulting item should be based on the original base item (i.e. the non-custom item at the top of the heirarchy)
            string originalBaseItemId = baseItem.Id;
            if (baseItem.CustomItem)
            {
                originalBaseItemId = baseItem.BaseItemId;
            }

            // Add a new custom item - this will be the resulting item type that the player can take
            itemManager.AddCustomItem(customItemId, baseItem.Id, originalBaseItemId);

            // Set the name of the new custom item to that of its parent item by default
            SetCustomisedItemName(itemManager.GetItemWithId(customiseSlot.ItemStack.StackItemsID).UIName);

            // Add the new custom item to the result slot
            resultSlot.ItemStack.AddItemToStack(customItemId, false);

            // Setup the customisation panel UI based on the custom item's properties
            SetupCustomisationPanel(baseItem);
        }
        else
        {
            // The player has put an invalid combination of items into the customise/currency slots

            // Hide the customisation panel as there is now nothing to customise
            customisationOptionsPanel.SetActive(false);

            // Try removing a custom item with the current customItemId, this is done in case a custom item was created
            //   with a valid combination of items without being picked up. Once the setup becomes invalid that custom
            //   item was never used and as such can be removed
            itemManager.RemoveCustomItem(customItemId);

            additionalRequiredCurrency = 0;
        }

        //Update the result slot UI to reflect changes
        resultSlotUI.UpdateUI();
    }

    private void SetupCustomisationPanel(Item baseItem)
    {
        // Clear existing custom upgrade properties UI that may be leftover from another item that was customised
        foreach (Transform t in customisationOptionsPanel.transform)
        {
            if (!t.CompareTag("DoNotDestroy"))
            {
                Destroy(t.gameObject);
            }
        }

        // If this item has custom float properties, add UI elements for each one
        for (int i = 0; i < baseItem.CustomFloatProperties.Length; i++)
        {
            // Get the property of the base item at the current index (the default options for this float property)
            CustomFloatProperty baseProperty = baseItem.CustomFloatProperties[i];

            // Add some text and set it to display the property name
            GameObject propertyText = Instantiate(propertyTextPrefab, customisationOptionsPanel.transform);
            propertyText.GetComponent<TextMeshProUGUI>().text = baseProperty.UIName;

            // Add a panel containing the property value and buttons to increase/decrease that value
            CustomFloatPropertyPanel propertyPanel = Instantiate(customFloatPropertyPrefab, customisationOptionsPanel.transform)
                                                        .GetComponent<CustomFloatPropertyPanel>();

            CustomFloatProperty resultItemProperty = itemManager.GetItemWithId(resultSlot.ItemStack.StackItemsID).CustomFloatProperties[i];

            // By default, the value of the current property to be displayed is the same as the base item's value
            //SetupFloatPropertyValueText(propertyPanel.ValueText, baseProperty.Value, baseProperty.Value);
            SetupFloatPropertyValueText(propertyPanel.ValueText, resultItemProperty.Value, baseProperty.Value);

            // Set the PropertyAddButton and PropertySubtractButton functions to be called when the corresponding buttons are clicked
            propertyPanel.AddButton.Button.onClick.AddListener(delegate { FloatPropertyAddButton(baseProperty.Name, propertyPanel); });
            propertyPanel.SubtractButton.Button.onClick.AddListener(delegate { FloatPropertySubtractButton(baseProperty.Name, propertyPanel); });

            propertyPanel.AddButton.Setup();
            propertyPanel.SubtractButton.Setup();

            UpdateFloatPropertyButtons(propertyPanel, baseProperty.Value, baseProperty.Value, baseProperty.MaxValue);
        }

        customInputFields = new List<TMP_InputField>();

        // If this item has custom string properties, add UI elements for each one
        for (int i = 0; i < baseItem.CustomStringProperties.Length; i++)
        {
            // Get the property of the base item at the current index (the default options for this string property)
            CustomStringProperty baseProperty = baseItem.CustomStringProperties[i];

            //Add some text and set it to display the property name
            GameObject propertyText = Instantiate(propertyTextPrefab, customisationOptionsPanel.transform);
            propertyText.GetComponent<TextMeshProUGUI>().text = baseProperty.UIName;

            // Add a panel containing an input field to edit the custom string value
            GameObject propertyPanel = Instantiate(customStringPropertyPrefab, customisationOptionsPanel.transform);

            // Get the input field from the panel created above
            TMP_InputField field = propertyPanel.GetComponent<TMP_InputField>();

            // Add the input field to the list of custom input fields so its contents can later be checked if needed
            customInputFields.Add(field);

            // When the player is done editing the field, the StringPropertyValueChanged
            //   function will be called to apply changes to the customised item
            int index = i;
            field.onEndEdit.AddListener(delegate { StringPropertyValueChanged(baseProperty.Name, index); });
        }

        // Show the customisation panel so the player can start making changes
        customisationOptionsPanel.SetActive(true);
    }

    private void StringPropertyValueChanged(string propertyName, int customFieldIndex)
    {
        // Get the text the player entered from the input field at the given index
        string newText = customInputFields[customFieldIndex].text;

        if (!string.IsNullOrWhiteSpace(newText))
        {
            // The player entered valid text

            // Set the custom string property on the item being customised to have the new value
            itemManager.SetCustomStringItemData(itemManager.GetUniqueCustomItemId(), propertyName, newText);
        }
    }

    private void FloatPropertyAddButton(string propertyName, CustomFloatPropertyPanel propertyPanel)
    {
        Item customResultItem = itemManager.GetCustomItemWithId(itemManager.GetUniqueCustomItemId());

        // Get the current values for the float property being edited
        CustomFloatProperty property = customResultItem.GetCustomFloatPropertyWithName(propertyName);

        // Add the upgrade increase to the current property value to get the new added value
        float addedValue = property.Value + property.UpgradeIncrease;

        // Adding 0.01 to prevent errors caused by float inaccuracies when comparing the two values
        if (FloatHelper.FloatIsLessThanOrEqualTo(addedValue, property.MaxValue))
        {
            // The added value is within the allowed value range

            UpdateFloatPropertyButtons(propertyPanel, addedValue, property.Value, property.MaxValue);

            // Set the custom float property on the item being customised to have the new value
            itemManager.SetCustomFloatItemData(itemManager.GetUniqueCustomItemId(), propertyName, addedValue);

            // Setup the UI text that shows the value of the property, and changes the text colour depending on if it matches that of the original item being customised
            SetupFloatPropertyValueText(propertyPanel.ValueText, addedValue, itemManager.GetItemWithId(customiseSlot.ItemStack.StackItemsID).GetCustomFloatPropertyWithName(propertyName).Value);

            // Add to the additional required currency count based on the property's currency increase value
            additionalRequiredCurrency += property.CurrencyIncrease;

            // Check for valid item inputs, since the required currency was potentially increased
            //   and the player may no longer have the necessary amount to customise their item
            CheckForValidItemInputs(out _);

            AudioManager.Instance.PlaySoundEffect2D("buttonClickSmall", true);
        }
    }

    private void FloatPropertySubtractButton(string propertyName, CustomFloatPropertyPanel propertyPanel)
    {
        Item customiseItem    = itemManager.GetItemWithId(customiseSlot.ItemStack.StackItemsID);
        Item customResultItem = itemManager.GetCustomItemWithId(itemManager.GetUniqueCustomItemId());

        // Get the current values for the float property being edited
        CustomFloatProperty property = customiseItem.GetCustomFloatPropertyWithName(propertyName);
        CustomFloatProperty customisedProperty = customResultItem.GetCustomFloatPropertyWithName(propertyName);

        // Subtract the upgrade increase (in this case decrease) from the current property value to get the new added value
        float subtractedValue = customisedProperty.Value - customisedProperty.UpgradeIncrease;

        // Subtracting 0.01 to prevent errors caused by float inaccuracies when comparing the two values
        if (FloatHelper.FloatIsGreaterThanOrEqualTo(subtractedValue, property.Value))
        {
            // The subtracted value is within the allowed value range

            UpdateFloatPropertyButtons(propertyPanel, subtractedValue, property.Value, property.MaxValue);

            // Set the custom float property on the item being customised to have the new value
            itemManager.SetCustomFloatItemData(itemManager.GetUniqueCustomItemId(), propertyName, subtractedValue);

            // Setup the UI text that shows the value of the property, and changes the text colour depending on if it matches that of the original item being customised
            SetupFloatPropertyValueText(propertyPanel.ValueText, subtractedValue, itemManager.GetItemWithId(customiseSlot.ItemStack.StackItemsID).GetCustomFloatPropertyWithName(propertyName).Value);

            // Reduce the additional required currency count by the property's currency increase value
            additionalRequiredCurrency -= customisedProperty.CurrencyIncrease;

            // Check for valid item inputs, since the required currency was potentially decreased
            //   and the player may be able to customise their item when they previously could not
            CheckForValidItemInputs(out _);

            AudioManager.Instance.PlaySoundEffect2D("buttonClickSmall", true);
        }
    }

    private void UpdateFloatPropertyButtons(CustomFloatPropertyPanel propertyPanel, float propertyValue, float minValue, float maxValue)
    {
        // Only allow the subtract button to be interacted with if the current property value has not reached the minimum
        propertyPanel.SubtractButton.SetInteractable(!FloatHelper.FloatsAreEqual(propertyValue, minValue));

        // Only allow the add button to be interacted with if the current property value has not reached the maximum
        propertyPanel.AddButton.SetInteractable(!FloatHelper.FloatsAreEqual(propertyValue, maxValue));
    }

    private void SetupFloatPropertyValueText(TextMeshProUGUI valueText, float value, float baseValue)
    {
        // Set the UI text to show the value
        valueText.text = value.ToString();

        if (FloatHelper.FloatsAreEqual(value, baseValue))
        {
            // The value of the customised item is the same as the original item, show the value in white
            valueText.color = Color.white;
        }
        else
        {
            // The value of the customised item is different to the original item,
            //   show the value in cyan so the player knows they have made a change
            valueText.color = Color.cyan;
        }
    }

    private void CheckForValidItemInputs(out bool validCustomiseItem)
    {
        //This function checks if the items in the customise and currency slots create a valid setup to produce a resulting item

        bool resultSlotEnabled = false;

        if (customiseSlot.ItemStack.StackSize > 0)
        {
            // The player has put an item in the customise slot

            Item itemToCustomise = itemManager.GetItemWithId(customiseSlot.ItemStack.StackItemsID);

            if (itemToCustomise.Customisable)
            {
                // The item in the customise slot is allowed to be customised

                Item currencyItem = null;           // The item required to customise the item in the customise slot, null by default in case no currency is needed
                int requiredCurrencyQuantity = 0;   // The quantity of the above item required

                if(itemToCustomise.CurrencyItemQuantity > 0)
                {
                    // The item being customised requires some kind of currency

                    // Get the item type of the required currency and set the required quantity of that item
                    currencyItem = itemManager.GetItemWithId(itemToCustomise.CurrencyItemId);

                    requiredCurrencyQuantity = itemToCustomise.CurrencyItemQuantity;
                }

                requiredCurrencyQuantity += additionalRequiredCurrency;

                if ((currencySlot.ItemStack.StackSize >= requiredCurrencyQuantity) && (currencyItem == null || currencySlot.ItemStack.StackItemsID == currencyItem.Id))
                {
                    // Either no currency is required or the player has added the required amount/type of currency item
                    //  - customising setup is valid!

                    infoText.text = "<color=#FFFFFF>Customising " + itemToCustomise.UIName + ".</color>";

                    validCustomiseItem = true;
                    resultSlotEnabled = true;
                }
                else
                {
                    // The required currency type or amount is not in place, setup is invalid. Dhow warning text

                    infoText.text = "Requires " + requiredCurrencyQuantity + "x " + currencyItem.UIName + ".";

                    validCustomiseItem = true;
                }
            }
            else
            {
                // The item in the customise slot is not allowed to be customised, setup is invalid. Show warning text

                infoText.text = itemToCustomise.UIName + " cannot be customised.";

                validCustomiseItem = false;
            }
        }
        else
        {
            // There is nothing in the customise slot, setup is invalid. Show info text

            ShowDefaultInfoText();

            validCustomiseItem = false;
            resultSlotEnabled = true;
        }

        if (resultSlotEnabled)
        {
            // The result should be enabled, allow the player to remove items and its the cover
            resultSlot.SlotUI.ClickToRemoveItems = true;
            resultSlot.SlotUI.SetCoverFillAmount(0.0f);
        }
        else
        {
            // The result slot should not be enabled, don't the player to remove items and cover it so the item is visually 'blocked'
            resultSlot.SlotUI.ClickToRemoveItems = false;
            resultSlot.SlotUI.SetCoverFillAmount(1.0f);
        }
    }

    private void ShowDefaultInfoText()
    {
        // Standard info text to display when nothing is being customised

        infoText.text = "<color=#FFFFFF>Place an item in the first slot to customise it.</color>";
    }

    public void OnCustomNameInputChanged(string value)
    {
        // Called when editing is done on the custom name input field

        if (!string.IsNullOrWhiteSpace(value))
        {
            // The entered name is valid, give the customised item the new name

            SetCustomisedItemName(value);
        }
        else
        {
            //The entered name is invalid, give the customised item its default UI name

            SetCustomisedItemName(itemManager.GetItemWithId(customiseSlot.ItemStack.StackItemsID).UIName);
        }
    }

    private void SetCustomisedItemName(string name)
    {
        // Ensure the input field text matches the name being set
        customNameInput.text = name;

        // Set the name of the customised item to the new name
        itemManager.SetCustomItemUIName(itemManager.GetUniqueCustomItemId(), name);
    }
}