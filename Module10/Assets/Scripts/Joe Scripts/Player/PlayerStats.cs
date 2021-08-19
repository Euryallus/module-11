using UnityEngine;

// ||=======================================================================||
// || PlayerStats: Handles certain stat values related to the player,       ||
// ||   their health, food level and breath level and related UI elements.  ||
// ||   Also saves and loads these values.                                  ||
// ||=======================================================================||
// || Used on prefab: Player                                                ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || originally for the prototype phase.                                   ||
// || Additional code by Hugo Bailey (see comments)                         ||
// ||                                                                       ||
// || Changes made during the production phase (Module 11):                 ||
// ||                                                                       ||
// || - Added a player damage effect                                        ||
// || - Health now increases over time based on the player's food level     ||
// || - Player now dies when health reaches 0                               ||
// || - Allowed the default/starting food & health values to be adjusted    ||
// ||=======================================================================||

// Edited for mod11:
// - Added damage effect
// - Health now increases over time based on food level
// - Player dies when health reached 0

[RequireComponent(typeof(PlayerMovement))]
public class PlayerStats : MonoBehaviour, IPersistentGlobalObject
{
    #region InspectorVariables
    // Variables in this region are set in the inspector. See tooltips for more info.

    [Header("Hunger/Food Level (See tooltips for info)")]

    [SerializeField] [Tooltip("The value the player's food level starts at when starting a game/respawning after death.")]
    private float   foodLevelDefaultValue   = 1.0f;

    [SerializeField] [Tooltip("The number of seconds it will take for the player to starve if they are idle.")]
    private float   baseTimeToStarve        = 600.0f;

    [SerializeField] [Tooltip("How many times quicker the player's food level will decrease when they are walking/crouching.")]
    private float   walkHungerMultiplier    = 1.25f;

    [SerializeField] [Tooltip("How many times quicker the player's food level will decrease when they are running.")]
    private float   runHungerMultiplier     = 1.5f;

    [SerializeField] [Tooltip("How full the player's food level has to be before that cannot eat any more food (1.0 = full, 0.0 = starving)")]
    private float   fullThreshold           = 0.9f;

    [SerializeField] [Tooltip("How much the player's health decreases by every [starveDamageInterval] seconds when starving.")]
    private float   starveDamage            = 0.05f;

    [SerializeField] [Tooltip("How frequently (in seconds) the player takes damage when starving.")]
    private float   starveDamageInterval    = 2.0f;

    [Header("Health")]

    [SerializeField] [Tooltip("The value the player's health starts at when starting a game/respawning after death.")]
    private float   healthDefaultValue      = 1.0f;

    [SerializeField] [Tooltip("How quickly the player's health will increase over time (adjusted based on food level).")]
    private float   healthIncreaseSpeed     = 0.04f;

    // Drowning-related variables added by Hugo:
    [Header("Drowning")]
    [SerializeField]  [Tooltip("How much the player's health decreases by every [drownDamageInterval] seconds when starving.")]
    private float   drownDamage             = 0.1f;

    [SerializeField] [Tooltip("How frequently (in seconds) the player takes damage when drowning.")]
    private float   drownDamageInterval     = 2.0f;

    [SerializeField] [Tooltip("Time player takes to drown")]
    private float   baseTimeToDrown         = 60.0f;

    [Header("UI")]
    [SerializeField] private GameObject  damageEffectPrefab; // Prefab showing a red vignette flash effect that is instantiated when the player takes damage

    #endregion

    #region Properties

    public float MaxHealth      { get { return healthDefaultValue; } }
    public float Health         { get { return health; } }
    public float MaxFoodLevel   { get { return foodLevelDefaultValue; } }
    public float FoodLevel      { get { return foodLevel; } }

    public static string PlayerName { get { return playerName; } set { playerName = value; } }

    #endregion

    private Transform       canvasTransform;        // Transform of the canvas used as a parent for the damage effect
    private float           health;                 // The player's health (0 = death, [healthDefaultValue] = full)
    private float           foodLevel;              // The player's food level (0 = starving, [foodLevelDefaultValue] = full)

    // Added by Hugo       
    private float           breath    = 1.0f;       // The player's breath level (0 = drowning, 1 = full)
                                                       
    private float           starveDamageTimer;      // Keeps track of seconds passed since damage was taken from starving
    private float           drownDamageTimer;       // Keeps trrack of seconds passed since damage was taken from drowning
    private PlayerStatsUI   statsUI;

    private static string playerName = "Player";        // The name set by the player

    private const float StatWarningThreshold = 0.15f;   // How low a stat value has to get before the related slider flashes red as a warning

