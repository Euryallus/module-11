using System;

// ||=======================================================================||
// || ContainerSlot: A slot that can contain a stack of items and has a     ||
// ||   connected UI element to display its contents.                       ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

//Updated for Mod11:
//  - Tweaks to allow moving items to slots with no slotUI set

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

        // Update the UI of this slot and the slot that items are being moved to (if they are connected to UI elements)
        //   so they accurately display the amount if items in each one
        if(slotUI != null)
        {
            slotUI.UpdateUI();
        }
        if(otherSlot.SlotUI != null)
        {
            otherSlot.SlotUI.UpdateUI();
        }

        // Items were moved, invoke the ItemsMovedEvent for both slots if they are not null
        ItemsMovedEvent?.Invoke();
        otherSlot.ItemsMovedEvent?.Invoke();
    }

    public void ReplaceItemInSlot(Item item)
    {
        // Replaces any existing items in this slot's ItemStack with the given item

        // First, remove any existing items from the stack
        for (int i = 0; i < itemStack.StackSize; i++)
        {
            itemStack.TryRemoveItemFromStack();
        }

        // Then add the new item and update the slot UI to show the new item
        itemStack.AddItemToStack(item.Id);
        slotUI.UpdateUI();
    }

    public bool IsEmpty()
    {
        return (string.IsNullOrEmpty(itemStack.StackItemsID) || itemStack.StackSize == 0);
    }
}
