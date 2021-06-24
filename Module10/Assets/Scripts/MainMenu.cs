using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject     optionsPanelPrefab;
    [SerializeField] private Sprite[]       logoSprites;
    [SerializeField] private Image          logoImage;

    private GameObject optionsPanel;

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
    }

    // Called by UI events:
    //=====================

    public void ButtonPlay()
    {
        SaveLoadManager.Instance.LoadGame();
    }

    public void ButtonOptions()
    {
        optionsPanel = Instantiate(optionsPanelPrefab, GameObject.FindGameObjectWithTag("JoeCanvas").transform);

        // Setup the panel so it knows to return to the mainS menu
        optionsPanel.GetComponent<OptionsPanel>().Setup(OptionsOpenType.MainMenu);
    }

    public void ButtonQuit()
    {
        Debug.Log("Quitting game (build only)");

        Application.Quit();
    }
}
