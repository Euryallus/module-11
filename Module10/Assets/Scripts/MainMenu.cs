using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject         optionsPanelPrefab;
    [SerializeField] private Sprite[]           logoSprites;
    [SerializeField] private Image              logoImage;
    [SerializeField] private TextMeshProUGUI    playButtonText;
    [SerializeField] private Canvas             mainContentCanvas;
    [SerializeField] private Canvas             menusContentCanvas;
    [SerializeField] private GameObject         infoInputPanelPrefab;

    private GameObject optionsPanel;

    private string  sceneToLoadName;
    private string  savedPlayerName;
    private string  savedAreaName;
    private bool    startingNewGame;

    private const string NewGameText        = "<b><size=45>New\nGame</size></b>";
    private const string ContinueGameText   = "<b><size=45>Continue</size></b>\n";

    private void Start()
    {
        int rand = Random.Range(0, 100);

        if(rand > 0)
        {
            logoImage.sprite = logoSprites[0];
        }
        else
        {
            logoImage.sprite = logoSprites[1];
        }

        playButtonText.text = NewGameText;

        if(SaveLoadManager.Instance.LoadGameInfo(out sceneToLoadName, out savedPlayerName, out savedAreaName))
        {
            startingNewGame = false;
            playButtonText.text = ContinueGameText + savedPlayerName + "\n" + savedAreaName;
        }
        else
        {
            startingNewGame = true;
            playButtonText.text = NewGameText;
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            HideOptionsPanel();
        }
    }

    public void HideOptionsPanel()
    {
        if(optionsPanel != null)
        {
            Destroy(optionsPanel);
            AudioManager.Instance.PlaySoundEffect2D("buttonClickMain2");
        }

        // Show main menu content
        mainContentCanvas.enabled = true;
    }

    // Called by UI events:
    //=====================

    public void ButtonPlay()
    {
        if(startingNewGame)
        {
            ShowNameInputPanel();
        }
        else
        {
            StartLoadingGame();
        }
    }

    private void ShowNameInputPanel()
    {
        InputInfoPanel nameInputPanel = Instantiate(infoInputPanelPrefab, menusContentCanvas.transform).GetComponent<InputInfoPanel>();

        nameInputPanel.Setup("Player Name", "What will your name be?");

        nameInputPanel.ConfirmButtonPressedEvent += OnPlayerNameConfirmed;
        nameInputPanel.CloseButtonPressedEvent += OnPlayerNameInputClosed;

        mainContentCanvas.enabled = false;

        AudioManager.Instance.PlaySoundEffect2D("buttonClickMain1");
    }

    private void OnPlayerNameConfirmed(string playerName)
    {
        PlayerStats.PlayerName = playerName;

        StartLoadingGame();
    }

    private void OnPlayerNameInputClosed()
    {
        mainContentCanvas.enabled = true;
    }

    private void StartLoadingGame()
    {
        AudioManager.Instance.PlaySoundEffect2D("buttonClickMain2");

        SaveLoadManager.Instance.LoadGameScene(sceneToLoadName);
    }

    public void ButtonOptions()
    {
        optionsPanel = Instantiate(optionsPanelPrefab, menusContentCanvas.transform);

        // Setup the panel so it knows to return to the mainS menu
        optionsPanel.GetComponent<OptionsPanel>().Setup(OptionsOpenType.MainMenu);

        // Hide main menu content
        mainContentCanvas.enabled = false;
    }

    public void ButtonCredits()
    {
        // Show credits

        AudioManager.Instance.PlaySoundEffect2D("buttonClickMain2");
    }

    public void ButtonQuit()
    {
        Debug.Log("Quitting game (build only)");

        Application.Quit();
    }
}
