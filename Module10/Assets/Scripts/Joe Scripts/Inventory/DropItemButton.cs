using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// Module 11

public class DropItemButton : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    public ItemContainer ParentContainer { get { return parentContainer; } }

    [SerializeField] private InventoryPanel inventoryPanel;
    [SerializeField] private ItemContainer  parentContainer;

    private HandSlotUI handSlotUI;

    public void OnPointerDown(PointerEventData eventData)
    {
        // Called when the button is clicked

        // Get the slot used for holding/moving items, if it hasn't been found already
        FindHandSlotUI();

        // Get the number of items in the player's hand
        int handStackSize = handSlotUI.Slot.ItemStack.StackSize;

        if (handStackSize > 0)
        {
            inventoryPanel.DropItemsInHand(false);

            // Hide the popup that showed what would be binned
            parentContainer.ItemInfoPopup.HidePopup();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Get the slot used for holding/moving items, if it hasn't been found already
        FindHandSlotUI();

        int heldItemCount = handSlotUI.Slot.ItemStack.StackSize;

        if (heldItemCount == 0)
        {
            // Player is not holding items, show that this button will drop any items being held
            parentContainer.ItemInfoPopup.ShowPopupWithText("Drop on Ground", "Click while holding item(s)");
        }
        else
        {
            Item heldItemType = ItemManager.Instance.GetItemWithId(handSlotUI.Slot.ItemStack.StackItemsID);

            // Player is holding items, show that this button will drop them, displaying the item count/name
            parentContainer.ItemInfoPopup.ShowPopupWithText("Drop on Ground", heldItemCount + "x " + heldItemType.UIName);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        parentContainer.ItemInfoPopup.HidePopup();
    }

    private void FindHandSlotUI()
    {
        if (handSlotUI == null)
        {
            handSlotUI = GameObject.FindGameObjectWithTag("HandSlot").GetComponent<HandSlotUI>();
        }
    }
}
