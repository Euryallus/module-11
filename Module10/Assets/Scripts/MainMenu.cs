using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject optionsPanelPrefab;

    private GameObject optionsPanel;

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
        SceneManager.LoadScene("Mod10SubmissionDemo");
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
