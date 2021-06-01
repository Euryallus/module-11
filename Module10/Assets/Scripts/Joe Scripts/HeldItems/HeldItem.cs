using UnityEngine;

// ||=======================================================================||
// || HeldItem: A GameObject that is attached to the player when a certain  ||
// ||   item is selected in their hotbar. e.g. a sword that can be held/    ||
// ||   used to attack while the sword item is selected.                    ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public class HeldItem : MonoBehaviour
{
    #region Properties

    public bool PerformingSecondaryAbility { get { return performingSecondaryAbility; } }

    #endregion

    protected   Item            item;                       // The item related to the held GameObject
    protected   ContainerSlotUI containerSlot;              // The slot containing the above item
    protected   bool            performingSecondaryAbility; // Whether the player is using their secondary ability
    protected   Transform       playerTransform;            // Transform of the player GameObject
    protected   Transform       playerCameraTransform;      // Transform of the main player camera for raycasts

    protected virtual void Awake()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        playerCameraTransform = GameObject.FindGameObjectWithTag("MainCamera").transform;
    }

    public virtual void Setup(Item item, ContainerSlotUI containerSlot)
    {
        this.item           = item;
        this.containerSlot  = containerSlot;
    }

    public virtual void PerformMainAbility()
    {
        // For example, break something
    }

    public virtual void StartSecondardAbility()
    {
        Debug.Log("Starting secondary ability");
        performingSecondaryAbility = true;

        // For example, pick up and start moving an object
    }

    public virtual void EndSecondaryAbility()
    {
        Debug.Log("Ending secondary ability");
        performingSecondaryAbility = false;

        // For example, drop an object
    }

    private void OnDestroy()
    {
        // If the player is performing an ability when this held item is destroyed,
        //   make sure the puzzle ability behaviour is stopped
        if (performingSecondaryAbility)
        {
            EndSecondaryAbility();
        }
    }
}