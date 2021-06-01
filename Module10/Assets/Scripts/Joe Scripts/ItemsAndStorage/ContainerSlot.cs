using System;

// ||=======================================================================||
// || ContainerSlot: A slot that can contain a stack of items and has a     ||
// ||   connected UI element to display its contents.                       ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public class ContainerSlot
{
    #region Properties

    public ItemContainer    ParentContainer { get { return parentContainer; } }
    public ContainerSlotUI  SlotUI          { get { return slotUI; } set { slotUI = value; } }
    public ItemStack        ItemStack       { get { return itemStack; } private set { itemStack = value; } }

    #endregion

    public  event Action    ItemsMovedEvent;    // Event invoked when an item is added/removed from the slot

    private ItemStack       itemStack;          // The items stored in this container slot
    private ContainerSlotUI slotUI;             // The UI object connected to this slot
    private ItemContainer   parentContainer;    // The ItemContainer that this slot is a child of

    // Constructor
    public ContainerSlot(int maxItemCapacity, ItemContainer parentContainer)
    {
        // Initialise the item stack with the given max capacity (0 = no max capacity)
        ItemStack = new ItemStack(this, maxItemCapacity);

        // Set the parent ItemContainer for this slot
        this.parentContainer = parentContainer;
    }

    public void MoveItemsToOtherSlot(ContainerSlot otherSlot, bool moveHalf = false)
    {
        // Size of the item stack to be moved to another slot
        int moveStackSize = itemStack.StackSize;

        if(moveHalf)
        {
            // The player has chosen to only move half of this slot's items
            moveStackSize = itemStack.StackSize / 2;
        }

        for (int i = 0; i < moveStackSize; i++)
        {
            // Attempt to move each item to the other slot

            if (otherSlot.ItemStack.AddItemToStack(itemStack.StackItemsID))
            {
                // The item can be added to the other slot, add it and remove it from this one

                itemStack.TryRemoveItemFromStack();
            }
            else
            {
                // The item cannot be added to the other slot (it is probably full),
                //  stop transferring items between slots

                break;
            }
        }

        // Update the UI of this slot and the slot that items are being moved to
        //   so they accurately display the amount if items in each one
        slotUI.UpdateUI();
        otherSlot.SlotUI.UpdateUI();

        // Items were moved, invoke the ItemsMovedEvent for both slots if they are not null
        ItemsMovedEvent?.Invoke();
        otherSlot.ItemsMovedEvent?.Invoke();
    }
}
