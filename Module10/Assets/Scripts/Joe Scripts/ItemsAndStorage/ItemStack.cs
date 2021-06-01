// ||=======================================================================||
// || ItemStack: A group of items of the same type that is linked to a      ||
// || ContainerSlot and allows items to be dynamically added and removed.   ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public class ItemStack
{
    #region Properties

    public string   StackItemsID    { get { return m_stackItemsId; } }
    public int      StackSize       { get { return m_stackSize; } }
    public float    StackWeight     { get { return m_stackWeight; } }

    #endregion

    private string          m_stackItemsId; // Id of the item type the stack holds
    private int             m_stackSize;    // Number of items in the stack
    private int             m_maxStackSize; // Maximum number of items allowed in the stack (0 = no limit), regardless of stack size of the contained item
                                            //   (i.e. if an item stacks to 64 but this variable is set to 2, no more than 2 items will ever be allowed)
    private float           m_stackWeight;  // Combined weight of all items in the stack 
    private ContainerSlot   m_slot;         // The slot containing this item stack

    // Constructor
    public ItemStack(ContainerSlot slot, int maxStackSize)
    {
        // Setup member variables, stack holds no items by default
        m_slot          = slot;
        m_stackItemsId  = "";
        m_stackSize     = 0;
        m_maxStackSize  = maxStackSize;
    }

    public bool CanAddItemToStack(string itemId)
    {
        if(m_stackSize < m_maxStackSize || m_maxStackSize == 0)
        {
            // The max overall stack size was not yet reached, or there is no limit

            if (m_stackSize > 0)
            {
                //The stack is not empty

                if (m_stackItemsId == itemId)
                {
                    Item item = ItemManager.Instance.GetItemWithId(itemId);

                    //This stack already contains some of item being added - check the max item-specific stack size was not already reached
                    if (m_stackSize < item.StackSize)
                    {
                        // The max stack size for this specific item type was not reached - item can be added
                        return true;
                    }
                    else
                    {
                        // The max stack size for this specific item type was reached - item cannot be added
                        return false;
                    }
                }
                else
                {
                    // This stack contains a different item type - item cannot be added
                    return false;
                }
            }
            else
            {
                //This stack is empty - item can definitely be added
                return true;
            }
        }
        else
        {
            //This stack has reached its maximum allowed size - item cannot be added
            return false;
        }
    }

    public bool AddItemToStack(string itemId, bool checkIfValid = true)
    {
        if ( !checkIfValid || (checkIfValid && CanAddItemToStack(itemId)) )
        {
            // The item can be safely added, or checkIfValid is false so checks were skipped

            Item item = ItemManager.Instance.GetItemWithId(itemId);

            if(item != null)
            {
                // Adding the item

                // Increment the stack size
                m_stackSize++;

                // Add to the overall stack weight
                m_stackWeight += item.Weight;

                // Set the stack items id in case this is the first item of this type to be added
                m_stackItemsId = itemId;

                if(m_slot.ParentContainer != null)
                {
                    // There is a parent container for the slot containing this stack,
                    //   notify it that the sate of its contents has changed
                    m_slot.ParentContainer.ContainerStateChanged();
                }

                // Item was added
                return true;
            }

            // Invalid item id, item not added
            return false;
        }

        // CanAddItemToStack returned false, item not added
        return false;
    }

    public bool TryRemoveItemFromStack()
    {
        if(m_stackSize > 0)
        {
            // Stack is not empty - there is something to remove

            Item item = ItemManager.Instance.GetItemWithId(m_stackItemsId);

            // Reduce the stack size by 1
            m_stackSize--;

            // Reduce stack weight
            m_stackWeight -= item.Weight;

            if (m_slot.ParentContainer != null)
            {
                // There is a parent container for the slot containing this stack,
                //   notify it that the sate of its contents has changed
                m_slot.ParentContainer.ContainerStateChanged();
            }

            // Item was removed
            return true;
        }

        // Nothing to remove
        return false;
    }
}