    protected void Start()
    {
        // Initialise default health/food level values
        ResetDefaultStatValues();

        // Subscribe to save/load events so player stats are saved/loaded with the game
        SaveLoadManager.Instance.SubscribeGlobalSaveLoadEvents(OnGlobalSave, OnGlobalLoadSetup, OnGlobalLoadConfigure);
    }

    private void OnDestroy()
    {
        //Unsubscribe from save/load events if for some reason the GameObject is destroyed to prevent null reference errors
        SaveLoadManager.Instance.UnsubscribeGlobalSaveLoadEvents(OnGlobalSave, OnGlobalLoadSetup, OnGlobalLoadConfigure);
    }

    private void Update()
    {
        if(statsUI == null)
        {
            // Find the player hotbar/stats UI if it's null, happens on start or when switching scenes
            statsUI = GameObject.FindGameObjectWithTag("HotbarAndStats").GetComponent<PlayerStatsUI>();
        }

        if (canvasTransform == null)
        {
            // Find the canvas transform if it's null, happens on start or when switching scenes
            canvasTransform = GameObject.FindGameObjectWithTag("JoeCanvas").transform;
        }

        // Calculate how much the food/breath levels should decrease each frame
        float foodLevelDecreaseAmount = Time.deltaTime / baseTimeToStarve;
        float breathDecreaseAmount = Time.deltaTime / baseTimeToDrown;

        // Update the player's food level value
        UpdateFoodLevel(foodLevelDecreaseAmount);

        // Update the player's health value
        UpdateHealth();

        // Breath/drowning code added by hugo
        UpdateBreathLevel(breathDecreaseAmount);

        // Update UI for all stats
        UpdateHealthUI();
        UpdateFoodLevelUI();
        UpdateBreathUI();
    }

    public void OnGlobalSave(SaveData saveData)
    {
        //Save the player's food level, health and breath

        saveData.AddData("playerFoodLevel", foodLevel);

        saveData.AddData("playerHealth", health);

        saveData.AddData("playerBreath", breath);
    }

    public void OnGlobalLoadSetup(SaveData saveData)
    {
        // Only load in health/food/breath values if the game is being loaded from the menu
        //   rather than after a death. If the player died, values will instead be set to their
        //   default values to restore full health/hunger

        if (!SaveLoadManager.Instance.LoadingAfterDeath)
        {
            //Load the player's food level, health and breath
            //  Stat levels will be left at their default values if loading fails

            float loadedFoodLevel = saveData.GetData<float>("playerFoodLevel", out bool loadSuccess);
            if (loadSuccess)
            {
                foodLevel = loadedFoodLevel;
            }

            float loadedHealth = saveData.GetData<float>("playerHealth", out loadSuccess);
            if (loadSuccess)
            {
                health = loadedHealth;
            }

            float loadedBreath = saveData.GetData<float>("playerBreath", out loadSuccess);
            if (loadSuccess)
            {
                breath = loadedBreath;
            }
        }
        else
        {
            // Loading after the player died, restore stats to their default values
            ResetDefaultStatValues();
        }
    }

    private void ResetDefaultStatValues()
    {
        // Resets stat variables to their default values

        foodLevel = foodLevelDefaultValue;
        health    = healthDefaultValue;
        breath    = 1.0f;
    }

    public void OnGlobalLoadConfigure(SaveData saveData) { } // Nothing to configure

    private void UpdateFoodLevel(float foodLevelDecreaseAmount)
    {
        if (foodLevel > 0.0f)
        {
            // Decrease food level depending on player state

            PlayerMovement playerMovement = PlayerInstance.ActivePlayer.PlayerMovement;

            if (playerMovement.PlayerIsMoving())
            {
                // Decrease hunger faster if the player is walking or running
                if (playerMovement.GetCurrentMovementState() == PlayerMovement.MovementStates.run)
                {
                    foodLevelDecreaseAmount *= runHungerMultiplier;
                }
                else
                {
                    foodLevelDecreaseAmount *= walkHungerMultiplier;
                }
            }

            DecreaseFoodLevel(foodLevelDecreaseAmount);
        }
        else
        {
            // Player is starving, increase damage timer
            starveDamageTimer += Time.deltaTime;

            //  Player has been starving for starveDamageInterval, take damage and reset the timer
            if (starveDamageTimer >= starveDamageInterval)
            {
                DecreaseHealth(starveDamage, PlayerDeathCause.Starved);
                starveDamageTimer = 0.0f;
            }
        }
    }

    private void UpdateHealth()
    {
        // Increase health over time based on healthIncreaseSpeed, and multiplied by the food level as a fraction
        //   of its max value to give a value between 0.0 (no increase) and 1.0 (full speed increase)
        IncreaseHealth(Time.deltaTime * healthIncreaseSpeed * (foodLevel / foodLevelDefaultValue));
    }

