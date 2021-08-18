using UnityEngine;

// ||=======================================================================||
// || FireMonumentInteraction: Allows the player to interact with the top   ||
// ||    part of a fire monument, causing the monument to be lit.           ||
// ||=======================================================================||
// || Used on prefab: Joe/Environment/FireMonument                          ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the production phase (Module 11).                                 ||
// ||=======================================================================||

public class FireMonumentInteraction : InteractableWithOutline
{
    [Header("Fire Monument")]
    [SerializeField] private FireMonument mainScipt; // Reference to the main FireMonument

    public override void Interact()
    {
        // Try to interact with the fire monument (returns true if the interaction was successful)
        if(mainScipt.OnInteract())
        {
            // Call base interaction code if the monument could be interacted with
            base.Interact();
        }
    }
}