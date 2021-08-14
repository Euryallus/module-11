using UnityEngine;
using UnityEngine.EventSystems;

// ||=======================================================================||
// || DropItemButton: A button that lets the player drop an item from their ||
// ||   inventory/hotbar onto the ground.                                   ||
// ||=======================================================================||
// || Used on prefab: Joe/UI/Inventory/DropItemButton                       ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the production phase (Module 11).                                 ||
// ||=======================================================================||

public class DropItemButton : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    private HandSlotUI handSlotUI;  // Reference to the hand container slot

    public void OnPointerDown(PointerEventData eventData)
    {
        InventoryPanel inventoryPanel = GameSceneUI.Instance.PlayerInventory;

        // Called when the button is clicked

        // Get the slot used for holding/moving items, if it hasn't been found already
        FindHandSlotUI();

        // Get the number of items in the player's hand
        int handStackSize = handSlotUI.Slot.ItemStack.StackSize;

        if (handStackSize > 0)
        {
            // The player is holding something

            // Get the item type being held
            Item itemBeingDropped = ItemManager.Instance.GetItemWithId(handSlotUI.Slot.ItemStack.StackItemsID);

            if(itemBeingDropped.CanDrop)
            {
                // The item being held can be dropped

                inventoryPanel.DropItemsInHand(false);

                // Hide the popup that showed what would be binned
                GameSceneUI.Instance.ItemInfoPopup.HidePopup();

                AudioManager.Instance.PlaySoundEffect2D("throw");
            }
            else
            {
                // The item being held cannot be dropped, notify the player

                NotificationManager.Instance.AddNotificationToQueue(NotificationMessageType.CantDropItem, new string[] { itemBeingDropped.UIName });
            }
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
            GameSceneUI.Instance.ItemInfoPopup.ShowPopupWithText("Drop on Ground", "Click while holding item(s)");
        }
        else
        {
            // Get the item type being held
            Item heldItemType = ItemManager.Instance.GetItemWithId(handSlotUI.Slot.ItemStack.StackItemsID);

            // Player is holding items, show that this button will drop them, displaying the item count/name
            GameSceneUI.Instance.ItemInfoPopup.ShowPopupWithText("Drop on Ground", heldItemCount + "x " + heldItemType.UIName);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Hides the info popup when the pointer leaves the button
        GameSceneUI.Instance.ItemInfoPopup.HidePopup();
    }

    private void FindHandSlotUI()
    {
        // Finds the slot that acts as the player's 'hand' when they pick up an item from a chest/their hotbar/inventory
        if (handSlotUI == null)
        {
            handSlotUI = GameObject.FindGameObjectWithTag("HandSlot").GetComponent<HandSlotUI>();
        }
    }
}
