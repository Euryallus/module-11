using UnityEngine;
using UnityEngine.SceneManagement;

// ||=======================================================================||
// || PausePanel: UI panel shown when the game is paused.                   ||
// ||=======================================================================||
// || Used on prefab: Joe/UI/PausePanel                                     ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || originally for the prototype phase.                                   ||
// ||                                                                       ||
// || Changes made during the production phase (Module 11):                 ||
// ||                                                                       ||
// || - Now inherits from UIPanel (fixes a number of issues).               ||
// ||=======================================================================||

public class PausePanel : UIPanel
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private CanvasGroup mainCanvasGroup;        // Canvas group on the main panel with pause option buttons (resume/menu/quit etc.)
    [SerializeField] private CanvasGroup menuConfirmCanvasGroup; // Canvas group on the panel for confirming the choice to exit to the main menu
    [SerializeField] private CanvasGroup quitConfirmCanvasGroup; // Canvas group on the panel for confirming the choice to quit the game

    #endregion

    protected override void Start()
    {
        base.Start();

        // Show just the main panel by default
        ShowMainPanelOnly();
    }

    public void ButtonResume()
    {
        // Hide this pause panel to return to gameplay

        GameSceneUI.Instance.HidePauseUI();
    }

    public void ButtonOptions()
    {
        // Show the options menu UI
        GameSceneUI.Instance.ShowOptionsUI();

        // Hide this pause panel
        GameSceneUI.Instance.HidePauseUI(false);
    }


    public void ShowMainPanelOnly()
    {
        // Show main buttons and hide main menu/quit confirm panels
        SetMainCanvasGroupShowing(true);
        SetMenuCanvasGroupShowing(false);
        SetQuitCanvasGroupShowing(false);

        AudioManager.Instance.PlaySoundEffect2D("buttonClickMain1");
    }

    public void ButtonMainMenu()
    {
        // Hide main buttons and show the main menu confirmation popup
        SetMainCanvasGroupShowing(false);
        SetMenuCanvasGroupShowing(true);

        AudioManager.Instance.PlaySoundEffect2D("buttonClickMain1");
    }

    public void ButtonQuit()
    {
        // Hide main buttons and show the quit game confirmation popup
        SetMainCanvasGroupShowing(false);
        SetQuitCanvasGroupShowing(true);

        AudioManager.Instance.PlaySoundEffect2D("buttonClickMain1");
    }

    public void ButtonMainMenuConfirm()
    {
        AudioManager.Instance.PlaySoundEffect2D("buttonClickMain1");

        // Stop any looping sounds so they don't continue to play in the menu scene
        AudioManager.Instance.StopAllLoopingSoundEffects();

        // Reset timeScale to its default value
        Time.timeScale = 1.0f;

        // Switch to main menu scene
        SceneManager.LoadScene("MainMenu");
    }

    public void ButtonQuitConfirm()
    {
        Debug.Log("Quitting game (build only)");

        AudioManager.Instance.PlaySoundEffect2D("buttonClickMain1");

        // Quit the game
        Application.Quit();
    }


    private void SetMainCanvasGroupShowing(bool show)
    {
        // Show/hide the main panel and allow/disallow button clicks and blocking other UI
        mainCanvasGroup.alpha = show ? 1.0f : 0.0f;
        mainCanvasGroup.blocksRaycasts = show;
    }

    private void SetMenuCanvasGroupShowing(bool show)
    {
        // Show/hide the menu confirm panel and allow/disallow button clicks and blocking other UI
        menuConfirmCanvasGroup.alpha = show ? 1.0f : 0.0f;
        menuConfirmCanvasGroup.blocksRaycasts = show;
    }

    private void SetQuitCanvasGroupShowing(bool show)
    {
        // Show/hide the quit confirm panel and allow/disallow button clicks and blocking other UI
        quitConfirmCanvasGroup.alpha = show ? 1.0f : 0.0f;
        quitConfirmCanvasGroup.blocksRaycasts = show;
    }
}