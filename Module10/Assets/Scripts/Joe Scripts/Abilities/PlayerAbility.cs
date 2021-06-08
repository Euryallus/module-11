using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerAbility : MonoBehaviour
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [Header("Player Ability")]

    [SerializeField] protected KeyCode  triggerKey;

    [Header("Charge & Cooldown")]
    [SerializeField] protected float    chargeTime = 0.5f;
    [SerializeField] protected float    cooldownTime = 5.0f;

    [Header("Food Level Decrease")]
    [SerializeField] protected float    initialFoodDecrease;
    [SerializeField] protected float    continuousFoodDecrease;

    [Header("Sounds")]
    [SerializeField] private SoundClass chargeSound;
    [SerializeField] private SoundClass chargeEndSound;
    [SerializeField] private SoundClass abilityTriggerSound;

    #endregion

    #region Properties

    #endregion

    protected AbilityIndicator uiIndicator;
    protected float charge;
    protected float cooldown;
    protected bool charging;
    private PlayerStats playerStats;    // Reference to the player stats script

    protected virtual void Start()
    {
        playerStats = GetComponent<PlayerStats>();

        FindUIIndicator();

        if(uiIndicator == null)
        {
            Debug.LogError("Failed to find UI ability indicator", gameObject);
        }

        SetChargeAmount(0.0f);
        SetCooldownAmount(1.0f);
    }

    protected virtual void Update()
    {
        if(cooldown >= 1.0f)
        {
            if (Input.GetKey(triggerKey))
            {
                if (!charging)
                {
                    ChargeStart();
                }

                if (charge < 1.0f)
                {
                    SetChargeAmount(charge + Time.deltaTime / chargeTime);
                }
                else
                {
                    ChargeEnd();

                    SetCooldownAmount(0.0f);

                    if (abilityTriggerSound != null)
                    {
                        AudioManager.Instance.PlaySoundEffect2D(abilityTriggerSound);
                    }

                    AbilityStart();
                }
            }
            else
            {
                if (charging)
                {
                    ChargeEnd();

                    if (chargeEndSound != null)
                    {
                        AudioManager.Instance.PlaySoundEffect2D(chargeEndSound);
                    }
                }
            }
        }
        else
        {
            SetCooldownAmount(cooldown + Time.deltaTime / cooldownTime);
        }
    }

    protected virtual void ChargeStart()
    {
        charging = true;

        StartChargeSound();
    }

    protected virtual void ChargeEnd()
    {
        charging = false;

        SetChargeAmount(0.0f);

        StopChargeSound();
    }

    protected virtual void AbilityStart()
    {
        // Reduce player food level when the launch ability is used
        playerStats.DecreaseFoodLevel(initialFoodDecrease);
    }

    private void StartChargeSound()
    {
        if (chargeSound != null)
        {
            AudioManager.Instance.PlayLoopingSoundEffect(chargeSound, "charge_" + uiIndicator.name);
        }
    }

    private void StopChargeSound()
    {
        if (chargeSound != null)
        {
            AudioManager.Instance.StopLoopingSoundEffect("charge_" + uiIndicator.name);
        }
    }

    private void SetChargeAmount(float chargeAmount)
    {
        charge = chargeAmount;
        uiIndicator.SetChargeAmount(chargeAmount);
    }

    private void SetCooldownAmount(float cooldownAmount)
    {
        cooldown = cooldownAmount;
        uiIndicator.SetCooldownAmount(cooldownAmount);
    }

    protected abstract void FindUIIndicator();
}