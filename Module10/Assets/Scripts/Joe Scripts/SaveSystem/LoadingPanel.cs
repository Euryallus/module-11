using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ||=======================================================================||
// || LoadingPanel: UI Panel shown when loading the game.                   ||
// ||=======================================================================||
// || Used on prefab: Joe/UI/LoadingPanel                                   ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || originally for the prototype phase.                                   ||
// ||                                                                       ||
// || Changes made during the production phase (Module 11):                 ||
// ||                                                                       ||
// || - Now displays a progress bar, the name of the area being loaded      ||
// ||    and a preview image of that area.                                  ||
// ||=======================================================================||

public class LoadingPanel : MonoBehaviour
{
    #region InspectorVariables
    // Variables in this region are set in the inspector. See tooltips for more info.

    [SerializeField] private CanvasGroup        canvasGroup;        // Canvas group for fading the panel in/out
    [SerializeField] private Animator           animator;           // Handles all load screen animations
    [SerializeField] private Slider             loadProgressSlider; // The slider used to show loading progress
    [SerializeField] private TextMeshProUGUI    areaNameText;       // Displays the name of the area being loaded
    [SerializeField] private Image              areaPreviewImage;   // Displays an image preview of the area being loaded

    #endregion

    private float loadProgress;             // How close loading is to completion (0.0 - 1.0)
    private float loadTimer;                // Keeps track of how many seconds loading has taken

    private const float MinShowTime = 2.0f; // The minumum amount of time the loading panel can be shown for (seconds)

    private void Awake()
    {
        // Hide the panel by default
        canvasGroup.alpha = 0.0f;

        // Reset load progress values
        loadProgress = 0.0f;
        loadProgressSlider.value = 0.0f;
    }

    private void Start()
    {
        // The loading panel needs to not be destroyed between scenes since its
        //   purpose is to cover the screen while switching scenes and loading data
        DontDestroyOnLoad(gameObject);

        // Trigger the loading music after a short delay to give
        //   the current background music time to fade out first
        StartCoroutine(TriggerLoadMusicAfterDelay());
    }

    private IEnumerator TriggerLoadMusicAfterDelay()
    {
        // Wait 0.7 seconds before stopping all active sounds and playing the looping load music
        yield return new WaitForSecondsRealtime(0.7f);

        AudioManager.Instance.StopAllLoopingSoundEffects();
        AudioManager.Instance.PlayMusicalLoopingSoundEffect("loadLoop", "loadLoop");
    }

    private void Update()
    {
        // Increment the load timer
        loadTimer += Time.unscaledDeltaTime;

        // Update the slider to smoothly move between its current value and the loading progress
        loadProgressSlider.value = Mathf.Lerp(loadProgressSlider.value, loadProgress, Time.unscaledDeltaTime * 10.0f);
    }

    public void SetAreaNameText(string name)
    {
        // Shows the given area name and loading text
        areaNameText.text = name + " | Loading";
    }

    public void SetAreaPreviewSprite(Sprite sprite)
    {
        // Sets the sprite used for the area preview image
        areaPreviewImage.sprite = sprite;
    }

    public void UpdateLoadProgress(float value)
    {
        loadProgress = value;
    }

    public void LoadDone()
    {
        // If loading took less time than MinShowTime, add the remaining time as a delay (no delay by default)
        float loadDoneDelay = 0.0f;

        if (loadTimer < MinShowTime)
        {
            loadDoneDelay = MinShowTime - loadTimer;
        }

        // Trigger loading done events after the delay calculated above
        StartCoroutine(LoadDoneEvents(loadDoneDelay));
    }

    public IEnumerator LoadDoneEvents(float initialDelay)
    {
        // Wait for any initial delay (see function above)
        yield return new WaitForSecondsRealtime(initialDelay);

        // Animate the load panel 'cracking' to reveal the loaded scene
        animator.SetTrigger("Crack");

        // Wait until the crack animation is complete
        yield return new WaitForSecondsRealtime(0.78f);

        // Stop the looping load music and play the load end sound
        AudioManager.Instance.StopLoopingSoundEffect("loadLoop");
        AudioManager.Instance.PlayMusicalSoundEffect("loadEnd");

        // Another short delay to give the loadEnd music time to play before fading back in background music
        yield return new WaitForSecondsRealtime(1.2f);

        // Fade in standard music/sounds
        AudioManager.Instance.FadeGlobalVolumeMultiplier(1.0f, 1.5f);

        // Loading is done, destroy the panel
        Destroy(gameObject);
    }
}