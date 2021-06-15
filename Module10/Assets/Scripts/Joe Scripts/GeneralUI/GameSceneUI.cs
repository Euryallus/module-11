using UnityEngine;

// ||=======================================================================||
// || GameSceneMenuUI: Handles triggering the pause and options menus       ||
// ||   in a game scene (a scene where gameplay happens as opposed to a     ||
// ||   main menu etc).                                                     ||
// ||=======================================================================||
// || Used on prefab: Joe/UI/JoeCanvas                                      ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

// Edited for mod11:
// - added cinematics stuff
// - changed name from GameSceneMenuUI to GameSceneUI

public class GameSceneUI : MonoBehaviour
{
    public static GameSceneUI Instance; // Static instance of the class for simple access

    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private Canvas[] canvases;
    [SerializeField] private GameObject cinematicsCanvasPrefab;

    [SerializeField] private GameObject pausePanelPrefab;
    [SerializeField] private GameObject optionsPanelPrefab;

    #endregion

    #region Properties

    public bool ShowingCinematicsCanvas { get { return showingCinematicsCanvas; } }

    #endregion

    private PlayerMovement   playerMovement;             // Reference to the player movement script for disabling movement when the pause menu is open
    private GameObject       pausePanel;                 // UI panel shown when the game is paused
    private GameObject       optionsPanel;               // UI panel containing options that can be edited during gameplay

    private bool             pausePanelShowing;         // Whether the pause panel is currently being shown
    private bool             optionsPanelShowing;       // Whether the options panel is currently being shown

    private bool             showingCinematicsCanvas;
    private CinematicsCanvas cinematicsCanvas;

    private void Awake()
    {
        // Ensure that an instance of the class does not already exist
        if (Instance == null)
        {
            // Set this class as the instance
            Instance = this;
        }
        // If there is an existing instance that is not this, destroy the GameObject this script is connected to
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        playerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if(!InputFieldSelection.AnyFieldSelected && Input.GetKeyDown(KeyCode.Escape) && !showingCinematicsCanvas)
        {
            // Esc key was pressed while not editing an input field

            if (pausePanelShowing)
            {
                // The pause panel is showing, hide it/unpause the game
                HidePauseUI();
            }
            else if (optionsPanelShowing)
            {
                // The options pane is showing, hide it/unpause the game
                HideOptionsUI();
                UnpauseGame();
            }
            else if (!UIPanel.AnyBlockingPanelShowing() && Time.timeScale > 0.0f)
            {
                // Otherwise show the pause panel if no other blocking UI panel is open, and time
                //   is not already paused (happens in certain situiations such as when the player dies)
                PauseAndShowPauseUI();
            }
        }
    }

    private void PauseGame()
    {
        // Stop player movement
        playerMovement.StopMoving();

        // Show the cursor and allow it to be moved around so the player can interact with UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Pause time to prevent any movement of other objects
        Time.timeScale = 0.0f;

        // Play a click sound
        AudioManager.Instance.PlaySoundEffect2D("buttonClickMain1");
    }

    private void UnpauseGame()
    {
        // Re-enable player movement
        playerMovement.StartMoving();

        // Lock the cursor to the centre of the screen for controlling the camera
        Cursor.lockState = CursorLockMode.Locked;

        // Set timeScale back to its default value
        Time.timeScale = 1.0f;

        // Play a click sound
        AudioManager.Instance.PlaySoundEffect2D("buttonClickMain2");
    }

    public void PauseAndShowPauseUI()
    {
        // Instantiate/show the pause panel
        pausePanel = Instantiate(pausePanelPrefab, canvases[0].transform);
        pausePanelShowing = true;

        // Pause the game
        PauseGame();
    }

    public void HidePauseUI(bool unpauseGame = true)
    {
        // Destroy/hide the pause panel (if showing)
        if (pausePanel != null)
        {
            Destroy(pausePanel);
        }
        pausePanelShowing = false;

        // Unpause the game
        if(unpauseGame)
        {
            UnpauseGame();
        }
    }

    public void ShowOptionsUI()
    {
        // Instantiate/show the options panel
        optionsPanel = Instantiate(optionsPanelPrefab, canvases[0].transform);
        optionsPanelShowing = true;

        // Setup the panel so it knows to return to the game pause menu
        optionsPanel.GetComponent<OptionsPanel>().Setup(OptionsOpenType.GameScenePause);

        // Play a click sound
        AudioManager.Instance.PlaySoundEffect2D("buttonClickMain1");
    }

    public void HideOptionsUI()
    {
        // Destroy/hide the options panel (if showing)
        if (optionsPanel != null)
        {
            Destroy(optionsPanel);
        }
        optionsPanelShowing = false;
    }

    public void SetUIShowing(bool show)
    {
        for (int i = 0; i < canvases.Length; i++)
        {
            if (canvases[i] != null)
            {
                canvases[i].enabled = show;
            }
        }
    }

    public void ShowCinematicsCanvas()
    {
        cinematicsCanvas = Instantiate(cinematicsCanvasPrefab).GetComponent<CinematicsCanvas>();

        showingCinematicsCanvas = true;
    }

    public void HideCinematicsCanvas()
    {
        if (cinematicsCanvas.gameObject != null)
        {
            Destroy(cinematicsCanvas.gameObject);
        }

        showingCinematicsCanvas = false;
    }

    public CinematicsCanvas GetActiveCinematicsCanvas()
    {
        if(cinematicsCanvas == null)
        {
            Debug.LogWarning("Calling GetActiveCinematicsCanvas with null cinematics canvas");
        }

        return cinematicsCanvas;
    }
}