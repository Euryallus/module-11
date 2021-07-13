using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchAbility : PlayerAbility
{
    [Header("Launch Ability")]
    [SerializeField] private GameObject prefabLaunchIndicator;  // GameObject instantiated when using the launch ability to visually indicate the launch charge

    private PlayerMovement  playerMovement;     // Reference to the player movement script
    private GameObject      launchIndicator;    // Indicator when using launch ability

    protected override void Start()
    {
        base.Start();

        playerMovement  = GetComponent<PlayerMovement>();
    }

    protected override void Update()
    {
        base.Update();

        if(charging)
        {
            if (launchIndicator != null)
            {
                // Reduce the scale of the launch indicator while charging the ability
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
        // Stop showing launch indicator
        if (launchIndicator != null)
        {
            Destroy(launchIndicator);
        }

        if (playerMovement.PlayerIsGrounded())
        {
            base.AbilityStart();

            float launchForce = abilityItem.GetCustomFloatPropertyWithName("launchPower").Value;

            // Launch the player into the air
            playerMovement.SetJumpVelocity(launchForce);
        }
    }

    protected override void FindUIIndicator()
    {
        uiIndicator = GameObject.FindGameObjectWithTag("LaunchIndicator").GetComponent<AbilityIndicator>();
    }

    protected override PlayerAbilityType GetAbilityType()
    {
        return PlayerAbilityType.Launch;
    }
}