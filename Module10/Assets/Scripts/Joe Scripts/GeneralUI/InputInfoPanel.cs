using System;
using UnityEngine;
using TMPro;

// ||=======================================================================||
// || InputInfoPanel: A panel containing some text and an input field for   ||
// ||   when the player needs to be prompted to enter some text.            ||
// ||=======================================================================||
// || Used on prefab: Joe/UI/InfoPanels/InputInfoPanel                      ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the production phase (Module 11).                                 ||
// ||=======================================================================||

public class InputInfoPanel : UIPanel
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private TextMeshProUGUI    titleText;  // Title text displayed at the top of the panel
    [SerializeField] private TextMeshProUGUI    infoText;   // Info text telling the player what text to enter
    [SerializeField] private TMP_InputField     inputField; // The input field for entering text

    #endregion

    public event Action<string> ConfirmButtonPressedEvent;  // Invoked when the confirm button is pressed
    public event Action         CloseButtonPressedEvent;    // Invoked when the cancel button is pressed

    public void Setup(string title, string info)
    {
        // Setup title/info text
        titleText.text = title;
        infoText.text = info;
    }

    // Called when the confirm button is pressed
    public void ConfirmButton()
    {
        if(!string.IsNullOrWhiteSpace(inputField.text))
        {
            // Valid input, some text has been entered by the player

            // Invoke the confirm button pressed event
            ConfirmButtonPressedEvent?.Invoke(inputField.text);

            AudioManager.Instance.PlaySoundEffect2D("buttonClickMain2");

            // Destroy the panel - it's not needed anymore
            Destroy(gameObject);
        }
    }

    // Called when the close button is pressed
    public void CloseButton()
    {
        // Invoke the close button pressed event
        if (CloseButtonPressedEvent != null)
        {
            CloseButtonPressedEvent.Invoke();
        }

        AudioManager.Instance.PlaySoundEffect2D("buttonClickMain2");

        // Destroy the panel - it's not needed anymore
        Destroy(gameObject);
    }
}