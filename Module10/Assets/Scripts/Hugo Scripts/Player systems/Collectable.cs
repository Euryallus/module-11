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
        foreach(ItemGroup group in collectedOnPickup)
        {
            for (int i = 0; i < group.Quantity; i++)
            {
                // Cycles each ItemGroup in the array collectionOnPickup and adds [x] quantity of said item to the player's inventory
                GameObject.FindGameObjectWithTag("Inventory").GetComponent<InventoryPanel>().TryAddItem(group.Item);
            }
        }

        // When all items are added, object destroys itself
        Destroy(gameObject);
    }
}
