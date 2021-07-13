using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerAbilityType
{
    Launch,
    Freeze,
    Slam,
    Grab
}

public abstract class PlayerAbility : MonoBehaviour
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [Header("Player Ability")]

    [SerializeField] protected KeyCode  triggerKey;
    [SerializeField] protected bool     useDefaultBehaviour = true;

    [Header("Upgrades")]
    [SerializeField] [Tooltip("The number of upgrade levels the ability has. Set to 0 if the ability cannot be upgraded")]
    protected int      maxUpgradeLevel;

    [Header("Charge & Cooldown")]
    [SerializeField] protected float    chargeTime   = 0.5f;
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

    public int MaxUpgradeLevel { get { return maxUpgradeLevel; } }

    #endregion

    protected   AbilityIndicator    uiIndicator;
    protected   float               charge;
    protected   float               cooldown;
    protected   bool                charging;
    protected   bool                abilityActive;
    protected   Item                abilityItem;

    private     PlayerStats         playerStats;    // Reference to the player stats script
    private     InventoryPanel      playerInventory;
    private     bool                unlocked;

    private static Dictionary<PlayerAbilityType, int> abilityUpgradeLevels = new Dictionary<PlayerAbilityType, int>()
    {
        { PlayerAbilityType.Launch, 0 },
        { PlayerAbilityType.Freeze, 0 },
        { PlayerAbilityType.Slam,   0 },
        { PlayerAbilityType.Grab,   0 }
    };

    private static Dictionary<PlayerAbilityType, bool> abilityUnlockStatuses = new Dictionary<PlayerAbilityType, bool>()
    {
        { PlayerAbilityType.Launch, false },
        { PlayerAbilityType.Freeze, false },
        { PlayerAbilityType.Slam,   false },
        { PlayerAbilityType.Grab,   false }
    };

    public static bool AbilityIsUnlocked(PlayerAbilityType abilityType)
    {
        return abilityUnlockStatuses[abilityType];
    }

    public static int GetAbilityUpgradeLevel(PlayerAbilityType abilityType)
    {
        return abilityUpgradeLevels[abilityType];
    }

    protected virtual void Start()
    {
        playerStats = GetComponent<PlayerStats>();

        charge = 0.0f;
        cooldown = 1.0f;

        FindUIIndicator();

        if(uiIndicator == null)
        {
            Debug.LogError("Failed to find UI ability indicator", gameObject);
        }

        SetupUIIndicator();
        LinkToInventory();

        UpdateUnlockStatus();
    }

    private void OnAbilityContainerStateChanged()
    {
        UpdateUnlockStatus();
    }

    protected virtual void Update()
    {
        if (uiIndicator == null)
        {
            // Re-find and setup the UI indicator if it becomes null (happens when switching scenes)
            FindUIIndicator();
            SetupUIIndicator();
        }

        if (playerInventory == null)
        {
            LinkToInventory();
        }

        if (unlocked && (!GameSceneUI.Instance.ShowingCinematicsCanvas))
        {
            // The ability was unlocked by the player, and the cinematics canvas is not blocking the ability from being used

            if (useDefaultBehaviour)
            {
                bool triggerKeyPressed = GetTriggerKeyInput();

                if (abilityActive)
                {
                    if (triggerKeyPressed)
                    {
                        AbilityActive();
                    }
                    else
                    {
                        AbilityEnd();
                    }
                }
                else
                {
                    if (cooldown >= 1.0f)
                    {
                        if (triggerKeyPressed)
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
            }
        }
    }

    private void UpdateUnlockStatus()
    {
        abilityItem = GameSceneUI.Instance.PlayerInventory.GetPlayerAbilityItem(GetAbilityType());

        SetAbilityUnlocked(abilityItem != null);
    }

    protected virtual void SetupUIIndicator()
    {
        SetChargeAmount(charge);
        SetCooldownAmount(cooldown);

        uiIndicator.SetKeyPromptText(triggerKey.ToString());

        if(maxUpgradeLevel == 0)
        {
            uiIndicator.HideUpgradeSlider();
        }
    }

    protected virtual void SetAbilityUnlocked(bool value)
    {
        PlayerAbilityType abilityType = GetAbilityType();

        unlocked = value;
        abilityUnlockStatuses[abilityType] = value;

        uiIndicator.gameObject.SetActive(unlocked);

        if(unlocked)
        {
            if(abilityItem != null)
            {
                CustomFloatProperty itemUpgradeLevelProperty = abilityItem.GetCustomFloatPropertyWithName("upgradeLevel", true);

                if(itemUpgradeLevelProperty != null)
                {
                    uiIndicator.SetUpgradeLevel(itemUpgradeLevelProperty.Value, maxUpgradeLevel);

                    abilityUpgradeLevels[abilityType] = (int)itemUpgradeLevelProperty.Value;
                }
            }
            else
            {
                Debug.LogError("Unlocking ability without setting abilityItem");
            }
        }
    }

    private bool GetTriggerKeyInput()
    {
        return Input.GetKey(triggerKey) && (!InputFieldSelection.AnyFieldSelected) && (!UIPanel.AnyBlockingPanelShowing());
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
        abilityActive = true;

        // Reduce player food level by initialFoodDecrease when the ability is first triggered
        playerStats.DecreaseFoodLevel(initialFoodDecrease);

        if (abilityTriggerSound != null)
        {
            AudioManager.Instance.PlaySoundEffect2D(abilityTriggerSound);
        }
    }

    protected virtual void AbilityActive()
    {
        // Reduce player food level by continuousFoodDecrease each frame while the ability is being used
        playerStats.DecreaseFoodLevel(continuousFoodDecrease * Time.deltaTime);
    }

    protected virtual void AbilityEnd()
    {
        abilityActive = false;
    }

    private void StartChargeSound()
    {
        if (chargeSound != null)
        {
            AudioManager.Instance.PlayLoopingSoundEffect(chargeSound, "charge_" + uiIndicator.name, false, true);
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

    private void LinkToInventory()
    {
        playerInventory = GameSceneUI.Instance.PlayerInventory;

        playerInventory.AbilitiesContainer.ContainerStateChangedEvent += OnAbilityContainerStateChanged;
    }

    protected abstract void FindUIIndicator();
    protected abstract PlayerAbilityType GetAbilityType();
}