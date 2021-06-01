using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ||=======================================================================||
// || ShopBuyPanel: UI panel for purchasing items from a shop.              ||
// ||=======================================================================||
// || Used on prefab: Joe/UI/Shops/ShopBuyPanel                             ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public class ShopBuyPanel : MonoBehaviour
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [Header("Main UI")] [Header("Shop Buy Panel")]

    [SerializeField] private TextMeshProUGUI    shopNameText;           //UI text displaying the shop name
    [SerializeField] private Transform[]        itemRows;               //Used for displaying items in rows
    [SerializeField] private Transform          categoriesParent;       //Parent object for the shop's category tabs
    [SerializeField] private GameObject         shopItemPrefab;         //Prefab of the UI element used to display items that are for sale
    [SerializeField] private GameObject         shopCategoryPrefab;     //Prefab of the UI element used to select a shop category

    [Header("Buy Button UI")]

    [SerializeField] private PressEffectButton  buyButton;              //The button used to purchase items
    [SerializeField] private GameObject         buyButtonGameObj;       //Parent GameObject of the above button
    [SerializeField] private TextMeshProUGUI    buyButtonText;          //Text displayed on the above button

    [Header("Currency Display UI")]

    [SerializeField] private TextMeshProUGUI    currencyTitleText;      //UI text displaying the currency type for the chosen category
    [SerializeField] private TextMeshProUGUI    currencyQuantityText;   //UI text displaying the amount of currency the player has
    [SerializeField] private Image              currencyIcon;           //Icon displaying the sprite for the current currency type

    [Header("UI Colours")]

    [SerializeField] private Color              standardTabColour;      //Default colour of category tabs
    [SerializeField] private Color              selectedTabColour;      //Colour of category tabs when selected
    [SerializeField] private Color              standardButtonColour;   //Default colour of item selection buttons
    [SerializeField] private Color              selectedButtonColour;   //Colour of selected item selection buttons
    [SerializeField] private Color              buyButtonColour;        //Default colour of the buy button
    [SerializeField] private Color              cannotBuyColour;        //Colour of the buy button when the player does not have enough currency to make a purchase

    #endregion

    private InventoryPanel      inventoryPanel;             // The player's inventory panel
    private HotbarPanel         hotbarPanel;                // The player's hotbar panel
    private ShopNPC             shopNPC;                    // The NPC the player talked to when acessing this shop
    private ShopType            shopType;                   // The shop type - defines what items are sold at the shop
    private PressEffectButton[] categoryButtons;            // Buttons for selecting the different item categories for the shop
    private Image               selectedItemButton;         // Image attached to the selected item button
    private ShopItem            selectedItem;               // The selected item and its price at the shop
    private int                 selectedCategoryIndex = -1; // The index of the selected category in the shop type's categories array
                                                               
    private bool                itemPurchasable;            // Whether the selected item can be purchased
    private int                 inventoryCurrencyQuantity;  // Amount of the required currency in the player's inventory
    private int                 hotbarCurrencyQuantity;     // Amount of the required currency in the player's hotbar

    private const int ItemsPerRow           = 5;    // Number of items to display in each row of the shop's UI
    private const int MaxDisplayableItems   = 20;   // Maximum number of items that can be displayed in a single category

    private void Awake()
    {
        // Get references to the player's inventory and hotbar
        inventoryPanel  = GameObject.FindGameObjectWithTag("Inventory").GetComponent<InventoryPanel>();
        hotbarPanel     = GameObject.FindGameObjectWithTag("Hotbar").GetComponent<HotbarPanel>();
    }

    private void Update()
    {
        // Exit the shop if the esc key is pressed
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            ButtonLeave();
        }
    }

    public void Setup(ShopNPC npc)
    {
        // Setup variables based on the npc used to access the shop
        shopNPC = npc;
        shopType = npc.ShopType;
        shopNameText.text = shopType.UIName;

        // Hide the buy button since to item is selected by default
        buyButtonGameObj.SetActive(false);

        // Setup the array of category buttons
        categoryButtons = new PressEffectButton[shopType.Categories.Length];

        for (int i = 0; i < shopType.Categories.Length; i++)
        {
            // Add a new category button GameObject for each category
            GameObject categoryButton = Instantiate(shopCategoryPrefab, categoriesParent).transform.GetChild(0).gameObject;

            // Add the button component of the GameObject to the buttons array
            categoryButtons[i] = categoryButton.GetComponent<PressEffectButton>();

            // Set the button's text to be the current category name
            categoryButton.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = shopType.Categories[i].UIName;

            // The ButtonSelectCategory function is called on the button's click event with the corresponding category index
            int categoryIndex = i;
            categoryButton.GetComponent<Button>().onClick.AddListener(delegate { ButtonSelectCategory(categoryIndex); });
        }

        // Select the first category by default
        SelectCategory(0);
    }

    private void ButtonSelectCategory(int categoryIndex)
    {
        // Select the category and play a sound on button press
        SelectCategory(categoryIndex);

        AudioManager.Instance.PlaySoundEffect2D("buttonClickMain1");
    }

    private void SelectCategory(int categoryIndex)
    {
        // No item is selected by default when switching to a new category
        selectedItemButton = null;
        selectedItem = null;

        // Since nothing is selected, hide the buy button
        buyButtonGameObj.SetActive(false);

        // If another category was selected, reset the corresponding button's colour to default
        if (selectedCategoryIndex != -1)
        {
            categoryButtons[selectedCategoryIndex].SetButtonColour(standardTabColour);
        }

        // Set the selected category index to be the new index
        selectedCategoryIndex = categoryIndex;

        // Set the newly-selected button's colour
        categoryButtons[categoryIndex].SetButtonColour(selectedTabColour);

        // Get the selected category from the shop type's categories array
        ShopCategory selectedCategory = shopNPC.ShopType.Categories[categoryIndex];

        // Update the values for the quantity of currency items in the player's inventory and hotbar
        //   since each category can use a different currency type
        inventoryCurrencyQuantity   = inventoryPanel.ItemContainer.CheckForQuantityOfItem(selectedCategory.CurrencyItem);
        hotbarCurrencyQuantity      = hotbarPanel.ItemContainer.CheckForQuantityOfItem(selectedCategory.CurrencyItem);

        // Update the currency UI to show the values calculated above
        UpdateCurrencyUI();

        // Setup the UI to display items in the selected category
        SetupCategoryUI(selectedCategory);
    }

    private void SetupCategoryUI(ShopCategory category)
    {
        for (int i = 0; i < itemRows.Length; i++)
        {
            // Remove all existing item buttons before new ones are added
            foreach(Transform t in itemRows[i].transform)
            {
                Destroy(t.gameObject);
            }

            // Hide all item rows by default
            itemRows[i].gameObject.SetActive(false);
        }

        // Add buttons for all sold items in the selected category
        for (int i = 0; i < category.SoldItems.Length; i++)
        {
            // Ensure there are not more items to display that the max allowed number
            if (i < MaxDisplayableItems)
            {
                // Get the parent row, for example if there are 5 items per row and this is the 6th item, this will return the 2nd row
                Transform parent = itemRows[i / ItemsPerRow];

                // Ensure the parent row is being displayed
                parent.gameObject.SetActive(true);

                // Add a GameObject to display the current item
                GameObject shopItem = Instantiate(shopItemPrefab, parent);
                Transform shopItemBG = shopItem.transform.Find("BG");
                Transform pricePanel = shopItemBG.Find("PricePanel");

                // When the button on the created GameObject us clicked, the corresponding item will be selected
                int index = i;
                shopItem.GetComponent<Button>().onClick.AddListener(delegate { SelectItemButton(shopItem.GetComponent<Image>(), category.SoldItems[index]); });

                // Set the item icon, currency icon and text displaying the item's price
                shopItemBG.Find("ItemIcon").GetComponent<Image>().sprite = category.SoldItems[i].Item.Sprite;
                pricePanel.Find("CurrencyItem").GetComponent<Image>().sprite = category.CurrencyItem.Sprite;
                pricePanel.Find("PriceText").GetComponent<TextMeshProUGUI>().text = category.SoldItems[i].Price.ToString();
            }
            else
            {
                Debug.LogWarning("Shop category has more items than can be displayed (" + shopType.UIName + ", " + category.UIName + ")");
                break;
            }
        }
    }

    private void SelectItemButton(Image itemButton, ShopItem shopItem)
    {
        // If another item button was selected, reset its colour to default
        if (selectedItemButton != null)
        {
            selectedItemButton.color = standardButtonColour;
        }

        // Set the new button to be the selected one and update its colour to show it's selected
        selectedItemButton = itemButton;
        selectedItemButton.color = selectedButtonColour;

        // Select the item that the button corresponds to
        SelectItem(shopItem);

        AudioManager.Instance.PlaySoundEffect2D("buttonClickSmall");
    }

    private void SelectItem(ShopItem shopItem)
    {
        if(shopItem != null)
        {
            // Set the given item to be the selected item
            selectedItem = shopItem;

            // Show the buy button and update its text based on the selected item
            buyButtonGameObj.SetActive(true);
            buyButtonText.text = "Buy " + shopItem.Item.UIName + " for " + shopItem.Price + " " + shopType.Categories[selectedCategoryIndex].CurrencyItem.UIName;

            // Check if the player has enough currency in their inventory/hotbar to buy the item
            itemPurchasable = ((inventoryCurrencyQuantity + hotbarCurrencyQuantity) >= shopItem.Price);

            // Change the colour of the buy button depending on if the item can be bought
            if(itemPurchasable)
            {
                buyButton.SetButtonColour(buyButtonColour);
            }
            else
            {
                buyButton.SetButtonColour(cannotBuyColour);
            }
        }
        else
        {
            // Nothing is selected
            selectedItem = null;
            buyButtonGameObj.SetActive(false);
        }
    }

    private void UpdateCurrencyUI()
    {
        // Get the item being used as currency
        Item currencyItem = shopType.Categories[selectedCategoryIndex].CurrencyItem;

        // Update currency UI to display the item and how much of it remains in the player's inventory/hotbar
        currencyTitleText.text = "Remaining " + currencyItem.UIName + ":";

        currencyIcon.sprite = currencyItem.Sprite;
        currencyQuantityText.text = (hotbarCurrencyQuantity + inventoryCurrencyQuantity).ToString();
    }

    public void ButtonBuy()
    {
        if (itemPurchasable)
        {
            // The player has enough currency to buy the item

            // Remove spent currency
            for (int i = 0; i < selectedItem.Price; i++)
            {
                if(inventoryCurrencyQuantity > 0)
                {
                    // Favour removing the currency item from the player's inventory before their hotbar
                    inventoryPanel.RemoveItemFromInventory(shopType.Categories[selectedCategoryIndex].CurrencyItem);
                    inventoryCurrencyQuantity--;
                }
                else if(hotbarCurrencyQuantity > 0)
                {
                    // There is no currency left in the inventory, take it from the hotbar instead
                    hotbarPanel.RemoveItemFromHotbar(shopType.Categories[selectedCategoryIndex].CurrencyItem);
                    hotbarCurrencyQuantity--;
                }
            }

            // Add the purchased item to the player's inventory
            inventoryPanel.AddItemToInventory(selectedItem.Item);

            // Re-select the selected item to repeat checks and confirm if they can/cannot buy it now
            SelectItem(selectedItem);

            // Update the currency UI to show how much currency if left
            UpdateCurrencyUI();

            AudioManager.Instance.PlaySoundEffect2D("coins");
        }
        else
        {
            // The item cannot be bought - tell the player
            NotificationManager.Instance.AddNotificationToQueue(NotificationMessageType.CantAffordItem,
                new string[] { selectedItem.Price.ToString(), shopType.Categories[selectedCategoryIndex].CurrencyItem.UIName });
        }
    }

    public void ButtonLeave()
    {
        AudioManager.Instance.PlaySoundEffect2D("buttonClickMain2");

        // Stop interacting with the NPC used to access the shop - returns camera to normal
        shopNPC.StopInteracting();

        // Destroy this buy panel as it's no longer needed
        Destroy(gameObject);
    }
}
