using UnityEngine;
using TMPro;

// ||=======================================================================||
// || ItemInfoPopup: A UI popup that displays info about an item such as    ||
// ||   name, description and custom properties.                            ||
// ||=======================================================================||
// || Used on prefab: Joe/UI/ItemInfoPopup                                  ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || originally for the prototype phase.                                   ||
// ||                                                                       ||
// || Changes made during the production phase (Module 11):                 ||
// ||                                                                       ||
// || - ItemInfoPopups can now display custom item properties               ||
// || - Can also now display any generic text using UpdatePopupInfo         ||
// ||    overload that takes two string values                              ||
// ||=======================================================================||

[RequireComponent(typeof(CanvasGroup))] [RequireComponent(typeof(RectTransform))]
public class ItemInfoPopup : MonoBehaviour
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private TextMeshProUGUI itemNameText;          // Text displaying the name of the item that info is being shown about
    [SerializeField] private TextMeshProUGUI itemDescriptionText;   // Text displaying the description of the item
    [SerializeField] private TextMeshProUGUI itemCustomisedText;    // Text showing whether the item is customised
    [SerializeField] private TextMeshProUGUI customPropertiesText;  // Text displaying info about the item's custom properties
    [SerializeField] private CanvasGroup     canvasGroup;           // Canvas group for fading the popup in/out

    #endregion

    private bool            canShowItemInfo; // Whether popups containing item info are allowed to be shown
    private bool            showing;         // Whether the popup is currently showing
    private Canvas          canvas;          // The Canvas containing the popup
    private RectTransform   rectTransform;   // The popup GameObject's RectTransform for getting its size

    private readonly Vector3 MouseOffset = new Vector3(10.0f, -5.0f, 0.0f); // Offset from the mouse pointer when displaying the popup

    private void Awake()
    {
        canvas          = GameObject.FindGameObjectWithTag("JoeCanvas").GetComponent<Canvas>();
        rectTransform   = GetComponent<RectTransform>();
    }

    private void Start()
    {
        // Hide the popup by default
        HidePopup();
    }

    public void SetCanShowItemInfo(bool canShow)
    {
        // Allow/disallow popups containing item info to be shown

        if(canShow != canShowItemInfo)
        {
            // The canShowItemInfo value is being changed

            if (!canShow && showing)
            {
                // The popup is showing but is no longer allowed to, hide it
                HidePopup();
            }

            canShowItemInfo = canShow;
        }
    }

    void Update()
    {
        if (showing)
        {
            // Fade the popup in slightly each frame that it's showing
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 1.0f, Time.unscaledDeltaTime * 25.0f);
            
            // Get the current width/height of the popup (scaled based on canvas scale factor)
            float width =   (rectTransform.rect.width / 2)  * canvas.scaleFactor;
            float height =  (rectTransform.rect.height / 2) * canvas.scaleFactor;

            Vector3 targetPos = Input.mousePosition + MouseOffset;

            // Move the popup to the mouse pointer position, and clamp the position to be within screen bounds
            transform.position = new Vector2(Mathf.Clamp(targetPos.x, 0, Screen.width - width  * 2.0f), Mathf.Clamp(targetPos.y, 0, Screen.height - height * 2.0f));
        }
    }

    public void ShowPopupWithItemInfo(string itemId, bool showMaxFloatValues = false)
    {
        // For showing popups with info about an item with the given id

        Item item = ItemManager.Instance.GetItemWithId(itemId);

        if (canShowItemInfo)
        {
            // The popup can be shown

            if (item != null)
            {
                // Show the popup with info based on the item's properties
                UpdatePopupInfo(item, showMaxFloatValues);
            }
            //else
            //{
            //    // Item is null, show an error popup
            //    UpdatePopupInfo("Error: Unknown Item", "", false, "", new CustomFloatProperty[] { }, new CustomStringProperty[] { });
            //}

            // The popup is now being shown
            showing = true;
        }
    }

    public void ShowPopupWithText(string mainText, string secondaryText = "")
    {
        // For showing popups with any specified text

        // Display the given text on the popup
        UpdatePopupInfo(mainText, secondaryText);

        // The popup is now being shown
        showing = true;
    }

    public void HidePopup()
    {
        showing = false;
        canvasGroup.alpha = 0.0f;
    }

    private void UpdatePopupInfo(Item item, bool showMaxFloatValues)
    {
        // Updates popup UI to display various info about an item

        // Set item name text
        itemNameText.text = item.UIName;

        if (item.CustomItem)
        {
            Item baseItem = ItemManager.Instance.GetItemWithId(item.BaseItemId);

            // Item is customised - show text displaying the name of the original item it's based on
            itemCustomisedText.gameObject.SetActive(true);
            itemCustomisedText.text = baseItem.UIName;
        }
        else
        {
            // Item is not customised, hide itemCustomisedText
            itemCustomisedText.gameObject.SetActive(false);
        }

        if (!string.IsNullOrWhiteSpace(item.UIDescription))
        {
            // Item has a description, display it
            itemDescriptionText.gameObject.SetActive(true);
            itemDescriptionText.text = item.UIDescription;
        }
        else
        {
            // No description to show, hide itemDescriptionText
            itemDescriptionText.gameObject.SetActive(false);
        }

        if (item.CustomFloatProperties.Length > 0 || item.CustomStringProperties.Length > 0)
        {
            // Item has some sort of custom property/properties, show custom property info
            ShowCustomPropertyInfo(item.CustomFloatProperties, item.CustomStringProperties, showMaxFloatValues);
        }
        else
        {
            // Item has no custom properties, hide customPropertiesText
            customPropertiesText.gameObject.SetActive(false);
        }
    }

    private void UpdatePopupInfo(string mainText, string secondaryText)
    {
        // Updates popup UI to display any generic text rather than info about an item

        // Use item name text to display main text
        itemNameText.text = mainText;

        // Show description text if any secondary text was given
        itemDescriptionText.gameObject.SetActive(!string.IsNullOrEmpty(secondaryText));

        // Use item description text to display secondary text
        itemDescriptionText.text = secondaryText;

        // Hide all other text that would be used for showing various info about an item
        itemCustomisedText.gameObject.SetActive(false);
        customPropertiesText.gameObject.SetActive(false);
    }

    private void ShowCustomPropertyInfo(CustomFloatProperty[] customFloatProperties, CustomStringProperty[] customStringProperties, bool showMaxFloatValues)
    {
        customPropertiesText.text = "";

        int shownPropertyCount = customFloatProperties.Length; // Number of custom properties (and hence lines of text) to show

        // Loop through all custom float properties

        foreach (var property in customFloatProperties)
        {
            // Add a line of text showing info about each custom float property - the property name and value
            string floatValText = property.GetDisplayText();

            if (showMaxFloatValues)
            {
                floatValText += " <color=#FFFFA0>(Max: " + property.MaxValue + ")</color>";
            }

            customPropertiesText.text += floatValText;
            customPropertiesText.text += "\n";
        }

        // Loop through all custom string properties

        foreach (var property in customStringProperties)
        {
            if (!string.IsNullOrWhiteSpace(property.Value))
            {
                // The current property is not an empty string, show info about it
                shownPropertyCount++;

                if (property.Name.Contains("control"))
                {
                    // Use a different text colour for a property displaying control info
                    customPropertiesText.text += "<color=#FF915D>";
                }

                // Add a line of text showing the property name and value
                customPropertiesText.text += (property.GetDisplayText());
                customPropertiesText.text += "\n";
            }
        }

        // Adjust the size of the customPropertiesText GameObject based on the number of lines being shown so the text all fits onto the popup
        customPropertiesText.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(150.0f, 16.0f * shownPropertyCount);

        // Show the added text
        customPropertiesText.gameObject.SetActive(true);
    }
}