using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ||=======================================================================||
// || CraftingItemPreview: Shows a preview of an item that is required for  ||
// ||   the selected crafting recipe.                                       ||
// ||=======================================================================||
// || Used on prefab: Joe/UI/Crafting/RequiredItemPreview                   ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public class CraftingItemPreview : MonoBehaviour
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private Image              backgroundImage;        // The background of the UI element
    [SerializeField] private Image              itemImage;              // The image showing the required item for crafting
    [SerializeField] private GameObject         itemQuantityContainer;  // Panel containing item quantity text
    [SerializeField] private TextMeshProUGUI    itemQuantityText;       // Text showing the quantity of the item that will be crafted
    [SerializeField] private GameObject         itemWarning;            // Warning icon that is shown when the player does not have the required quantity of the item

    [SerializeField] private Color              validColour;            // Colour used when the required quantity of the item is in the player's inventory
    [SerializeField] private Color              invalidColour;          // Colour used when the player does not have the required quantity of the item

    #endregion

    public void Setup(bool valid, ItemGroup itemGroup)
    {
        if (valid)
        {
            // The player has the required quantity of the item, no warning required
            itemWarning.SetActive(false);
            backgroundImage.color = validColour;
        }
        else
        {
            // The player does not have the required quantity of the item, show warning
            itemWarning.SetActive(true);
            backgroundImage.color = invalidColour;
        }

        // Show the sprite of the required item
        itemImage.sprite = itemGroup.Item.Sprite;

        // Only show the required quantity if more than 1 of the item is needed
        itemQuantityContainer.SetActive(itemGroup.Quantity > 1);
        itemQuantityText.text = itemGroup.Quantity.ToString();
    }
}
