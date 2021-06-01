using UnityEngine;

// ||=======================================================================||
// || HeldTool: Base class for all tools the player can hold and trigger    ||
// ||   abilities on.                                                       ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public class HeldTool : HeldItem
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    // See tooltips for comments

    [Header("Food level reduction when item is used (for tools):")]
    [Header("Held Tool")]

    [SerializeField] [Tooltip("How much the player's food level decreases when the item's main ability is used")]
    protected float mainAbilityHunger;

    [SerializeField] [Tooltip("How much the player's food level decreases when the item's secondary ability is used")]
    protected float secondaryAbilityHunger;

    [SerializeField] [Tooltip("The sound that is played when the tool's primary ability is used")]
    protected SoundClass useToolSound;

    #endregion

    protected PlayerStats   playerStats;        // Reference to the player stats script
    protected CameraShake   playerCameraShake;  // Reference to the player's camera shake script
    protected RaycastHit    toolRaycastHit;     // Info about what was hit by a raycast sent out from the player's camera

    public MeshRenderer toolRenderer;

    protected override void Awake()
    {
        base.Awake();

        // Find and get references to player stats/camera shake scripts
        playerStats =       playerTransform.GetComponent<PlayerStats>();
        playerCameraShake = playerTransform.GetComponent<CameraShake>();
    }

    public override void PerformMainAbility()
    {
        base.PerformMainAbility();

        //Check that the player cam isn't null, this can occur in certain cases when an alternate camera is being used (e.g. talking to an NPC)
        if (playerCameraTransform != null)
        {
            if (Physics.Raycast(playerCameraTransform.position, playerCameraTransform.forward, out toolRaycastHit, 4.0f))
            {
                // The raycast hit something

                // Attempt to get a DestructableObject script component from the hit GameObject
                DestructableObject destructable = toolRaycastHit.transform.gameObject.GetComponent<DestructableObject>();

                if (destructable != null)
                {
                    // The hit GameObject was a descrictible object

                    // Loop through all tools that can be used to break the hit destructible
                    foreach (Item tool in destructable.toolToBreak)
                    {
                        // Get the base id of the item being held (so customised items can still be used for their original purpose)
                        string compareId = item.CustomItem ? item.BaseItemId : item.Id;

                        if (tool.Id == compareId)
                        {
                            // This is a valid tool for breaking the destructible, hit it
                            HitDestructibleObject(destructable, toolRaycastHit);

                            // No need to continue checking the toolToBreak array
                            break;
                        }
                    }
                }
            }
        }
    }

    protected virtual void HitDestructibleObject(DestructableObject destructible, RaycastHit raycastHit)
    {
        // Reduce the player's food level based on the value set in the inspector, and hit the destructible object

        playerStats.DecreaseFoodLevel(mainAbilityHunger);

        destructible.TakeHit();
    }
}
