using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

// ||=======================================================================||
// || MainMenu: Handles all behaviour for the MainMenu scene.               ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || originally for the prototype phase.                                   ||
// ||                                                                       ||
// || Changes made during the production phase (Module 11):                 ||
// ||                                                                       ||
// || - Load button now adapts depending on if the player has a saved game  ||
// || - Player now prompted to enter a name before they first start playing ||
// || - Various fixes and improvements                                      ||
// ||=======================================================================||

public class MainMenu : MonoBehaviour
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private GameObject         optionsPanelPrefab;     // The options panel GameObject instantiated when options button is clicked
    [SerializeField] private Sprite[]           logoSprites;            // Array of possible logo sprites that can be shown
    [SerializeField] private Image              logoImage;              // Image used to display the logo
    [SerializeField] private TextMeshProUGUI    playButtonText;         // Text on the play/continue button
    [SerializeField] private Canvas             mainContentCanvas;      // Canvas that contains the main menu UI
    [SerializeField] private Canvas             menusContentCanvas;     // Canvas that contains the options menu UI when it is opened
    [SerializeField] private GameObject         infoInputPanelPrefab;   // The panel that is instantiated when playing for the first time that prompts the player to enter a name

    #endregion

    private GameObject optionsPanel;    // The options panel GameObject

    private string  sceneToLoadName;    // Name of the scene that should be loaded when the play button is clicked
    private string  savedPlayerName;    // If a saved game exists, the name the player game themselves
    private string  savedAreaName;      // If a saved game exists, the name of the area the player will be placed in when the game is loaded (e.g. 'Dark Catacombs')
    private bool    startingNewGame;    // If true, a new game will be started when the play button is pressed. Otherwise a saved game will be loaded

    // Base text to be displayed on the play button when either starting a new game or continuing an existing one:
    private const string NewGameText        = "<b><size=45>New\nGame</size></b>";
    private const string ContinueGameText   = "<b><size=45>Continue</size></b>\n";

    private void Start()
    {
        // Get a random number between 0 and 99
        int rand = Random.Range(0, 100);

        // On start, there is a 99 in 100 chance that the normal logo will be shown,
        //   and a 1 in 100 chance that an alternate easter egg logo will show instead:

        if (rand > 0)
        {
            logoImage.sprite = logoSprites[0];
        }
        else
        {
            logoImage.sprite = logoSprites[1];
        }

        // Check if the player has an existing saved game
        CheckForSavedGame();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            // Hide the options panel when Esc is pressed if it's being shown
            HideOptionsPanel();
        }
    }

    private void CheckForSavedGame()
    {
        // Attempt to load game info, returns true if saved data is found, and fetches the name of the scene to load,
        //   as well as the saved player name and name of the area the player was last in

        if (SaveLoadManager.Instance.LoadGameInfo(out sceneToLoadName, out savedPlayerName, out savedAreaName))
        {
            // A saved game exists - show the player's name and the area they were last in on the play button

            startingNewGame = false;
            playButtonText.text = ContinueGameText + savedPlayerName + "\n" + savedAreaName;
        }
        else
        {
            // A saved game does not exist, show 'New Game' text on the play button

            startingNewGame = true;
            playButtonText.text = NewGameText;
        }
    }

    public void HideOptionsPanel()
    {
        if(optionsPanel != null)
        {
            // Destroy the option panel if it's open
            Destroy(optionsPanel);
            AudioManager.Instance.PlaySoundEffect2D("buttonClickMain2");

            // Show main menu content
            mainContentCanvas.enabled = true;

            // Check again for a saved game in case save data was reset in the options menu
            CheckForSavedGame();
        }
    }

    private void ShowNameInputPanel()
    {
        // Instantiate the panel that prompts the player to enter a name
        InputInfoPanel nameInputPanel = Instantiate(infoInputPanelPrefab, menusContentCanvas.transform).GetComponent<InputInfoPanel>();

        // Set panel text
        nameInputPanel.Setup("Player Name", "What is your name?");

        // OnPlayerNameConfirmed will be called if the confirm button is pressed,
        //   or OnPlayerNameInputClosed if the close/cancel button is pressed
        nameInputPanel.ConfirmButtonPressedEvent += OnPlayerNameConfirmed;
        nameInputPanel.CloseButtonPressedEvent   += OnPlayerNameInputClosed;

        // Hide the main menu UI while the input panel is showing
        mainContentCanvas.enabled = false;

        AudioManager.Instance.PlaySoundEffect2D("buttonClickMain1");
    }

    private void OnPlayerNameConfirmed(string playerName)
    {
        // A valid name was entered, store the name in PlayerStats and start loading a new game

        PlayerStats.PlayerName = playerName;

        StartLoadingGame();
    }

    private void OnPlayerNameInputClosed()
    {
        // Re-show main menu UI if the player name input panel is closed

        mainContentCanvas.enabled = true;
    }

    private void StartLoadingGame()
    {
        AudioManager.Instance.PlaySoundEffect2D("buttonClickMain2");

        // Load the default scene or the scene the player was last in
        SaveLoadManager.Instance.LoadGameScene(sceneToLoadName);
    }

    // Called by UI button click events:
    //==================================

    public void ButtonPlay()
    {
        if(startingNewGame)
        {
            // New game - prompt the player to enter a name
            ShowNameInputPanel();
        }
        else
        {
            // Load an existing game
            StartLoadingGame();
        }
    }

    public void ButtonOptions()
    {
        // Instantiate the options panel
        optionsPanel = Instantiate(optionsPanelPrefab, menusContentCanvas.transform);

        // Setup the panel so it knows to return to the main menu when closed
        optionsPanel.GetComponent<OptionsPanel>().Setup(OptionsOpenType.MainMenu);

        // Hide main menu content
        mainContentCanvas.enabled = false;

        AudioManager.Instance.PlaySoundEffect2D("buttonClickMain2");
    }

    public void ButtonCredits()
    {
        // Load the credits scene
        SceneManager.LoadScene("Credits");

        AudioManager.Instance.PlaySoundEffect2D("buttonClickMain2");
    }

    public void ButtonQuit()
    {
        Debug.Log("Quitting game (build only)");

        // Quit the game
        Application.Quit();
    }
}
