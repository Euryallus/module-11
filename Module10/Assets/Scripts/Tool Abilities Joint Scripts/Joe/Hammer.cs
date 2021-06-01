using UnityEngine;

// ||=======================================================================||
// || Hammer: Held tool used by the player to collect certain resources and ||
// ||    launch themselves into the air.                                    ||
// ||=======================================================================||
// || Used on all prefabs in: HeldItems/Hammer                              ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

[RequireComponent(typeof(Animator))]
public class Hammer : HeldTool
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [Header("Hammer")]
    [SerializeField] private SoundClass launchSound;            // Sound player when the launch ability is used
    [SerializeField] private GameObject prefabLaunchIndicator;  // GameObject instantiated when using the launch ability to visually indicate the launch timer value

    #endregion

    private PlayerMovement  playerMovement;     // Reference to the player movement script
    private GameObject      launchIndicator;    // Indicator when using launch ability
    private float           launchTimer;        // How long the player has been charging the jump ability
    private bool            launched;           // Whether the player has launched into the air while using secondary ability
    private Animator        animator;           // Animator for tool swing animation

    private const float launchDelay = 0.5f;             // How long the player has to hold down the mouse button before launching
    private const float indicatorShrinkSpeed = 1.8f;    // How quickly the launch indicator reduces in size while charging

    protected override void Awake()
    {
        base.Awake();

        // Find and get references to animator/player movement
        animator        = GetComponent<Animator>();
        playerMovement  = playerTransform.GetComponent<PlayerMovement>();
    }

    protected override void HitDestructibleObject(DestructableObject destructible, RaycastHit raycastHit)
    {
        base.HitDestructibleObject(destructible, raycastHit);

        // Show the swing animation when something is hit by the hammer
        animator.SetTrigger("Swing");
    }

    public override void StartSecondardAbility()
    {
        if (playerMovement.PlayerIsGrounded())
        {
            // Player is on the ground, start charging launch ability

            base.StartSecondardAbility();

            launchTimer = 0.0f;

            // Spawn the indicator showing how close the player is to launching
            launchIndicator = Instantiate(prefabLaunchIndicator, playerTransform);
        }
    }

    public override void EndSecondaryAbility()
    {
        base.EndSecondaryAbility();

        if(launchIndicator != null)
        {
            // No longer charging launch, destroy indicator
            Destroy(launchIndicator);
        }

        // Reset launched bool for next time the ability is used
        launched = false;
    }

    private void Update()
    {
        if (performingSecondaryAbility)
        {
            // Increment the lauinch timer
            launchTimer += Time.deltaTime;

            if(launchTimer >= launchDelay && !launched)
            {
                // Launch delay was reached and the player didn't launch yet, launch them into the air
                LaunchPlayer();
            }
            else if(launchIndicator != null)
            {
                // Reduce the scale of the launch indicator while charging the ability
                launchIndicator.transform.localScale -= new Vector3(Time.deltaTime * indicatorShrinkSpeed, 0.0f,
                                                                    Time.deltaTime * indicatorShrinkSpeed);
            }
        }
    }

    private void LaunchPlayer()
    {
        // Reduce player food level when the launch ability is used
        playerStats.DecreaseFoodLevel(secondaryAbilityHunger);

        // Get the launch force value of the hammer item being used
        float launchForce = item.GetCustomFloatPropertyWithName("LaunchForce").Value;

        // Launch the player into the air
        playerMovement.SetJumpVelocity(launchForce);

        // Stop showing launch indicator
        if (launchIndicator != null)
        {
            Destroy(launchIndicator);
        }

        // Play a launch sound if one was set
        if (launchSound != null)
        {
            AudioManager.Instance.PlaySoundEffect2D(launchSound);
        }

        // Player has been launched
        launched = true;
    }
}
