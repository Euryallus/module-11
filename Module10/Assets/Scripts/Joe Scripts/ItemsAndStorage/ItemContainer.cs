using System;
using System.Collections.Generic;
using UnityEngine;

// ||=======================================================================||
// || ItemContainer: Contains a collection of item slots and saves/loads    ||
// ||   the contents of those slots, also provides useful functions to      ||
// ||   check if a certain item/how much of an item is in the container.    ||
// ||=======================================================================||
// || Used on prefabs: Joe/UI/Inventory/IC_PlayerInventory                  ||
// ||                  Joe/Environment/Crafting & Chests/Chest              ||
// ||                  Joe/Environment/Crafting & Chests/Loot Chest         ||
// ||                  Joe/Environment/Crafting & Chests/IC_LinkedChest     ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

// Updated for mod11: can now be saved/loaded globally or as a scene object

public class ItemContainer : MonoBehaviour, IPersistentSceneObject, IPersistentGlobalObject
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [Header("Important: Set unique id")]
    [Header("Item Container")]

    public string                               ContainerId;        // Unique id used when saving/loading the contents of this container
    [SerializeField] private int                numberOfSlots;      // The number of item slots in this container
    [SerializeField] private bool               globalSave;         // Whether this container should use global save/load functions

    #endregion

    #region Properties

    public ItemInfoPopup ItemInfoPopup  { get { return itemInfoPopup; } }
    public ContainerSlot[]  Slots       { get { return slots; } }

    #endregion

    public event Action<bool>     ContainerStateChangedEvent;   //Event that is invoked when the container state changes (i.e. items are added/removed/moved),
                                                                //   with a bool parameter for whether the event is called while loading

    private ContainerSlot[] slots;                      // All slots in the container that can hold items
    private bool            containerStateChanged;      // Set to true each time an action occurs that changes the item container's state
    private ItemInfoPopup   itemInfoPopup;              // Popup for showing info when items in the container's slots are hovered over
    private bool loading;

    private void Awake()
    {
        // Initialise empty container slots by default
        slots = new ContainerSlot[numberOfSlots];

        for (int i = 0; i < numberOfSlots; i++)
        {
            slots[i] = new ContainerSlot(0, this);
        }

        // Fine the item info popup in the scene and get a reference to its script
        itemInfoPopup = GameObject.FindGameObjectWithTag("ItemInfoPopup").GetComponent<ItemInfoPopup>();
    }

    private void Start()
    {
        if (string.IsNullOrEmpty(ContainerId))
        {
            // Warning if no id is set
            Debug.LogError("IMPORTANT: ItemContainer exists without id. All item containers require a *unique* id for saving/loading data. Click this message to view the problematic GameObject.", gameObject);
        }

        // Subscribe to save/load events so the container's data will be saved/loaded with the game
        if(globalSave)
        {
            SaveLoadManager.Instance.SubscribeGlobalSaveLoadEvents(OnGlobalSave, OnGlobalLoadSetup, OnGlobalLoadConfigure);
        }
        else
        {
            SaveLoadManager.Instance.SubscribeSceneSaveLoadEvents(OnSceneSave, OnSceneLoadSetup, OnSceneLoadConfigure);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from save/load events if the container is destroyed to prevent null reference errors
        if(globalSave)
        {
            SaveLoadManager.Instance.UnsubscribeGlobalSaveLoadEvents(OnGlobalSave, OnGlobalLoadSetup, OnGlobalLoadConfigure);
        }
        else
        {
            SaveLoadManager.Instance.UnsubscribeSceneSaveLoadEvents(OnSceneSave, OnSceneLoadSetup, OnSceneLoadConfigure);
        }
    }

    private void Update()
    {
        if (containerStateChanged)
        {
            // The container state was changed one or more times in the last frame
            ContainerStateChangedThisFrame(loading);
            containerStateChanged = false;
        }

        if(loading)
        {
            loading = false;
        }
    }

    public void OnSceneSave(SaveData saveData)
    {
        OnSave(saveData);
    }

    public void OnGlobalSave(SaveData saveData)
    {
        OnSave(saveData);
    }

    private void OnSave(SaveData saveData)
    {
        Debug.Log("Saving item container data for " + ContainerId);

        for (int i = 0; i < slots.Length; i++)
        {
            // Save data for each slot in this container (its stack size/item type)
            saveData.AddData(ContainerId + "_slotStackSize" + i, slots[i].ItemStack.StackSize);
            saveData.AddData(ContainerId + "_stackItemsId" + i, slots[i].ItemStack.StackItemsID);
        }
    }

    public void OnSceneLoadSetup(SaveData saveData)
    {
        // Loading for ItemContainer occurs in the OnLoadConfigure function since it
        //   depends on data that is initialised by other objects in the OnLoadSetup function
    }

    public void OnGlobalLoadSetup(SaveData saveData) { }

    public void OnSceneLoadConfigure(SaveData saveData)
    {
        OnLoadConfigure(saveData);
    }

    public void OnGlobalLoadConfigure(SaveData saveData)
    {
        OnLoadConfigure(saveData);
    }

    private void OnLoadConfigure(SaveData saveData)
    {
        loading = true;

        Debug.Log("Loading item container data for " + ContainerId);

        for (int i = 0; i < slots.Length; i++)
        {
            // Load data for each container slot - (stack size and item type)
            int stackSize = saveData.GetData<int>(ContainerId + "_slotStackSize" + i);
            string itemId = saveData.GetData<string>(ContainerId + "_stackItemsId" + i);

            // Add items based on the loaded values
            for (int j = 0; j < stackSize; j++)
            {
                slots[i].ItemStack.AddItemToStack(itemId, false, false);
            }

            // If slots are linked to UI, update it for each one to reflect changes
            if (slots[i].SlotUI != null)
            {
                slots[i].SlotUI.UpdateUI();
            }
        }
    }

    public void LinkSlotsToUI(List<ContainerSlotUI> slotUIList)
    {
        // Links all related slot UI elements to the slot objects in this container
        for (int i = 0; i < numberOfSlots; i++)
        {
            slotUIList[i].LinkToContainerSlot(slots[i]);
        }
    }

    public void ContainerStateChanged()
    {
        // Marks the container state as changed, called when items are added/removed from a slot in this container
        containerStateChanged = true;
    }

    private void ContainerStateChangedThisFrame(bool loading)
    {
        // At least one change was made to the state of the container in the last frame, invoke ContainerStateChangedEvent
        ContainerStateChangedEvent?.Invoke(loading);
    }

    public bool CanAddItemToContainer(Item item, out int firstValidSlotIndex)
    {
        // Returns true/false depending on if an item can be added to the container
        //   (Note: Does not add the item, just checks if there is enough space TO add it)

        // Step 1 - loop through all slots to find valid ones

        FindValidContainerSlots(item, out int firstEmptySlot, out int firstStackableSlot);

        // Step 2: If a slot was found, add the item to it in this priority: stackable slot > empty slot
        //   Note: Stackable slots are slots that already contain a stack with items of the same type (but can hold at least one more), empty slots are fully empty

        if (firstStackableSlot == -1 && firstEmptySlot == -1)
        {
            // No empty or stackable slots, meaning the inventory is full and the item cannot be added
            firstValidSlotIndex = -1;
            return false;
        }
        else
        {
            int chosenSlotIndex; // The index of the chosen slot to add items to

            if (firstStackableSlot != -1)
            {
                // A stackable slot was found, set it as the chosen slot
                chosenSlotIndex = firstStackableSlot;
            }
            else
            {
                // No stackable slots but an empty slot was found, set it as the chosen slot
                chosenSlotIndex = firstEmptySlot;
            }

            // Item can be added to the chosen slot
            firstValidSlotIndex = chosenSlotIndex;

            return true;
        }
    }

    public bool TryAddItemToContainer(Item item)
    {
        // Check if the item can be added to the container, and which slot it should be added to
        bool canAdd = CanAddItemToContainer(item, out int firstValidSlotIndex);

        if(canAdd)
        {
            // The item can be added - add it to the valid slot that was found
            slots[firstValidSlotIndex].ItemStack.AddItemToStack(item.Id);

            // Update slot UI to show new item
            slots[firstValidSlotIndex].SlotUI.UpdateUI();
        }

        return canAdd;
    }

    public bool TryRemoveItemFromContainer(string itemId)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            // Find a slot that is not empty and contains item with the given id
            if(slots[i].ItemStack.StackSize > 0 && slots[i].ItemStack.StackItemsID == itemId)
            {
                // Remove an item
                slots[i].ItemStack.TryRemoveItemFromStack();

                // Update UI to show changes
                slots[i].SlotUI.UpdateUI();

                // Item was removed successfully
                return true;
            }
        }

        //No matching items in the inventory - item was not removed
        return false;
    }

    private void FindValidContainerSlots(Item item, out int firstEmptySlot, out int firstStackableSlot)
    {
        firstEmptySlot = -1;      //Keeps track of the index of the first empty slot that is found
        firstStackableSlot = -1;  //Keeps track of the index of the first slot where the item can stack that is found

        for (int i = 0; i < slots.Length; i++)
        {
            // Check if the current stack can take the item
            if (slots[i].ItemStack.CanAddItemToStack(item.Id))
            {
                if (slots[i].ItemStack.StackSize == 0 && firstEmptySlot == -1)
                {
                    // The first empty slot was found
                    firstEmptySlot = i;
                }
                else if (slots[i].ItemStack.StackSize > 0 && firstStackableSlot == -1)
                {
                    // The first stackable slot was found - no more searching is needed as stackable slots take priority
                    firstStackableSlot = i;
                    return;
                }
            }
        }
    }

    public bool ContainsQuantityOfItem(ItemGroup itemGroup, out List<ContainerSlot> containingSlots)
    {
        // Checks if the container has a set amount of a certain item type, and returns
        //   a bool depending on if it does, also returns a list of slots that contain the item type if any are found

        int numberOfItemType = 0;   // Keeps track of the number of the given item type found

        containingSlots = new List<ContainerSlot>();

        // Loop through all slots in the container
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].ItemStack.StackSize > 0 && slots[i].ItemStack.StackItemsID == itemGroup.Item.Id)
            {
                // Found a slot with the correct item type and at least one item

                // Add the number of items in the slot's stack to the counter
                numberOfItemType += slots[i].ItemStack.StackSize;

                // Add the slot to the list of slots containing the item
                containingSlots.Add(slots[i]);
            }

            if (numberOfItemType >= itemGroup.Quantity)
            {
                // The container does have the given quantity/item type
                return true;
            }
        }

        // The container does not have the given quantity/item type
        return false;
    }

    public int CheckForQuantityOfItem(Item item)
    {
        // Returns the quantity of a given item type that is stored in the container

        int numberOfItemType = 0;   // Keeps track of the number of the given item type

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].ItemStack.StackSize > 0 && slots[i].ItemStack.StackItemsID == item.Id)
            {
                // Found a slot with the correct item type and at least one item

                // Add the number of items in the slot's stack to the counter
                numberOfItemType += slots[i].ItemStack.StackSize;
            }
        }

        // Return the quantity of the given item type that was found
        return numberOfItemType;
    }
}
