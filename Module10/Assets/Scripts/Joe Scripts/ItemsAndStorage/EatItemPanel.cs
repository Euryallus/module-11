using UnityEngine;
using UnityEngine.EventSystems;

// ||=======================================================================||
// || EatItemPanel: A panel that the player can drag a consumable onto to   ||
// ||   'eat' the item (an alternative to holding click on a hotbar item)   ||        
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public class EatItemPanel : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        // Get a reference to the hand slot, used to 'hold' items
        HandSlotUI handSlotUI = GameObject.FindGameObjectWithTag("HandSlot").GetComponent<HandSlotUI>();

        if (handSlotUI.Slot.ItemStack.StackSize > 0)
        {
            // The hand slot contains at least one item

            // Find the item type in the player's hand
            Item itemInHand = ItemManager.Instance.GetItemWithId(handSlotUI.Slot.ItemStack.StackItemsID);

            if(itemInHand is ConsumableItem consumable)
            {
                // The held item type is a consumable (can be eaten)

                PlayerStats playerStats = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>();

                if (!playerStats.PlayerIsFull())
                {
                    // The player can eat the item

                    // Remove the item from the player's hand
                    handSlotUI.Slot.ItemStack.TryRemoveItemFromStack();

                    // Increase food level based on the consumable's hunger increase value
                    playerStats.IncreaseFoodLevel(consumable.HungerIncrease);

                    // Update the hand slot UI to show the item was removed
                    handSlotUI.UpdateUI();
                }
                else
                {
                    // The player is too full to eat, notify them
                    NotificationManager.Instance.AddNotificationToQueue(NotificationMessageType.PlayerTooFull);
                }
            }
            else
            {
                // The player clicked on the item panel with a non-consumable item, this should not be possible
                //   since the EatItemPanel should only be active when holding a consumable
                Debug.LogError("Should never be able to click on the eat item panel with a non-consumable item");
            }
        }
    }
}