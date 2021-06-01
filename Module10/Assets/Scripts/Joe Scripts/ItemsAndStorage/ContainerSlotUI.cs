using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// ||=======================================================================||
// || ContainerSlotUI: Attached to a UI element that displays an icon       ||
// ||    and quantity number for the items in a ContainerSlot.              ||
// ||=======================================================================||
// || Used on prefab: Joe/UI/ContainerSlot                                  ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public class ContainerSlotUI : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private UIPanel                parentPanel;                // The UI panel that this slot is a child of
    [SerializeField] private Image                  itemImage;                  // The image displaying the icon of the item type stored in the slot
    [SerializeField] private Image                  coverImage;                 // Image that covers the slot UI, can be used as a highlight/fill
    [SerializeField] private GameObject             itemCountPanel;             // Panel containing itemCountText
    [SerializeField] private TextMeshProUGUI        itemCountText;              // Text showing the number of items in the slot (if > 1)
    [SerializeField] private UnityEngine.UI.Outline outline;                    // Outline of the main slot panel

    [SerializeField] private Color                  standardOutlineColour;      // Colour to use for the outline by default
    [SerializeField] private Color                  selectedOutlineColour;      // Colour to use for the outline when the slot is selected (e.g. in the player's hotbar)

    [SerializeField] private bool                   clickToAddItems     = true; // Whether the player can click on this slot to add items that are in their hand
    [SerializeField] private bool                   clickToRemoveItems  = true; // Whether the player can click on this slot to transfer items to their hand

    #endregion

    #region Properties

    public Image ItemImage { get { return itemImage; } }
    public GameObject ItemCountPanel { get { return itemCountPanel; } }
    public TextMeshProUGUI ItemCountText { get { return itemCountText; } }
    public ContainerSlot Slot { get { return slot; } }

    #endregion


    protected   ContainerSlot   slot;           // The container slot this UI element is linked to
    private     HandSlotUI      handSlotUI;     // The slot that allows the player to hold/move items

    private void Start()
    {
        // Hide the item image/count as the slot is empty by default
        itemImage.gameObject    .SetActive(false);
        itemCountPanel          .SetActive(false);
        itemCountText.gameObject.SetActive(false);

        // Get a reference to the player's hand slot used for moving items
        handSlotUI = GameObject.FindGameObjectWithTag("HandSlot").GetComponent<HandSlotUI>();
    }

    public void LinkToContainerSlot(ContainerSlot slot)
    {
        // Connects the UI element to a container slot object by giving them references to each other
        //   so changes to the object can be reflected in the UI
        this.slot = slot;
        slot.SlotUI = this;
    }

    public void UpdateUI()
    {
        // Updates the slot UI to display the item type/quantity of the linked ContainerSlot

        if(slot != null)
        {
            // Get the number of items in the linked slot's item stack
            int stackSize = slot.ItemStack.StackSize;

            if (stackSize > 0 && !string.IsNullOrEmpty(slot.ItemStack.StackItemsID))
            {
                // At least 1 item is being stored in the slot's stack

                // Get the type of the items in the stack
                Item item = ItemManager.Instance.GetItemWithId(slot.ItemStack.StackItemsID);

                if (item != null)
                {
                    // Set the item image to display the contained item type's sprite
                    ItemImage.sprite = item.Sprite;
                }

                // Show the item image GameObject so the icon is displayed
                ItemImage.gameObject.SetActive(stackSize > 0);
            }
            else
            {
                // No items in the stack, no need to display an image preview
                ItemImage.gameObject.SetActive(false);
            }

            // Also display the number of items in the stack if there are more than 1
            ItemCountText.text = stackSize.ToString();
            ItemCountPanel          .SetActive(stackSize > 1);
            ItemCountText.gameObject.SetActive(stackSize > 1);
        }
        else
        {
            // This slot UI was not linked to a ContainerSlot object
            ErrorNotLinked();
        }
    }

    public void SetCoverFillAmount(float value)
    {
        // Alters how much of the cover image is shown (cover image uses a vertical fill)

        if(value == 0.0f)
        {
            // Showing none of the cover image, hide its GameObject
            coverImage.gameObject.SetActive(false);
        }
        else
        {
            // Show a certain amount of the image based on the given value
            coverImage.gameObject.SetActive(true);
            coverImage.fillAmount = value;
        }
    }

    public void SetSelected(bool selected)
    {
        // Selects or deselects the slot UI

        if (selected)
        {
            // Slot is selected, show a slightly thicker outline with the selected colour
            outline.effectDistance = new Vector2(2f, 2f);
            outline.effectColor = selectedOutlineColour;
        }
        else
        {
            // Slot is not selected, show a thinner outline with the standard colour
            outline.effectDistance = Vector2.one;
            outline.effectColor = standardOutlineColour;
        }
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        // Ignore pointer enter events if the parent panel is hidden
        if (parentPanel.Showing)
        {
            if (slot != null)
            {
                if (slot.ItemStack.StackSize > 0)
                {
                    // Player is hovering over the slot while it contains some items, show an info popup
                    //   that displays the name/description of the item in the slot
                    slot.ParentContainer.ItemInfoPopup.ShowPopup(slot.ItemStack.StackItemsID);
                }
            }
            else
            {
                // This slot UI was not linked to a ContainerSlot object
                ErrorNotLinked();
            }
        }
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        if (slot != null)
        {
            // Player is no longer hovering over the slot, hide the info popup
            slot.ParentContainer.ItemInfoPopup.HidePopup();
        }
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        // Ignore pointer down events if the parent panel is hidden
        if (parentPanel.Showing)
        {
            if (slot != null)
            {
                // Snap the hand slot position to mouse position
                handSlotUI.transform.position = Input.mousePosition;

                // Check if the player clicked with the right mouse button
                bool rightClick = (eventData.button == PointerEventData.InputButton.Right);

                if (clickToRemoveItems && slot.ItemStack.StackSize > 0)
                {
                    // Clicking to remove items is allowed

                    // The linked slot contains some items, try and move them to the hand slot
                    slot.MoveItemsToOtherSlot(handSlotUI.Slot, rightClick);
                }

                else if (clickToAddItems && handSlotUI.Slot.ItemStack.StackSize > 0)
                {
                    // Clicking to add items is allowed

                    // The hand slot contains some items, try and move them to the linked slot
                    handSlotUI.Slot.MoveItemsToOtherSlot(slot, rightClick);
                }
            }
            else
            {
                // This slot UI was not linked to a ContainerSlot object
                ErrorNotLinked();
            }
        }
    }

    private void ErrorNotLinked()
    {
        // ContainerSlot not linked error
        Debug.LogError("Slot UI not linked to a ContainerSlot");
    }
}
