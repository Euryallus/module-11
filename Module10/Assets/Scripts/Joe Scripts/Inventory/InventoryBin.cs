using UnityEngine;
using UnityEngine.EventSystems;

// ||=======================================================================||
// || InventoryBin: Used to destroy/discard items that are the player does  ||
// ||   not need to make space in their inventory                           ||
// ||=======================================================================||
// || Used on prefab: Joe/UI/Inventory/InventoryBinPanel                    ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

// Edited in module 11: some items cannot be binned, shows notification when trying to throw away one of these items

public class InventoryBin : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    public ItemContainer ParentContainer { get { return parentContainer; } }

    [SerializeField] private ItemContainer parentContainer;

    private HandSlotUI handSlotUI;

    public void OnPointerDown(PointerEventData eventData)
    {
        // Called when the bin is clicked

        // Get the slot used for holding/moving items, if it hasn't been found already
        FindHandSlotUI();

        // Get the number of items in the player's hand
        int handStackSize = handSlotUI.Slot.ItemStack.StackSize;

        if (handStackSize > 0)
        {
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
                parentContainer.ItemInfoPopup.HidePopup();

                AudioManager.Instance.PlaySoundEffect2D("bin");
            }
            else
            {
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
            parentContainer.ItemInfoPopup.ShowPopupWithText("Bin Permanently", "Click while holding item(s)");
        }
        else
        {
            Item heldItemType = ItemManager.Instance.GetItemWithId(handSlotUI.Slot.ItemStack.StackItemsID);

            // Player is holding items, show that this button will bin them, displaying the item count/name
            parentContainer.ItemInfoPopup.ShowPopupWithText("Bin Permanently", heldItemCount + "x " + heldItemType.UIName);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        parentContainer.ItemInfoPopup.HidePopup();
    }

    private void FindHandSlotUI()
    {
        if(handSlotUI == null)
        {
            handSlotUI = GameObject.FindGameObjectWithTag("HandSlot").GetComponent<HandSlotUI>();
        }
    }
}