using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

// ||=======================================================================||
// || CraftingItemButton: A button used to select a certain crafting recipe ||
// ||   within the crafting menu interface.                                 ||
// ||=======================================================================||
// || Used on prefab: Joe/UI/Crafting/CraftingItemButton                    ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public class CraftingItemButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private Image              backgroundImage;        // Button background image
    [SerializeField] private Image              itemImage;              // Image displaying the item this button is used to craft
    [SerializeField] private GameObject         itemQuantityContainer;  // Panel containing item quantity text
    [SerializeField] private TextMeshProUGUI    itemQuantityText;       // Text showing the quantity of the item that will be crafted

    [SerializeField] private Color              standardColour;         // Colour when the button is not selected
    [SerializeField] private Color              selectedColour;         // Colour when the button is selected

    #endregion

    private CraftingPanel   parentPanel;    // The crafting panel containing this button
    private CraftingRecipe  recipe;         // The recipe that defines the required item(s)/result item related to this button

    public void Setup(CraftingPanel parentPanel, CraftingRecipe recipe)
    {
        this.parentPanel    = parentPanel;
        this.recipe         = recipe;

        // Setup UI elements to display item image/quantity text

        itemImage.sprite = recipe.ResultItem.Item.Sprite;

        // Only show quantity text if more that 1 will be crafted
        itemQuantityContainer.SetActive(recipe.ResultItem.Quantity > 1);

        itemQuantityText.text = recipe.ResultItem.Quantity.ToString();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (parentPanel.Showing)
        {
            // Show a popup with info about the item to be crafted on hover
            parentPanel.InventoryPanel.ItemContainer.ItemInfoPopup.ShowPopup(recipe.ResultItem.Item.Id);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Hide the info popup when the pointer leaves the button
        parentPanel.InventoryPanel.ItemContainer.ItemInfoPopup.HidePopup();
    }

    public void OnClick()
    {
        if(parentPanel.SelectedRecipe != recipe)
        {
            // The recipe is not selected, select it on click
            parentPanel.SelectRecipe(recipe, this);
        }
        else
        {
            // The recipe is already selected, deselect it (so nothing is selected)
            parentPanel.SelectRecipe(null, null);
        }

        AudioManager.Instance.PlaySoundEffect2D("buttonClickSmall");
    }

    public void Select()
    {
        // The button is selected, change its colour to the selected colour
        backgroundImage.color = selectedColour;
    }

    public void Deselect()
    {
        // The button is not selected, change its colour back to the default colour
        backgroundImage.color = standardColour;
    }
}
