using UnityEngine;
using TMPro;

// ||=======================================================================||
// || ItemInfoPopup: A UI popup that displays info about an item such as    ||
// ||   name, description and custom properties.                            ||
// ||=======================================================================||
// || Used on prefab: Joe/UI/ItemInfoPopup                                  ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
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

    private bool            canShow;        // Whether the popup is allowed to be shown
    private bool            showing;        // Whether the popup is currently showing
    private Canvas          canvas;         // The Canvas containing the popup
    private RectTransform   rectTransform;  // The popup GameObject's RectTransform for getting its size

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

    public void SetCanShow(bool canShow)
    {
        if(!canShow && showing)
        {
            // The popup is showing but is no longer allowed to, hide it
            HidePopup();
        }

        this.canShow = canShow;
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

            // Move the popup to the mouse pointer position, and clamp the position to be within screen bounds
            transform.position = new Vector2(Mathf.Clamp(Input.mousePosition.x, width, Screen.width - width), Mathf.Clamp(Input.mousePosition.y, height, Screen.height - height));
        }
    }

    public void ShowPopup(string itemId)
    {
        Item item = ItemManager.Instance.GetItemWithId(itemId);

        if (canShow)
        {
            //The popup can be shown

            if (item != null)
            {
                // Show the popup with info based on the item's properties
                UpdatePopupInfo(item.UIName, item.UIDescription, item.CustomItem, item.BaseItemId, item.CustomFloatProperties, item.CustomStringProperties);
            }
            else
            {
                // Item is null, show an error popup
                UpdatePopupInfo("Error: Unknown Item", "", false, "", new CustomFloatProperty[] { }, new CustomStringProperty[] { });
            }
        }

        // The popup is now being shown
        showing = true;
    }

    public void HidePopup()
    {
        showing = false;
        canvasGroup.alpha = 0.0f;
    }

    private void UpdatePopupInfo(string itemName, string itemDescription, bool customItem, string baseItemId,
                                    CustomFloatProperty[] customFloatProperties, CustomStringProperty[] customStringProperties)
    {
        // Set item name text
        itemNameText.text = itemName;

        if (customItem)
        {
            // Item is customised - show text displaying the name of the original item it's based on
            itemCustomisedText.gameObject.SetActive(true);
            itemCustomisedText.text = ItemManager.Instance.GetItemWithId(baseItemId).UIName;
        }
        else
        {
            // Item is not customised, hide itemCustomisedText
            itemCustomisedText.gameObject.SetActive(false);
        }

        if (!string.IsNullOrWhiteSpace(itemDescription))
        {
            // Item has a description, display it
            itemDescriptionText.gameObject.SetActive(true);
            itemDescriptionText.text = itemDescription;
        }
        else
        {
            // No description to show, hide itemDescriptionText
            itemDescriptionText.gameObject.SetActive(false);
        }

        if (customFloatProperties.Length > 0 || customStringProperties.Length > 0)
        {
            // Item has some sort of custom property/properties, show custom property info
            ShowCustomPropertyInfo(customFloatProperties, customStringProperties);
        }
        else
        {
            // Item has no custom properties, hide customPropertiesText
            customPropertiesText.gameObject.SetActive(false);
        }
    }

    private void ShowCustomPropertyInfo(CustomFloatProperty[] customFloatProperties, CustomStringProperty[] customStringProperties)
    {
        customPropertiesText.text = "";

        int shownPropertyCount = customFloatProperties.Length; // Number of custom properties (and hence lines of text) to show

        // Loop through all custom float properties
        for (int i = 0; i < customFloatProperties.Length; i++)
        {
            // Add a line of text showing info about each custom float property - the property name and value
            customPropertiesText.text += (customFloatProperties[i].UIName + ": " + customFloatProperties[i].Value);
            customPropertiesText.text += "\n";
        }

        // Loop through all custom string properties
        for (int i = 0; i < customStringProperties.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(customStringProperties[i].Value))
            {
                // The current property is not an empty string, show info about it
                shownPropertyCount++;

                // Either show the full string value, or remove the end and append a "..." if it's longer than 10 characters
                string value = customStringProperties[i].Value;
                if (value.Length > 10)
                {
                    value = value.Remove(10);
                    value += "...";
                }

                // Add a line of text showing the property name and value
                customPropertiesText.text += (customStringProperties[i].UIName + ": " + value);
                customPropertiesText.text += "\n";
            }
        }

        // Adjust the size of the customPropertiesText GameObject based on the number of lines being shown so the text all fits onto the popup
        customPropertiesText.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(150.0f, 16.0f * shownPropertyCount);

        // Show the added text
        customPropertiesText.gameObject.SetActive(true);
    }
}