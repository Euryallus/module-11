using UnityEngine;

// ||=======================================================================||
// || HeldConsumable: A held item that can be eaten by the player.          ||
// ||=======================================================================||
// || Used on prefab: HeldItems/Consumable                                  ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public class HeldConsumable : HeldItem
{
    [Header("Consumable")]
    [SerializeField] private SoundClass eatSound;   // Sound to be played when the item is eaten

    public override void StartSecondardAbility()
    {
        base.StartSecondardAbility();

        if(item is ConsumableItem consumable)
        {
            // Get a reference to the player's PlayerStats script
            PlayerStats playerStats = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>();

            if (!playerStats.PlayerIsFull())
            {
                // The player can eat

                // Increase the player's food level depending on the item's HungerIncrease value
                playerStats.IncreaseFoodLevel(consumable.HungerIncrease);

                // Remove the item that was consumed from its stack
                containerSlot.Slot.ItemStack.TryRemoveItemFromStack();

                // Update the slot UI to show that an item was removed
                containerSlot.UpdateUI();

                if(eatSound != null)
                {
                    // Play the eat sound if one was set
                    AudioManager.Instance.PlaySoundEffect2D(eatSound);
                }
            }
            else
            {
                // Notify the player that they are full
                NotificationManager.Instance.AddNotificationToQueue(NotificationMessageType.PlayerTooFull);
            }
        }
        else
        {
            Debug.LogError("HeldConsumable script should never be attached to a non-consulable item");
        }
    }
}