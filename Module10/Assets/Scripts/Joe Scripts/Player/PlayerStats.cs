using UnityEngine;
using UnityEngine.UI;

// ||=======================================================================||
// || PlayerStats: Handles certain stat values related to the player,       ||
// ||   their health, food level and breath level and related UI elements.  ||
// ||   Also saves and loads these values.                                  ||
// ||=======================================================================||
// || Used on prefab: Player                                                ||
// ||=======================================================================||
// || Written by Joseph Allen for the prototype phase.                      ||
// || Additional code by Hugo Bailey (see comments).                        ||
// ||=======================================================================||

[RequireComponent(typeof(PlayerMovement))]
public class PlayerStats : MonoBehaviour, IPersistentObject
{
    #region InspectorVariables
    // Variables in this region are set in the inspector. See tooltips for more info.

    [Header("Hunger Options (See tooltips for info)")]

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

    // Drowning-related variables added by Hugo:

    [SerializeField]  [Tooltip("How much the player's health decreases by every [drownDamageInterval] seconds when starving.")]
    private float   drownDamage             = 0.1f;

    [SerializeField] [Tooltip("How frequently (in seconds) the player takes damage when drowning.")]
    private float   drownDamageInterval     = 2.0f;

    [SerializeField] [Tooltip("Time player takes to drown")]
    private float baseTimeToDrown           = 60.0f;

    [Header("UI")]
    [SerializeField] private Slider      healthSlider;       // The slider showing health
    [SerializeField] private Image       healthSliderFill;   // The fill bar for the above slider
    [SerializeField] private Slider      foodLevelSlider;    // The slider showing food level
    [SerializeField] private Image       foodSliderFill;     // The fill bar for the above slider
    [SerializeField] private Slider      breathLevelSlider;  // The slider showing breath
    [SerializeField] private Image       breathSliderFill;   // The fill bar for the above slider
    [SerializeField] private CanvasGroup breathCanvasGroup;  // Canvas group for showing/hiding the breath indicator
        


    #endregion

    private float           health    = 1.0f;       // The player's health (0 = death, 1 = full)
    private float           foodLevel = 1.0f;       // The player's food level (0 = starving, 1 = full)
                                                       
    // Added by Hugo       
    private float           breath    = 1.0f;       // The player's breath level (0 = drowning, 1 = full)
                                                       
    private float           starveDamageTimer;      // Keeps track of seconds passed since damage was taken from starving
    private float           drownDamageTimer;          
    private PlayerMovement  playerMovement;         // Reference to the script that controls player movement
    private Animator        healthSliderAnimator;   // Animator used for flashing the health slider bar red when health is low
    private Animator        foodSliderAnimator;     // Animator used for flashing the food slider bar red when food level is low
    private Animator        foodSliderBgAnimator;   // Animator used for flashing the food slider background red when player is starving                                        
    private Animator        breathSliderAnimator;   // Animator used for flashing the breath slider bar red when player is close to drowning
    private Animator        breathSliderBgAnimator; // Animator used for flashing the breath slider background red when player is drowning     

    private const float StatWarningThreshold = 0.15f;   // How low a stat value has to get before the related slider flashes red as a warning

    private void Awake()
    {
        // Get player movement script and animators for various UI elements

        playerMovement = GetComponent<PlayerMovement>();

        healthSliderAnimator    = healthSliderFill.gameObject.GetComponent<Animator>();

        foodSliderAnimator      = foodSliderFill.gameObject.GetComponent<Animator>();
        foodSliderBgAnimator    = foodLevelSlider.transform.Find("Background").GetComponent<Animator>();

        breathSliderAnimator    = breathSliderFill.gameObject.GetComponent<Animator>();
        breathSliderBgAnimator  = breathLevelSlider.transform.Find("Background").GetComponent<Animator>();
    }

    protected void Start()
    {
        // Subscribe to save/load events so player stats are saved/loaded with the game
        SaveLoadManager.Instance.SubscribeSaveLoadEvents(OnSave, OnLoadSetup, OnLoadConfigure);
    }

    private void OnDestroy()
    {
        //Unsubscribe from save/load events if for some reason the GameObject is destroyed to prevent null reference errors
        SaveLoadManager.Instance.UnsubscribeSaveLoadEvents(OnSave, OnLoadSetup, OnLoadConfigure);
    }