    private void UpdateBreathLevel(float breathDecreaseAmount)
    {
        // Added by Hugo - decreases breath while in water, or increases it otherwise

        statsUI.BreathCanvasGroup.alpha = breath < 1.0 ? 1 : 0;

        if (PlayerInstance.ActivePlayer.PlayerMovement.currentMovementState == PlayerMovement.MovementStates.dive)
        {
            DecreaseBreath(breathDecreaseAmount);
        }
        else
        {
            IncreaseBreath(Time.deltaTime);
        }

        if (breath < 1.0f)
        {
            statsUI.BreathCanvasGroup.alpha = 1f;

            if (breath == 0.0f)
            {
                // Player is drowning, increase damage timer
                drownDamageTimer += Time.deltaTime;

                //  Player has been drowning for drownDamageInterval, take damage and reset the timer
                if (drownDamageTimer >= drownDamageInterval)
                {
                    DecreaseHealth(drownDamage, PlayerDeathCause.Drowned);
                    drownDamageTimer = 0.0f;
                }
            }
        }
    }

    public void IncreaseFoodLevel(float amount)
    {
        // Increase food level by the given amount, ensuring it stays between 0.0 and 1.0

        foodLevel += amount;

        foodLevel = Mathf.Clamp(foodLevel, 0.0f, foodLevelDefaultValue);
    }

    public void DecreaseFoodLevel(float amount)
    {
        // Decrease food level by the given amount, ensuring it stays between 0.0 and 1.0

        foodLevel -= amount;

        foodLevel = Mathf.Clamp(foodLevel, 0.0f, foodLevelDefaultValue);
    }

    public void IncreaseHealth(float amount)
    {
        // Increase health by the given amount, ensuring it stays between 0.0 and 1.0

        health += amount;

        health = Mathf.Clamp(health, 0.0f, healthDefaultValue);
    }

    public void DecreaseHealth(float amount, PlayerDeathCause potentialDeathCause)
    {
        // Decrease health by the given amount, ensuring it stays between 0.0 and 1.0

        health -= amount;

        health = Mathf.Clamp(health, 0.0f, healthDefaultValue);

        if (health == 0.0f)
        {
            GetComponent<PlayerDeath>().KillPlayer(potentialDeathCause);

            ResetDefaultStatValues();
        }
        else
        {
            Instantiate(damageEffectPrefab, canvasTransform);
        }
    }

    // Increase/DecreaseBreath and UpdateBreathUI Added by Hugo
    public void IncreaseBreath(float amount)
    {
        // Increase breath level by the given amount, ensuring it stays between 0.0 and 1.0

        breath += amount;

        breath = Mathf.Clamp(breath, 0.0f, 1.0f);
    }

    public void DecreaseBreath(float amount)
    {
        // Decrease breath level by the given amount, ensuring it stays between 0.0 and 1.0

        breath -= amount;

        breath = Mathf.Clamp(breath, 0.0f, 1.0f);
    }

    private void UpdateBreathUI()
    {
        // Lerp the breath slider value towards breath so the value smoothly changes
        statsUI.BreathLevelSlider.value = Mathf.Lerp(statsUI.BreathLevelSlider.value, breath, Time.unscaledDeltaTime * 25.0f);

        //Flash the breath slider bar or background red depending on how low the level is
        statsUI.BreathSliderAnimator.SetBool("Flash", (breath < StatWarningThreshold));

        statsUI.BreathSliderBGAnimator.SetBool("Flash", (breath == 0.0f));
    }

    private void UpdateFoodLevelUI()
    {
        // Lerp the food slider value towards foodLevel so the value smoothly changes
        statsUI.FoodLevelSlider.value = Mathf.Lerp(statsUI.FoodLevelSlider.value, (foodLevel / foodLevelDefaultValue), Time.unscaledDeltaTime * 25.0f);

        //Flash the food slider bar or background red depending on how low the level is
        statsUI.FoodSliderAnimator.SetBool("Flash", (foodLevel < StatWarningThreshold));

        statsUI.FoodSliderBGAnimator.SetBool("Flash", (foodLevel == 0.0f));
    }

    private void UpdateHealthUI()
    {
        // Lerp the health slider value towards health so the value smoothly changes
        statsUI.HealthSlider.value = Mathf.Lerp(statsUI.HealthSlider.value, (health / healthDefaultValue), Time.unscaledDeltaTime * 25.0f);

        //Flash the health slider red if health is getting low
        statsUI.HealthSliderAnimator.SetBool("Flash", (health < StatWarningThreshold));
    }

    public bool PlayerIsFull()
    {
        // Returns true if the player is close to having a full food level

        return (foodLevel > fullThreshold);
    }
}