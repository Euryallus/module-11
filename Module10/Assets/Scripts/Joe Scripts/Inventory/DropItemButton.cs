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

    public void OnPointerDown(PointerEventData eventData)
    {
        // Called when the button is clicked

        // Get the slot used for holding/moving items
        HandSlotUI handSlotUI = GameObject.FindGameObjectWithTag("HandSlot").GetComponent<HandSlotUI>();

        // Get the number of items in the player's hand
        int handStackSize = handSlotUI.Slot.ItemStack.StackSize;

        if (handStackSize > 0)
        {
            // The hand stack contains at least one item, remove all items from it
            for (int i = 0; i < handStackSize; i++)
            {
                handSlotUI.Slot.ItemStack.TryRemoveItemFromStack();
            }

            // Update hand slot UI to show the player they are no longer holding items
            handSlotUI.UpdateUI();

            Item itemTypeToDrop = ItemManager.Instance.GetItemWithId(handSlotUI.Slot.ItemStack.StackItemsID);

            // Drop items
            inventoryPanel.DropItemGroup(new ItemGroup(itemTypeToDrop, handStackSize), false);

            AudioManager.Instance.PlaySoundEffect2D("throw");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        parentContainer.ItemInfoPopup.ShowPopupWithText("Drop On Ground", "Click while holding item(s)");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        parentContainer.ItemInfoPopup.HidePopup();
    }
}
