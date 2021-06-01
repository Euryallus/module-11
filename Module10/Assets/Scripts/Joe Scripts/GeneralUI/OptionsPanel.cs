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

public class OptionsPanel : MonoBehaviour
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private TextMeshProUGUI musicVolumeText;        // Text displaying music volume title/percentage
    [SerializeField] private TextMeshProUGUI soundEffectsVolumeText; // Text displaying sound effects volume title/percentage

    [SerializeField] private Slider musicSlider;                // Slider for adjusting music volume
    [SerializeField] private Slider soundEffectsSlider;         // Slider for adjusting sound effects volume

    [SerializeField] private OptionsToggle screenShakeToggle;   // Toggle for enabling/disabling the screen shake effect
    [SerializeField] private OptionsToggle viewBobbingToggle;   // Toggle for enabling/disabling the view bobbing effect

    #endregion

    private OptionsOpenType openType;   //Determines where the panel was opened from and where to return to (see OptionsOpenType)

    private void Start()
    {
        // Subscribe to toggle events for all OptionsToggles to values can be saved when they are clicked
        screenShakeToggle.ToggleEvent += ScreenShakeToggled;
        viewBobbingToggle.ToggleEvent += ViewBobbingToggled;
    }

    public void Setup(OptionsOpenType openType)
    {
        this.openType = openType;

        SaveLoadManager slm = SaveLoadManager.Instance;

        // Update UI to show the current saved values for all options:

        // Slider options
        UpdateMusicVolumeUI       (slm.GetIntFromPlayerPrefs("musicVolume"));
        UpdateSoundEffectsVolumeUI(slm.GetIntFromPlayerPrefs("soundEffectsVolume"));

        // Toggle options
        screenShakeToggle.SetSelected(slm.GetBoolFromPlayerPrefs("screenShake"));
        viewBobbingToggle.SetSelected(slm.GetBoolFromPlayerPrefs("viewBobbing"));
    }

    private void UpdateMusicVolumeUI(int savedMusicVolume)
    {
        // Set the value of the slider and display the new music volume as a percentage

        musicSlider.value = savedMusicVolume;
        musicVolumeText.text = "Music Volume (" + (savedMusicVolume * 5) + "%)";

        // Update the volume of all active music sources
        AudioManager.Instance.UpdateMusicSourcesVolume(savedMusicVolume);
    }

    private void UpdateSoundEffectsVolumeUI(int savedSoundEffectsVolume)
    {
        soundEffectsSlider.value = savedSoundEffectsVolume;
        soundEffectsVolumeText.text = "Sound Effects Volume (" + (savedSoundEffectsVolume * 5) + "%)";

        // Update the volume of all active looping sound sources (volume for standard sound effects is determined when they are played,
        //   but looping sound volume needs to be adjusted dynamically since volume may be changed while they are playing)
        AudioManager.Instance.UpdateActiveLoopingSoundsVolume(savedSoundEffectsVolume);
    }

    private void ScreenShakeToggled(bool enabled)
    {
        // Save the new value for whether screen shake is enabled
        SaveLoadManager.Instance.SaveBoolToPlayerPrefs("screenShake", enabled);

        AudioManager.Instance.PlaySoundEffect2D("buttonClickMain1");
    }

    private void ViewBobbingToggled(bool enabled)
    {
        // Save the new value for whether view bobbing is enabled
        SaveLoadManager.Instance.SaveBoolToPlayerPrefs("viewBobbing", enabled);

        AudioManager.Instance.PlaySoundEffect2D("buttonClickMain1");
    }

    // Called by UI events:
    //=====================

    public void ButtonReturn()
    {
        if(openType == OptionsOpenType.GameScenePause)
        {
            // Hide this options panel and re-show the pause menu
            GameSceneMenuUI.Instance.PauseAndShowPauseUI();
            GameSceneMenuUI.Instance.HideOptionsUI();
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

        // Play a click sound
        AudioManager.Instance.PlaySoundEffect2D("buttonClickMain1");
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

        // Play a click sound
        AudioManager.Instance.PlaySoundEffect2D("buttonClickMain1");
    }
}
