using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Used when a game object is destructable in exchange for resources
// Development window:  Prototype phase
// Inherits from:       MonoBehaviour

public class DestructableObject : MonoBehaviour
{
    [Header("Items associated with object")]
    [SerializeField]    private ItemGroup[] itemDroppedOnDestroy;   // Array of items that are given to player when object is destroyed using tools
                        public Item[] toolToBreak;                  // Array of tools that can be used to destroy object

    [Header("Object health")] [SerializeField]
                        protected int hitsToBreak = 3;              // Number of hits object needs to be destroyed

    [Header("Particles")]
    [SerializeField]    private GameObject destroyParticlesPrefab;      //Prefab containing particles to be spawned when the piece is hit
    [SerializeField]    private Transform destroyParticlesTransform;    // Position particles should spawn at (if any are defined)

    [Header("Hit Sound")]
    [SerializeField]    private SoundClass hitSound;    // Sound played when the object is hit
                        protected int health;           // Current health of the object
                        bool destroyed = false;         // Indicates if object has been Destroyed (use for save features)

    protected virtual void Start()
    {
        // Sets health to default when game loads
        health = hitsToBreak;
    }

    public virtual void TakeHit() 
    {
        // Reduces "health" of resource object
        --health;

        // Plays sound if one is defined
        if(hitSound != null)
        {
            AudioManager.Instance.PlaySoundEffect3D(hitSound, transform.position);
        }

        // If health reaches 0, runs Destroyed()
        if(health <= 0)
        {
            Destroyed();
        }
    }

    public virtual void Destroyed()
    {
        // Adds item dropped to inventory
        InventoryPanel inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<InventoryPanel>();
        foreach(ItemGroup stack in itemDroppedOnDestroy)
        {
            for (int i = 0; i < stack.Quantity; i++)
            {
                inventory.AddItemToInventory(stack.Item);
                // Flagged destroyed as true
                destroyed = true;
            }
        }

        // If particles have been defined, instantiate them at position desired (e.g. splinters, leaves etc.)
        if(destroyParticlesPrefab != null)
        {
            ParticleGroup particleGroup = Instantiate(destroyParticlesPrefab, destroyParticlesTransform.position, Quaternion.identity).GetComponent<ParticleGroup>();
            particleGroup.PlayEffect();
        }
    }
}
