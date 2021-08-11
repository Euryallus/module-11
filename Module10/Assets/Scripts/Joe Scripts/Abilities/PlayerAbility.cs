using System.Collections.Generic;
using UnityEngine;

// ||=======================================================================||
// || PlayerAbility: Base class for all player abilities that are collected ||
// ||    with an ability item and can be triggered in various ways.         ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the production phase (Module 11).                                 ||
// ||=======================================================================||

// The 5 ability types
public enum PlayerAbilityType
{
    Launch = 0,

    Freeze,

    Slam_Levitate,  // Note: You'll see this ability referred to as 'slam' throughout the code as this was the original name,
                    //   but in-game the name is displayed as 'Levitate' for extra clarity on what the ability does
    Grab,

    Glider
}

public abstract class PlayerAbility : MonoBehaviour
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [Header("Player Ability")]

    [SerializeField] protected KeyCode  triggerKey;                 // The key used to trigger the ability
    [SerializeField] protected bool     useDefaultBehaviour = true; // Whether to use the default functionality for triggering/charging the ability

    [Header("Upgrades")]

    [SerializeField]
    [Tooltip("The number of upgrade levels the ability has. Set to 0 if the ability cannot be upgraded")]
    protected int      maxUpgradeLevel;

    [Header("Charge & Cooldown")]

    [SerializeField] protected float    chargeTime   = 0.5f;    // The number of seconds it takes to charge the ability
    [SerializeField] protected float    cooldownTime = 5.0f;    // The number of seconds it takes for the ability to cooldown after being used

    [Header("Food Level Decrease")]

    [SerializeField] protected float    initialFoodDecrease;    // How much the player's food level decreases when the ability is first triggered
    [SerializeField] protected float    continuousFoodDecrease; // How much the food level continues to decrease each frame (* deltaTime) while the ability is continually used

    [Header("Sounds")]

    [SerializeField] private SoundClass chargeSound;            // The sound played while charging the ability
    [SerializeField] private SoundClass chargeEndSound;         // The sound played when charging stops
    [SerializeField] private SoundClass abilityTriggerSound;    // The sound played when the ability is triggered

    #endregion

    #region Properties

    public int MaxUpgradeLevel { get { return maxUpgradeLevel; } }

    #endregion

    protected   AbilityIndicator    uiIndicator;        // The UI indicator linked to the ability
    protected   float               charge;             // How much the ability is charged (0.0 - 1.0, 1.0 = fully charged)
    protected   float               cooldown;           // The cooldown value (0.0 - 1.0, 1.0 = cooldown done)
    protected   bool                charging;           // Whether the ability is being charged
    protected   bool                abilityActive;      // Whether the ability is being used
    protected   Item                abilityItem;        // The item that was collected to gain the ability

    private     InventoryPanel      playerInventory;    // Reference to the player's inventory
    private     bool                unlocked;           // Whether the ability has been unlocked

    // The current upgrade level of each ability type
    private static Dictionary<PlayerAbilityType, int> abilityUpgradeLevels = new Dictionary<PlayerAbilityType, int>()
    {
        { PlayerAbilityType.Launch,          0 },
        { PlayerAbilityType.Freeze,          0 },
        { PlayerAbilityType.Slam_Levitate,   0 },
        { PlayerAbilityType.Grab,            0 },
        { PlayerAbilityType.Glider,          0 }
    };

    // The current unlock status of each ability type
    private static Dictionary<PlayerAbilityType, bool> abilityUnlockStatuses = new Dictionary<PlayerAbilityType, bool>()
    {
        { PlayerAbilityType.Launch,          false },
        { PlayerAbilityType.Freeze,          false },
        { PlayerAbilityType.Slam_Levitate,   false },
        { PlayerAbilityType.Grab,            false },
        { PlayerAbilityType.Glider,          false }
    };


    public static bool AbilityIsUnlocked(PlayerAbilityType abilityType)
    {
        // Returns true if the given ability type has been unlocked
        return abilityUnlockStatuses[abilityType];
    }

    public static int GetAbilityUpgradeLevel(PlayerAbilityType abilityType)
    {
        // Returns the current upgrade level of the given ability type
        return abilityUpgradeLevels[abilityType];
    }

    protected virtual void Start()
    {
        // By default, cooldown is done but the ability is not charged
        charge = 0.0f;
        cooldown = 1.0f;

        // Find the UI indicator linked to the ability (FindUIIndicator is overridden in each child class)
        FindUIIndicator();

        if(uiIndicator == null)
        {
            // Throw an error if the UI indicator could not be found
            Debug.LogError("Failed to find UI ability indicator", gameObject);
        }

        // Set up the UI indicator to show the cooldown/charge/key prompt
        SetupUIIndicator();

        // Link the OnAbilityContainerStateChanged function to the inventory's container state changed event
        LinkToInventory();
    }

    private void OnAbilityContainerStateChanged(bool loadingContainer)
    {
        // Called when the inventory state changes to check if an ability item was acquired

        // Update the ability unlock status, only allowing an unlock cutscene to be shown if this function is not being called while loading
        UpdateUnlockStatus(!loadingContainer);
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
            // Re-link to the player's inventory if it becomes null (happens when switching scenes)
            LinkToInventory();
        }

        if (unlocked && useDefaultBehaviour)
        {
            // The ability was unlocked by the player and default behaviour should be used

            // Check for a valid key press
            bool triggerKeyPressed = GetTriggerKeyInput();

            if (abilityActive)
            {
                if (triggerKeyPressed)
                {
                    // Call AbilityActive while abilityActive == true and the trigger key is still being pressed
                    AbilityActive();
                }
                else
                {
                    // End the ability if currently marked as active but the trigger key was released
                    AbilityEnd();
                }
            }
            else
            {
                if (cooldown >= 1.0f)
                {
                    // Cooldown time has passed

                    if (triggerKeyPressed)
                    {
                        if (!charging)
                        {
                            // Start charging if the trigger key is pressed
                            ChargeStart();
                        }

                        if (charge < 1.0f)
                        {
                            // While charging, update the charge amount as a fraction of the total charge time
                            SetChargeAmount(charge + Time.deltaTime / chargeTime);
                        }
                        else
                        {
                            // Charging is done, start the ability and begin the cooldown period
                            ChargeEnd();

                            SetCooldownAmount(0.0f);

                            AbilityStart();
                        }
                    }
                    else
                    {
                        // Trigger key is not being pressed

                        if (charging)
                        {
                            // Stop charging and play a sound if charging is currently true

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
                    // Cooldown period is currently active, update the cooldown value as a fraction of the total cooldown time

                    SetCooldownAmount(cooldown + Time.deltaTime / cooldownTime);
                }
            }
        }
    }

    private void UpdateUnlockStatus(bool allowUnlockCutscene)
    {
        // Checks if the ability is unlocked, and if so also checks the current upgrade level of the ability

        PlayerAbilityType abilityType = GetAbilityType();

        // Try getting the item that must be obtained to unlock the ability
        abilityItem = GameSceneUI.Instance.PlayerInventory.GetPlayerAbilityItem(GetAbilityType());

        // If the player has the ability item, the ability is now unlocked
        bool nowUnlocked = (abilityItem != null);

        // Show/hide ability UI depending on if the ability is now unlocked
        uiIndicator.gameObject.SetActive(nowUnlocked);

        if (nowUnlocked)
        {
            if (abilityItem != null)
            {
                // Try getting the up-to-date upgrade level of the ability
                CustomFloatProperty itemUpgradeLevelProperty = abilityItem.GetCustomFloatPropertyWithName("upgradeLevel", true);

                // Get the current (non-updated) upgrade level
                int currentUpgradeLevel = abilityUpgradeLevels[abilityType];
                int newUpgradeLevel = 0;

                if (itemUpgradeLevelProperty != null)
                {
                    // The item has an upgrade level property and hence can be upgraded.
                    //   Get the new upgrade level.

                    newUpgradeLevel = (int)itemUpgradeLevelProperty.Value;

                    // Display the upgrade level in the UI and update the dictionary value
                    uiIndicator.SetUpgradeLevel(newUpgradeLevel, maxUpgradeLevel);
                    abilityUpgradeLevels[abilityType] = newUpgradeLevel;
                }

                if(allowUnlockCutscene)
                {
                    // An unlock cutscene can be shown if the item was unlocked/upgraded

                    if ((!unlocked) || (newUpgradeLevel > currentUpgradeLevel))
                    {
                        // The ability has been unlocked when it wasn't before, or has been upgraded to a higher level
                        //   Show an unlock cutscene displaying the ability that was unlocked and its level

                        TriggerUnlockCutscene(abilityItem, abilityType, newUpgradeLevel);
                    }
                }
            }
            else
            {
                Debug.LogError("Unlocking ability without setting abilityItem");
            }
        }

        // Update variables that keep track of unlock status
        abilityUnlockStatuses[abilityType] = nowUnlocked;
        unlocked = nowUnlocked;
    }

    private void TriggerUnlockCutscene(Item abilityItem, PlayerAbilityType abilityType, int upgradeLevel)
    {
        // Show a cutscene which tells the player they have unlocked/upgraded the ability

        AbilityUnlockCutscene cutscene = GetComponent<AbilityUnlockCutscene>();

        cutscene.Setup(abilityItem, abilityType, upgradeLevel);
        cutscene.StartCutscene();
    }

    protected virtual void SetupUIIndicator()
    {
        // Check for the ability unlock/upgrade status (without triggering a cutscene)
        UpdateUnlockStatus(false);

        // Update images to display the current charge/cooldown values
        SetChargeAmount(charge);
        SetCooldownAmount(cooldown);

        // Show the required key to trigger the ability
        uiIndicator.SetKeyPromptText(triggerKey.ToString());

        if(maxUpgradeLevel == 0)
        {
            // Don't show the upgrade slider if the ability cannot be upgraded
            uiIndicator.HideUpgradeSlider();
        }
    }

    private bool GetTriggerKeyInput()
    {
        // Returns true if the trigger key is pressed while not being blocked by something else (an input field, UI panel or cinematic)

        return Input.GetKey(triggerKey) && (!InputFieldSelection.AnyFieldSelected) &&
                (!UIPanel.AnyBlockingPanelShowing()) && (!GameSceneUI.Instance.ShowingCinematicsCanvas);
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
        PlayerInstance.ActivePlayer.PlayerStats.DecreaseFoodLevel(initialFoodDecrease);

        if (abilityTriggerSound != null)
        {
            AudioManager.Instance.PlaySoundEffect2D(abilityTriggerSound);
        }
    }

    protected virtual void AbilityActive()
    {
        // Reduce player food level by continuousFoodDecrease each frame while the ability is being used
        PlayerInstance.ActivePlayer.PlayerStats.DecreaseFoodLevel(continuousFoodDecrease * Time.deltaTime);
    }

    protected virtual void AbilityEnd()
    {
        abilityActive = false;
    }

    private void StartChargeSound()
    {
        // Starts playing a looping charge sound of one was set
        if (chargeSound != null)
        {
            AudioManager.Instance.PlayLoopingSoundEffect(chargeSound, "charge_" + uiIndicator.name, false, true);
        }
    }

    private void StopChargeSound()
    {
        // Stops the looping charge sound from playing if it was started
        if (chargeSound != null)
        {
            AudioManager.Instance.StopLoopingSoundEffect("charge_" + uiIndicator.name);
        }
    }

    public void SetChargeAmount(float chargeAmount)
    {
        // Sets charge value and updates UI to show it

        charge = chargeAmount;
        uiIndicator.SetChargeAmount(chargeAmount);
    }

    public void SetCooldownAmount(float cooldownAmount)
    {
        // Sets cooldown value and updates UI to show it

        cooldown = cooldownAmount;
        uiIndicator.SetCooldownAmount(cooldownAmount);
    }

    private void LinkToInventory()
    {
        // Gets a reference to the player's inventory and ensures OnAbilityContainerStateChanged
        //   is called when the state of the inventory ItemContainer changes

        playerInventory = GameSceneUI.Instance.PlayerInventory;

        playerInventory.AbilitiesContainer.ContainerStateChangedEvent += OnAbilityContainerStateChanged;
    }

    protected abstract void FindUIIndicator();
    protected abstract PlayerAbilityType GetAbilityType();
}