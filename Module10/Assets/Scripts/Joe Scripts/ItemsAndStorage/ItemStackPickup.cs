using UnityEngine;
using TMPro;

// ||=======================================================================||
// || ItemStackPickup: A stack of items that sits on the ground and can     ||
// ||   be collected up by the player by walking into the pickup.           ||
// ||=======================================================================||
// || Used on prefab: Joe/Items/ItemStackPickup                             ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the production phase (Module 11).                                 ||
// ||=======================================================================||

public class ItemStackPickup : MonoBehaviour, IPersistentPlacedObject
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private SpriteRenderer itemSpriteRenderer; // Renders the sprite for the item type this stack holds
    [SerializeField] private GameObject     itemCountPanel;     // Parent of the text below
    [SerializeField] private TextMeshPro    itemCountText;      // Text displaying how many items are in the stack

    #endregion

    private string          itemId;             // Id of the item type in the stack
    private int             itemQuantity;       // Number of items in the stack
    private bool            playerInTrigger;    // Whether the player is currently in the trigger for picking up items
    private bool            canPickup;          // Whether the player is currently allowed to pick up items form this stack

    private void Start()
    {
        // Tell the WorldSave that this is an object that should be saved with the world
        WorldSave.Instance.AddPlacedObjectToSave(this);
    }

    private void OnDestroy()
    {
        // This object no longer exists in the world, remove it from the save list
        WorldSave.Instance.RemovePlacedObjectFromSaveList(this);

        // Unsubscribe from the ContainerStateChangedEvent to prevent null ref errors
        GameSceneUI.Instance.PlayerInventory.MainContainer.ContainerStateChangedEvent -= OnInventoryStateChanged;
    }

    public void Setup(ItemGroup itemGroup, bool allowInstantPickup = true)
    {
        itemId          = itemGroup.Item.Id;
        itemQuantity    = itemGroup.Quantity;

        // OnInventoryStateChanged will be called when the state of the player's inventory changes (e.g. an item is added/removed)
        GameSceneUI.Instance.PlayerInventory.MainContainer.ContainerStateChangedEvent += OnInventoryStateChanged;

        // Mark the player as already being in the trigger area if allowInstantPickup = false
        //  to prevent OnTriggerEnter being called which would cause items to be picked up
        playerInTrigger = !allowInstantPickup;

        // Allow the stack to be picked up straight away if allowInstantPickup is true
        canPickup = allowInstantPickup;

        // Display the sprite of the item that this pickup will give the player
        itemSpriteRenderer.sprite = itemGroup.Item.Sprite;

        // Update the visual count showing how many items are in the stack
        UpdateItemCountDisplay(itemGroup.Quantity);
    }

    public void Setup(string id, int quantity, InventoryPanel playerInventory)
    {
        // Setup function overload, takes an item id and quantity instead of an
        //  ItemGroup and uses these parameters to create a new ItemGroup

        Item item = ItemManager.Instance.GetItemWithId(id);

        Setup(new ItemGroup(item, quantity), playerInventory);
    }

    public void AddDataToWorldSave(SaveData saveData)
    {
        // Adds save data which contains the pickup's position, rotation, item id and
        //  item quantity so the item stack can be restored when the game is loaded

        saveData.AddData("itemStackPickup*", new ItemStackPickupSaveData()
        {
            Position = new float[3] { transform.position.x, transform.position.y, transform.position.z },
            Rotation = new float[3] { transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z },
            ItemId = itemId,
            ItemQuantity = itemQuantity
        });
    }

    private void UpdateItemCountDisplay(int itemCount)
    {
        if (itemCount > 1)
        {
            // Display item count text if there is more than one item in the stack
            itemCountText.text = itemCount.ToString();
        }
        else
        {
            // Hide item count text if there is only one item in the stack
            itemCountPanel.SetActive(false);
        }
    }

    private void OnInventoryStateChanged(bool loadingContainer)
    {
        // Try picking up items when inventory state changes in case there is
        //   now more space in the inventory than there was previously
        
        if(playerInTrigger && canPickup)
        {
            TryPickupItems();
        }
    }

    private void TryPickupItems()
    {
        bool anyItemAdded = false; // Keeps track of whether any items were picked up and added to the player's inventory

        Item item = ItemManager.Instance.GetItemWithId(itemId);
        int currentQuantity = itemQuantity;

        // Loop through the number of items in the stack
        for (int i = 0; i < currentQuantity; i++)
        {
            // Attempt to add an item to the players inventory/hotbar
            if (GameSceneUI.Instance.PlayerInventory.TryAddItem(item))
            {
                // The item was added successfully, reduce itemQuantity
                //   as there is now one less item in the stack

                anyItemAdded = true;
                itemQuantity--;
            }
        }

        if(anyItemAdded)
        {
            // At least one item was picked up/removed from the stack

            if (itemQuantity == 0)
            {
                // Destroy the pickup if there are no items left
                Destroy(gameObject);
            }
            else
            {
                // Update the item count if there is at least one item still in the
                //   stack to show the player how many are left to be picked up
                UpdateItemCountDisplay(itemQuantity);
            }

            // Play a pickup sound
            AudioManager.Instance.PlaySoundEffect2D("pop");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!playerInTrigger && other.CompareTag("Player"))
        {
            // Allow pickups the first time the player enters the trigger, which may be
            //   as soon as the item is dropped or after leaving and re-entering depending on
            //   in allowInstantPickup was true when setting up

            canPickup       = true;
            playerInTrigger = true;

            TryPickupItems();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // The player left the trigger
            playerInTrigger = false;
        }
    }
}

// ItemStackPickupSaveData contains data that will be
//   serialised when an ItemStackPickup is saved

[System.Serializable]
public class ItemStackPickupSaveData : TransformSaveData
{
    public string   ItemId;
    public int      ItemQuantity;
}