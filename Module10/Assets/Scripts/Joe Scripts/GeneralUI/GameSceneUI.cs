using UnityEngine;

// ||=======================================================================||
// || GameSceneUI: (Previously GameSceneMenuUI) Handles various game UI     ||
// ||   such as the pause menu and cinematics canvas.                       ||
// ||=======================================================================||
// || Used on prefab: Joe/UI/JoeCanvas                                      ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || originally for the prototype phase.                                   ||
// ||                                                                       ||
// || Changes made during the production phase (Module 11):                 ||
// ||                                                                       ||
// || - Added the option to show/hide the cinematics canvas and main UI     ||
// ||=======================================================================||

public class GameSceneUI : MonoBehaviour
{
    public static GameSceneUI Instance; // Static instance of the class for simple access

    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private Canvas[]   canvases;               // Canvases that make up the game scene UI
    [SerializeField] private GameObject cinematicsCanvasPrefab; // Canvas to be instantiated during cinematics (see CinematicsCanvas)

    [SerializeField] private GameObject pausePanelPrefab;       // Pause UI, instantiated when the game is paused
    [SerializeField] private GameObject optionsPanelPrefab;     // Options panel UI

    [SerializeField] private GameObject npcIndicatorPrefab;     // The indicator instantiated above the head of NPCs in the scene
    [SerializeField] private Sprite[]   npcIndicatorSprites;    // All sprites that can be used on the NPC indicators

    #endregion

    #region Properties

    public InventoryPanel   PlayerInventory         { get { return playerInventory; } }
    public ItemInfoPopup    ItemInfoPopup           { get { return itemInfoPopup; } }
    public bool             ShowingCinematicsCanvas { get { return showingCinematicsCanvas; } }
    public GameObject       NPCIndicatorPrefab      { get { return npcIndicatorPrefab; } }
    public Sprite[]         NPCIndicatorSprites     { get { return npcIndicatorSprites; } }

    #endregion

    private PlayerMovement   playerMovement;            // Reference to the player movement script for disabling movement when the pause menu is open
    private PausePanel       pausePanel;                // UI panel shown when the game is paused
    private OptionsPanel     optionsPanel;              // UI panel containing options that can be edited during gameplay

    private bool             showingCinematicsCanvas;   // Whether the cinematics canvas is currently being shown
    private CinematicsCanvas cinematicsCanvas;          // The active cinematics canvas (null if one is not active)

    private InventoryPanel   playerInventory;           // Reference to the player's inventory panel

    private ItemInfoPopup    itemInfoPopup;             // Popup used for displaying info, usually about an item

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

        // Get a reference to the player's inventory panel
        FindPlayerInventory();
    }

    private void Start()
    {
        playerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();

        itemInfoPopup = GameObject.FindGameObjectWithTag("ItemInfoPopup").GetComponent<ItemInfoPopup>();
    }

    private void Update()
    {
        if(playerInventory == null)
        {
            // Re-find the inventory panel if it becomed null, can happen when switching scenes
            FindPlayerInventory();
        }

        if (!InputFieldSelection.AnyFieldSelected && !showingCinematicsCanvas && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)))
        {
            // Esc key was pressed while not editing an input field

            if (pausePanel != null && pausePanel.Showing)
            {
                // The pause panel is showing, hide it/unpause the game
                HidePauseUI();
            }
            else if (optionsPanel != null && optionsPanel.Showing)
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

    private void FindPlayerInventory()
    {
        playerInventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<InventoryPanel>();
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
        pausePanel = Instantiate(pausePanelPrefab, canvases[0].transform).GetComponent<PausePanel>();
        pausePanel.Showing = true;

        // Pause the game
        PauseGame();
    }

    public void HidePauseUI(bool unpauseGame = true)
    {
        // Destroy/hide the pause panel (if showing)
        if (pausePanel != null)
        {
            Destroy(pausePanel.gameObject);
            pausePanel = null;
        }

        // Unpause the game
        if(unpauseGame)
        {
            UnpauseGame();
        }
    }

    public void ShowOptionsUI()
    {
        // Instantiate/show the options panel
        optionsPanel = Instantiate(optionsPanelPrefab, canvases[0].transform).GetComponent<OptionsPanel>();
        optionsPanel.Showing = true;

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
            Destroy(optionsPanel.gameObject);
            optionsPanel = null;
        }
    }

    public void SetUIShowing(bool show)
    {
        // Enable/disable all canvases
        for (int i = 0; i < canvases.Length; i++)
        {
            if (canvases[i] != null)
            {
                canvases[i].enabled = show;
            }
        }

        // Also show/hides the quest manager which contains various quest related UI
        GameObject.FindGameObjectWithTag("QuestManager").GetComponent<CanvasGroup>().alpha = show ? 1.0f : 0.0f;
    }

    public void ShowCinematicsCanvas()
    {
        cinematicsCanvas = Instantiate(cinematicsCanvasPrefab).GetComponent<CinematicsCanvas>();

        showingCinematicsCanvas = true;
    }

    public void HideCinematicsCanvas()
    {
        // Destroy the cinematics canvas if one is being shown
        if (cinematicsCanvas.gameObject != null)
        {
            Destroy(cinematicsCanvas.gameObject);
        }

        showingCinematicsCanvas = false;
    }

    public CinematicsCanvas GetActiveCinematicsCanvas()
    {
        // Returns the cinematics canvas being shown, or throws an error if there is no active canvas

        if(cinematicsCanvas == null)
        {
            Debug.LogWarning("Calling GetActiveCinematicsCanvas with null cinematics canvas");
        }

        return cinematicsCanvas;
    }
}