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

public class InventoryBin : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        // Called when the bin is clicked

        // Get the slot used for holding/moving items
        HandSlotUI handSlotUI = GameObject.FindGameObjectWithTag("HandSlot").GetComponent<HandSlotUI>();

        // Get the number of items in the player's hand
        int handStackSize = handSlotUI.Slot.ItemStack.StackSize;

        if (handStackSize > 0)
        {
            //The hand stack contains at least one item, remove all items from it
            for (int i = 0; i < handStackSize; i++)
            {
                handSlotUI.Slot.ItemStack.TryRemoveItemFromStack();
            }

            //Update hand slot UI to show the player they are no longer holding items
            handSlotUI.UpdateUI();
        }
    }
}