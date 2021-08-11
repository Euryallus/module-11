using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

// ||=======================================================================||
// || OptionsPanel: UI panel used to adjust various game options such       ||
// ||    as music/sound volume.                                             ||
// ||=======================================================================||
// || Used on prefab: Joe/UI/OptionsPanel                                   ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || originally for the prototype phase.                                   ||
// ||                                                                       ||
// || Changes made during the production phase (Module 11):                 ||
// ||                                                                       ||
// || - Now inherits from UIPanel (fixes a number of issues).               ||
// || - Added optionGroups, basically pages that can be switched between    ||
// ||    by clicking various tabs.                                          ||
// || - Added the option to reset game data.                                ||
// ||=======================================================================||

// All ways the options panel can be opened
public enum OptionsOpenType
{
    GameScenePause, // Options panel was opened from the game scene pause menu
    MainMenu        // Options panel was opened from the main menu
}

public class OptionsPanel : UIPanel
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private TabButtonsGroup    tabs;                   // Tabs used to switch between options groups
    [SerializeField] private GameObject[]       optionsGroups;          // GameObjects that contain groups of options, one for each tab

    [SerializeField] private TextMeshProUGUI    musicVolumeText;        // Text displaying music volume title/percentage
    [SerializeField] private TextMeshProUGUI    soundEffectsVolumeText; // Text displaying sound effects volume title/percentage

    [SerializeField] private Slider             musicSlider;            // Slider for adjusting music volume
    [SerializeField] private Slider             soundEffectsSlider;     // Slider for adjusting sound effects volume

    [SerializeField] private OptionsToggle      screenShakeToggle;      // Toggle for enabling/disabling the screen shake effect
    [SerializeField] private OptionsToggle      viewBobbingToggle;      // Toggle for enabling/disabling the view bobbing effect

    [SerializeField] private PressEffectButton  resetGameDataButton;    // Buttons used to reset game data

    [SerializeField] private GameObject         confirmPanelPrefab;     // Confirmation panel shown when the above button is pressed to prevent accidental clearing of data

    #endregion

    private OptionsOpenType openType;                   // Determines where the panel was opened from and where to return to (see OptionsOpenType)

    private bool            suppressSelectionSounds;    // Whether the sounds played when options are selected/changed should not be played (used for initial setup)
    private GameObject      activeOptionsGroup;         // The selected options group

    protected override void Start()
    {
        base.Start();

        foreach (GameObject optionsGroup in optionsGroups)
        {
            // Hide all options groups before one is selected
            optionsGroup.SetActive(false);
        }

        // Subscribe to toggle events for all OptionsToggles to values can be saved when they are clicked
        screenShakeToggle.ToggleEvent += ScreenShakeToggled;
        viewBobbingToggle.ToggleEvent += ViewBobbingToggled;

        // Call the OnTabSelected function when any tab is selected
        tabs.TabSelectedEvent += OnTabSelected;
    }

    public void Setup(OptionsOpenType openType)
    {
        this.openType = openType;
    }

    public void OnTabSelected(int tabIndex, bool playerSelected)
    {
        // Setup the options group that corresponds to the selected tab
        SetupOptionsGroup(tabIndex);

        // Play a sound if the tab was selected by the player
        if(playerSelected)
        {
            AudioManager.Instance.PlaySoundEffect2D("buttonClickSmall");
        }
    }

    private void SetupOptionsGroup(int groupIndex)
    {
        // Don't play selection sounds while setting up options
        suppressSelectionSounds = true;

        if(activeOptionsGroup != null)
        {
            // Hide any active options group
            activeOptionsGroup.SetActive(false);
        }

        // Show the group at the given index and set it as the active group
        optionsGroups[groupIndex].SetActive(true);
        activeOptionsGroup = optionsGroups[groupIndex];

        // Setup whatever options are contained within the selected group
        switch (groupIndex)
        {
            case 0: SetupGraphicsOptions();
                break;

            case 1: SetupAudioOptions();
                break;

            case 2: SetupGameOptions();
                break;
        }

        // Allow sounds to be played again now setup is done
        suppressSelectionSounds = false;
    }

    private void SetupGraphicsOptions()
    {
        SaveLoadManager slm = SaveLoadManager.Instance;

        // Update UI to show the current saved values for graphics options:

        // Graphics toggles
        screenShakeToggle.SetSelected(slm.GetBoolFromPlayerPrefs("screenShake"));
        viewBobbingToggle.SetSelected(slm.GetBoolFromPlayerPrefs("viewBobbing"));
    }

    private void SetupAudioOptions()
    {
        SaveLoadManager slm = SaveLoadManager.Instance;

        // Update UI to show the current saved values for audio options:

        // Slider options
        UpdateMusicVolumeUI       (slm.GetIntFromPlayerPrefs("musicVolume"));
        UpdateSoundEffectsVolumeUI(slm.GetIntFromPlayerPrefs("soundEffectsVolume"));
    }

    private void SetupGameOptions()
    {
        // Update UI to show the current saved values for game options:

        resetGameDataButton.SetInteractable(SceneManager.GetActiveScene().name == "MainMenu");
    }

    private void UpdateMusicVolumeUI(int savedMusicVolume)
    {
        // Set the value of the slider and display the new music volume as a percentage

        musicSlider.value = savedMusicVolume;
        musicVolumeText.text = "Music Volume (" + (savedMusicVolume * 5) + "%)";

        // Update the volume of all active music sources
        AudioManager.Instance.UpdateMusicSourcesVolume();
    }

    private void UpdateSoundEffectsVolumeUI(int savedSoundEffectsVolume)
    {
        soundEffectsSlider.value = savedSoundEffectsVolume;
        soundEffectsVolumeText.text = "Sound Effects Volume (" + (savedSoundEffectsVolume * 5) + "%)";

        // Update the volume of all active looping sound sources (volume for standard sound effects is determined when they are played,
        //   but looping sound volume needs to be adjusted dynamically since volume may be changed while they are playing)
        AudioManager.Instance.UpdateActiveLoopingSoundsVolume();
    }

    // Called by UI/Panel/Toggle events:
    //==================================

    private void ScreenShakeToggled(bool enabled)
    {
        // Save the new value for whether screen shake is enabled
        SaveLoadManager.Instance.SaveBoolToPlayerPrefs("screenShake", enabled);

        if (!suppressSelectionSounds)
        {
            AudioManager.Instance.PlaySoundEffect2D("buttonClickSmall");
        }
    }

    private void ViewBobbingToggled(bool enabled)
    {
        // Save the new value for whether view bobbing is enabled
        SaveLoadManager.Instance.SaveBoolToPlayerPrefs("viewBobbing", enabled);

        if (!suppressSelectionSounds)
        {
            AudioManager.Instance.PlaySoundEffect2D("buttonClickSmall");
        }
    }

    public void ButtonReturn()
    {
        if(openType == OptionsOpenType.GameScenePause)
        {
            // Hide this options panel and re-show the pause menu
            GameSceneUI.Instance.PauseAndShowPauseUI();
            GameSceneUI.Instance.HideOptionsUI();
        }
        else if(openType == OptionsOpenType.MainMenu)
        {
            // Hide this options panel to stop blocking the main menu
            GameObject.Find("MainMenu").GetComponent<MainMenu>().HideOptionsPanel();
        }
    }

    public void MusicSliderValueChanged(float value)
    {
        // The music slider was adjusted

        // Get the value the slider was set to as an int
        //  (slider moves in integer increments but always returns the value as a float)
        int valueAsInt = (int)value;

        // Save the new value
        SaveLoadManager.Instance.SaveIntToPlayerPrefs("musicVolume", valueAsInt);

        // Update UI to reflect the new value
        UpdateMusicVolumeUI(valueAsInt);

        if (!suppressSelectionSounds)
        {
            AudioManager.Instance.PlaySoundEffect2D("buttonClickTiny1");
        }
    }

    public void SoundEffectsSliderValueChanged(float value)
    {
        // The sound effects slider was adjusted

        // Get the value the slider was set to as an int
        //  (slider moves in integer increments but always returns the value as a float)
        int valueAsInt = (int)value;

        // Save the new value
        SaveLoadManager.Instance.SaveIntToPlayerPrefs("soundEffectsVolume", valueAsInt);

        // Update UI to reflect the new value
        UpdateSoundEffectsVolumeUI(valueAsInt);

        if(!suppressSelectionSounds)
        {
            AudioManager.Instance.PlaySoundEffect2D("buttonClickTiny1");
        }
    }

    public void ResetGameDataButton()
    {
        // Hide the options panel to make way for the confirmation panel
        Hide();

        // Show a panel asking the player to confirm or cancel their choice to reset data

        ConfirmInfoPanel resetDataConfirmPanel = Instantiate(confirmPanelPrefab, transform.parent).GetComponent<ConfirmInfoPanel>();
        resetDataConfirmPanel.Setup("Reset Game Data", "Are you sure you want to reset game data? All saved progress will be lost.");

        // OnResetDataCancel or OnResetDataConfirm are called if the respective buttons are pressed
        resetDataConfirmPanel.CloseButtonPressedEvent   += OnResetDataCancel;
        resetDataConfirmPanel.ConfirmButtonPressedEvent += OnResetDataConfirm;

        AudioManager.Instance.PlaySoundEffect2D("buttonClickMain1");
    }

    private void OnResetDataCancel()
    {
        // The reset data operation was cancelled, re-show the options menu
        Show();
    }

    private void OnResetDataConfirm()
    {
        // The reset data operation was confirmed

        // Attempt to reset game data
        bool resetSuccess = SaveLoadManager.Instance.ResetGameData();

        // Show a panel that will tell the player if the reset was successful
        ConfirmInfoPanel resetResultPanel = Instantiate(confirmPanelPrefab, transform.parent).GetComponent<ConfirmInfoPanel>();

        // Re-show the options menu when the confirm button is pressed on the panel created above
        resetResultPanel.ConfirmButtonPressedEvent += Show;

        if (resetSuccess)
        {
            // Reset was successful, tell the player
            resetResultPanel.Setup("Reset Successful", "Game data has been reset.", true, "", "OK");
        }
        else
        {
            // Reset resulted in an error, warn the player and recommend a possible fix
            resetResultPanel.Setup("Reset Failed", "Game data could not be reset. Ensure the save directory is not marked as read-only.", true, "", "OK");
        }
    }
}