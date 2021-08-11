using UnityEngine;

// ||=======================================================================||
// || LaunchAbility: A child class of PlayerAbility that handles the launch ||
// ||    ability specifically.                                              ||
// ||=======================================================================||
// || Used on prefab: Hugo/Player                                           ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the production phase (Module 11).                                 ||
// ||=======================================================================||

public class LaunchAbility : PlayerAbility
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [Header("Launch Ability")]
    [SerializeField] private GameObject prefabLaunchIndicator;  // GameObject instantiated when using the launch ability to visually indicate the launch charge

    #endregion

    private GameObject      launchIndicator;    // Indicator displayed below the player when using launch ability

    protected override void Update()
    {
        base.Update();

        if(charging)
        {
            if (launchIndicator != null)
            {
                // Reduce the scale of the launch indicator over time while charging the ability
                launchIndicator.transform.localScale -= new Vector3(Time.deltaTime / chargeTime, 0.0f,
                                                                    Time.deltaTime / chargeTime);
            }
        }
    }

    protected override void ChargeStart()
    {
        base.ChargeStart();

        // Spawn the indicator showing how close the player is to launching
        launchIndicator = Instantiate(prefabLaunchIndicator, transform);
    }

    protected override void ChargeEnd()
    {
        base.ChargeEnd();

        // No longer charging launch, destroy indicator
        if(launchIndicator != null)
        {
            Destroy(launchIndicator);
        }
    }

    protected override void AbilityStart()
    {
        // Stop showing the launch indicator if one already exists
        if (launchIndicator != null)
        {
            Destroy(launchIndicator);
        }

        PlayerMovement playerMovement = PlayerInstance.ActivePlayer.PlayerMovement;

        if (playerMovement.PlayerIsGrounded())
        {
            // The player is stood on ground, start the ability

            base.AbilityStart();

            // Change the launch force to be used based on the property (that can be upgraded) on the launch ability item
            float launchForce = abilityItem.GetCustomFloatPropertyWithName("launchPower").Value;

            // Launch the player into the air with the force determined above
            playerMovement.SetJumpVelocity(launchForce);
        }
    }

    protected override void FindUIIndicator()
    {
        // Finds the UI indicator for the launch ability
        uiIndicator = GameObject.FindGameObjectWithTag("LaunchIndicator").GetComponent<AbilityIndicator>();
    }

    protected override PlayerAbilityType GetAbilityType()
    {
        return PlayerAbilityType.Launch;
    }
}