using UnityEngine;
using UnityEngine.UI;
using TMPro;

// All ways the options panel can be opened
public enum OptionsOpenType
{
    GameScenePause, // Options panel was opened from the game scene pause menu
    MainMenu        // Options panel was opened from the main menu
}

// ||=======================================================================||
// || OptionsPanel: UI panel used to adjust various game options such       ||
// ||    as music/sound volume.                                             ||
// ||=======================================================================||
// || Used on prefab: Joe/UI/OptionsPanel                                   ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

// Edited for Mod11: now inherits from UIPanel

public class OptionsPanel : UIPanel
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private TabButtonsGroup tabs;
    [SerializeField] private GameObject[]    optionsGroups;

    [SerializeField] private TextMeshProUGUI musicVolumeText;        // Text displaying music volume title/percentage
    [SerializeField] private TextMeshProUGUI soundEffectsVolumeText; // Text displaying sound effects volume title/percentage

    [SerializeField] private Slider          musicSlider;            // Slider for adjusting music volume
    [SerializeField] private Slider          soundEffectsSlider;     // Slider for adjusting sound effects volume

    [SerializeField] private OptionsToggle  screenShakeToggle;       // Toggle for enabling/disabling the screen shake effect
    [SerializeField] private OptionsToggle  viewBobbingToggle;       // Toggle for enabling/disabling the view bobbing effect

    [SerializeField] private GameObject     confirmPanelPrefab;

    #endregion

    private OptionsOpenType openType;   //Determines where the panel was opened from and where to return to (see OptionsOpenType)

    private bool suppressSelectionSounds;
    private GameObject activeOptionsGroup;

    protected override void Start()
    {
        base.Start();

        foreach (GameObject optionsGroup in optionsGroups)
        {
            optionsGroup.SetActive(false);
        }

        // Subscribe to toggle events for all OptionsToggles to values can be saved when they are clicked
        screenShakeToggle.ToggleEvent += ScreenShakeToggled;
        viewBobbingToggle.ToggleEvent += ViewBobbingToggled;

        tabs.TabSelectedEvent += OnTabSelected;
    }

    public void Setup(OptionsOpenType openType)
    {
        this.openType = openType;
    }

    public void OnTabSelected(int tabIndex, bool playerSelected)
    {
        SetupOptionsGroup(tabIndex);

        if(playerSelected)
        {
            AudioManager.Instance.PlaySoundEffect2D("buttonClickSmall");
        }
    }

    private void SetupOptionsGroup(int groupIndex)
    {
        suppressSelectionSounds = true;

        if(activeOptionsGroup != null)
        {
            activeOptionsGroup.SetActive(false);
        }

        optionsGroups[groupIndex].SetActive(true);
        activeOptionsGroup = optionsGroups[groupIndex];

        switch (groupIndex)
        {
            case 0: SetupGraphicsOptions();
                break;

            case 1: SetupAudioOptions();
                break;

            case 2: SetupGameOptions();
                break;
        }

        suppressSelectionSounds = false;
    }

    private void SetupGraphicsOptions()
    {
        Debug.Log("Setting up graphics options");

        SaveLoadManager slm = SaveLoadManager.Instance;

        // Update UI to show the current saved values for graphics options:

        // Graphics toggles
        screenShakeToggle.SetSelected(slm.GetBoolFromPlayerPrefs("screenShake"));
        viewBobbingToggle.SetSelected(slm.GetBoolFromPlayerPrefs("viewBobbing"));
    }

    private void SetupAudioOptions()
    {
        Debug.Log("Setting up audio options");

        SaveLoadManager slm = SaveLoadManager.Instance;

        // Update UI to show the current saved values for audio options:

        // Slider options
        UpdateMusicVolumeUI       (slm.GetIntFromPlayerPrefs("musicVolume"));
        UpdateSoundEffectsVolumeUI(slm.GetIntFromPlayerPrefs("soundEffectsVolume"));
    }

    private void SetupGameOptions()
    {
        // Update UI to show the current saved values for game options:

        Debug.Log("Setting up game options");
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
        Hide();

        ConfirmInfoPanel resetDataConfirmPanel = Instantiate(confirmPanelPrefab, transform.parent).GetComponent<ConfirmInfoPanel>();
        resetDataConfirmPanel.Setup("Reset Game Data", "Are you sure you want to reset game data? All saved progress will be lost.");

        resetDataConfirmPanel.CloseButtonPressedEvent   += OnResetDataCancel;
        resetDataConfirmPanel.ConfirmButtonPressedEvent += OnResetDataConfirm;

        AudioManager.Instance.PlaySoundEffect2D("buttonClickMain1");
    }

    private void OnResetDataCancel()
    {
        Show();
    }

    private void OnResetDataConfirm()
    {
        bool resetSuccess = SaveLoadManager.Instance.ResetGameData();

        ConfirmInfoPanel resetResultPanel = Instantiate(confirmPanelPrefab, transform.parent).GetComponent<ConfirmInfoPanel>();
        resetResultPanel.ConfirmButtonPressedEvent += Show;

        if (resetSuccess)
        {
            resetResultPanel.Setup("Reset Successful", "Game data has been reset.", true, "", "OK");
        }
        else
        {
            resetResultPanel.Setup("Reset Failed", "Game data could not be reset. Ensure the save directory is not marked as read-only.", true, "", "OK");
        }
    }
}
