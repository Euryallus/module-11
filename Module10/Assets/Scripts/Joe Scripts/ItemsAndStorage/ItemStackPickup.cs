using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Module 11

public class ItemStackPickup : MonoBehaviour, IPersistentPlacedObject
{
    [SerializeField] private SpriteRenderer itemSpriteRenderer;
    [SerializeField] private GameObject     itemCountPanel;
    [SerializeField] private TextMeshPro    itemCountText;

    private string          itemId;
    private int             itemQuantity;
    private bool            playerInTrigger;
    private bool            canPickup;
    private InventoryPanel  inventory;

    private void Start()
    {
        // Tell the WorldSave that this is an object that should be saved with the world
        WorldSave.Instance.AddPlacedObjectToSave(this);
    }

    private void OnDestroy()
    {
        // This object no longer exists in the world, remove it from the save list
        WorldSave.Instance.RemovePlacedObjectFromSaveList(this);

        // Unsibscribe from the ContainerStateChangedEvent to prevent null ref errors
        inventory.ItemContainer.ContainerStateChangedEvent -= OnInventoryStateChanged;
    }

    public void Setup(ItemGroup itemGroup, InventoryPanel playerInventory, bool allowInstantPickup = true)
    {
        itemId          = itemGroup.Item.Id;
        itemQuantity    = itemGroup.Quantity;

        inventory       = playerInventory;
        inventory.ItemContainer.ContainerStateChangedEvent += OnInventoryStateChanged;

        playerInTrigger = !allowInstantPickup;
        canPickup       = allowInstantPickup;

        itemSpriteRenderer.sprite = itemGroup.Item.Sprite;

        UpdateItemCountDisplay(itemGroup.Quantity);
    }

    private void OnInventoryStateChanged()
    {
        // Try picking up items when inventory state changes in case there is
        //   now more space in the inventory than there was previously
        if(playerInTrigger && canPickup)
        {
            TryPickupItems();
        }
    }

    private void UpdateItemCountDisplay(int itemCount)
    {
        if (itemCount > 1)
        {
            itemCountText.text = itemCount.ToString();
        }
        else
        {
            itemCountPanel.SetActive(false);
        }
    }

    public void Setup(string id, int quantity, InventoryPanel playerInventory)
    {
        Item item = ItemManager.Instance.GetItemWithId(id);

        Setup(new ItemGroup(item, quantity), playerInventory);
    }

    public void AddDataToWorldSave(SaveData saveData)
    {
        saveData.AddData("itemStackPickup*",    new ItemStackPickupSaveData()
                                                {
                                                    Position     = new float[3] { transform.position.x, transform.position.y, transform.position.z },
                                                    Rotation     = new float[3] { transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z },
                                                    ItemId       = itemId,
                                                    ItemQuantity = itemQuantity
                                                });
    }

    private void TryPickupItems()
    {
        bool anyItemAdded = false;

        Item item = ItemManager.Instance.GetItemWithId(itemId);

        int currentQuantity = itemQuantity;

        for (int i = 0; i < currentQuantity; i++)
        {
            if (inventory.TryAddItem(item))
            {
                anyItemAdded = true;
                itemQuantity--;
            }
        }

        if(anyItemAdded)
        {
            if (itemQuantity == 0)
            {
                Destroy(gameObject);
            }
            else
            {
                UpdateItemCountDisplay(itemQuantity);
            }

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
            playerInTrigger = false;
        }
    }
}

[System.Serializable]
public class ItemStackPickupSaveData : TransformSaveData
{
    public string   ItemId;
    public int      ItemQuantity;
}