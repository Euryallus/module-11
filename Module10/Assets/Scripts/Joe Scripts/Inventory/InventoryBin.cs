using UnityEngine;
using UnityEngine.EventSystems;

// ||=======================================================================||
// || InventoryBin: Used to destroy/discard items that are the player does  ||
// ||   not need to make space in their inventory                           ||
// ||=======================================================================||
// || Used on prefab: Joe/UI/Inventory/InventoryBinPanel                    ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || originally for the prototype phase.                                   ||
// ||                                                                       ||
// || Changes made during the production phase (Module 11):                 ||
// ||                                                                       ||
// || - Some items now cannot be binned, which triggers a notification      ||
// ||=======================================================================||

public class InventoryBin : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    private HandSlotUI handSlotUI; // Reference to the hand container slot

    public void OnPointerDown(PointerEventData eventData)
    {
        // Called when the bin is clicked

        // Get the slot used for holding/moving items, if it hasn't been found already
        FindHandSlotUI();

        // Get the number of items in the player's hand
        int handStackSize = handSlotUI.Slot.ItemStack.StackSize;

        if (handStackSize > 0)
        {
            // Get the item type being held that will potentially be binned
            Item itemBeingBinned = ItemManager.Instance.GetItemWithId(handSlotUI.Slot.ItemStack.StackItemsID);

            if(itemBeingBinned.CanThrowAway)
            {
                // The item type being held can be binned

                // The hand stack contains at least one item, remove all items from it
                for (int i = 0; i < handStackSize; i++)
                {
                    handSlotUI.Slot.ItemStack.TryRemoveItemFromStack();
                }

                // Update hand slot UI to show the player they are no longer holding items
                handSlotUI.UpdateUI();

                // Hide the popup that showed what would be binned
                GameSceneUI.Instance.ItemInfoPopup.HidePopup();

                AudioManager.Instance.PlaySoundEffect2D("bin");
            }
            else
            {
                // The item being held cannot be binned, notify the player
                NotificationManager.Instance.AddNotificationToQueue(NotificationMessageType.CantThrowAwayItem, new string[] { itemBeingBinned.UIName });
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
            // Player is not holding items, show that this button will bin any items being held
            GameSceneUI.Instance.ItemInfoPopup.ShowPopupWithText("Bin Permanently", "Click while holding item(s)");
        }
        else
        {
            Item heldItemType = ItemManager.Instance.GetItemWithId(handSlotUI.Slot.ItemStack.StackItemsID);

            // Player is holding items, show that this button will bin them, displaying the item count/name
            GameSceneUI.Instance.ItemInfoPopup.ShowPopupWithText("Bin Permanently", heldItemCount + "x " + heldItemType.UIName);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Hides the info popup when the pointer leaves the bin button
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