using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Used to allow the player to "collect" items in-game and add them to their inventory
// Development window:  Prototype phase
// Inherits from:       MonoBehaviour

public class Collectable : InteractableWithOutline
{
    [SerializeField] private List<ItemGroup> collectedOnPickup = new List<ItemGroup>(); // List of items & quantities the player recieves when they interact with the object

    // Called using PlayerInteractions
    public override void Interact()
    {
        base.Interact();

        InventoryPanel inventory = GameSceneUI.Instance.PlayerInventory;

        // Added by Joe: Keeps track of any item groups that cannot be added to the player's inventory and should instead be dropped
        List<ItemGroup> dropItemGroups = new List<ItemGroup>();

        foreach (ItemGroup group in collectedOnPickup)
        {
            ItemGroup dropItemGroup = new ItemGroup(group.Item, 0);

            for (int i = 0; i < group.Quantity; i++)
            {
                if (!inventory.TryAddItem(group.Item))
                {
                    dropItemGroup.Quantity++;
                }
            }

            dropItemGroups.Add(dropItemGroup);
        }

        if (dropItemGroups.Count > 0)
        {
            // Drop any item groups that couldn't be added to the player's inventory
            inventory.DropItemGroups(dropItemGroups, true, true);
        }

        // When all items are added/dropped, object destroys itself
        Destroy(gameObject);
    }
}