    private void Update()
    {
        // Calculate how much the food/breath levels should decrease each frame
        float foodLevelDecreaseAmount = Time.deltaTime / baseTimeToStarve;
        float breathDecreaseAmount = Time.deltaTime / baseTimeToDrown;

        UpdateFoodLevel(foodLevelDecreaseAmount);

        // Breath/drowning code added by hugo
        UpdateBreathLevel(breathDecreaseAmount);

        UpdateHealthUI();
        UpdateFoodLevelUI();
        UpdateBreathUI();
    }

    public void OnSave(SaveData saveData)
    {
        // Save the player's food level, health and breath

        saveData.AddData("playerFoodLevel", foodLevel);

        saveData.AddData("playerHealth", health);

        saveData.AddData("playerBreath", breath);
    }

    public void OnLoadSetup(SaveData saveData)
    {
        // Load the player's food level, health and breath
        //  Stat levels will be left at their default values if loading fails

        bool loadSuccess;

        float loadedFoodLevel = saveData.GetData<float>("playerFoodLevel", out loadSuccess);
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

    public void OnLoadConfigure(SaveData saveData) { } // Nothing to configure

    private void UpdateFoodLevel(float foodLevelDecreaseAmount)
    {
        if (foodLevel > 0.0f)
        {
            // Decrease food level depending on player state

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
                DecreaseHealth(starveDamage);
                starveDamageTimer = 0.0f;
            }
        }
    }

    private void UpdateBreathLevel(float breathDecreaseAmount)
    {
        // Added by Hugo

        breathCanvasGroup.alpha = breath < 1.0 ? 1 : 0;

        if (playerMovement.currentMovementState == PlayerMovement.MovementStates.dive)
        {
            DecreaseBreath(breathDecreaseAmount);
        }
        else
        {
            IncreaseBreath(Time.deltaTime);
        }

        if (breath < 1.0f)
        {
            breathCanvasGroup.alpha = 1f;

            if (breath == 0.0f)
            {
                // Player is drowning, increase damage timer
                drownDamageTimer += Time.deltaTime;

                //  Player has been drowning for drownDamageInterval, take damage and reset the timer
                if (drownDamageTimer >= drownDamageInterval)
                {
                    DecreaseHealth(drownDamage);
                    drownDamageTimer = 0.0f;
                }
            }
        }
    }

    public void IncreaseFoodLevel(float amount)
    {
        // Increase food level by the given amount, ensuring it stays between 0.0 and 1.0

        foodLevel += amount;

        foodLevel = Mathf.Clamp(foodLevel, 0.0f, 1.0f);
    }

    public void DecreaseFoodLevel(float amount)
    {
        // Decrease food level by the given amount, ensuring it stays between 0.0 and 1.0

        foodLevel -= amount;

        foodLevel = Mathf.Clamp(foodLevel, 0.0f, 1.0f);
    }

    public void IncreaseHealth(float amount)
    {
        // Increase health by the given amount, ensuring it stays between 0.0 and 1.0

        health += amount;

        health = Mathf.Clamp(health, 0.0f, 1.0f);
    }

    public void DecreaseHealth(float amount)
    {
        // Decrease health by the given amount, ensuring it stays between 0.0 and 1.0

        health -= amount;

        health = Mathf.Clamp(health, 0.0f, 1.0f);
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
        breathLevelSlider.value = Mathf.Lerp(breathLevelSlider.value, breath, Time.deltaTime * 25.0f);

        //Flash the breath slider bar or background red depending on how low the level is
        breathSliderAnimator.SetBool("Flash", (breath < StatWarningThreshold));

        breathSliderBgAnimator.SetBool("Flash", (breath == 0.0f));
    }

    private void UpdateFoodLevelUI()
    {
        // Lerp the food slider value towards foodLevel so the value smoothly changes
        foodLevelSlider.value = Mathf.Lerp(foodLevelSlider.value, foodLevel, Time.deltaTime * 25.0f);

        //Flash the food slider bar or background red depending on how low the level is
        foodSliderAnimator.SetBool("Flash", (foodLevel < StatWarningThreshold));

        foodSliderBgAnimator.SetBool("Flash", (foodLevel == 0.0f));
    }

    private void UpdateHealthUI()
    {
        // Lerp the health slider value towards health so the value smoothly changes
        healthSlider.value = Mathf.Lerp(healthSlider.value, health, Time.deltaTime * 25.0f);

        //Flash the health slider red if health is getting low
        healthSliderAnimator.SetBool("Flash", (health < StatWarningThreshold));
    }

    public bool PlayerIsFull()
    {
        // Returns true if the player is close to having a full food level

        return (foodLevel > fullThreshold);
    }

}