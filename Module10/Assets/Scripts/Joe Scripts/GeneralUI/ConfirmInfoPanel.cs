using UnityEngine;
using TMPro;
using System;

// ||=======================================================================||
// || ConfirmInfoPanel: A panel containing some info text and two buttons,  ||
// ||    by default a 'Cancel' and 'Yes' button. Used when the player needs ||
// ||    to confirm an action, or generally be shown some information.      ||
// ||=======================================================================||
// || Used on prefab: Joe/UI/InfoPanels/ConfirmInfoPanel                    ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the production phase (Module 11).                                 ||
// ||=======================================================================||

public class ConfirmInfoPanel : UIPanel
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private TextMeshProUGUI titleText;             // Title text displayed at the top of the panel
    [SerializeField] private TextMeshProUGUI infoText;              // Info text, the main content of the panel
    [SerializeField] private GameObject      cancelButtonGameObj;   // The cancel button that can be disabled if only one button is required
    [SerializeField] private TextMeshProUGUI button1Text;           // Text on button 1 (Cancel by default)
    [SerializeField] private TextMeshProUGUI button2Text;           // Text on button 2 (Yes by default)

    #endregion

    public event Action ConfirmButtonPressedEvent;  // Event invoked when the confirm buttton (aka button 2) is pressed
    public event Action CloseButtonPressedEvent;    // Event invoked when the close buttton (aka button 1) is pressed

    public void Setup(string title, string info, bool onlyShowConfirm = false, string cancelText = "Cancel", string confirmText = "Yes")
    {
        // Hide the cancel button if only button 2/confirm is needed
        cancelButtonGameObj.SetActive(!onlyShowConfirm);

        // Setup all text
        titleText.text   = title;
        infoText.text    = info;
        button1Text.text = cancelText;
        button2Text.text = confirmText;
    }

    public void ConfirmButton()
    {
        // Invoke the ConfirmButtonPressedEvent event, also destroy the panel as it's no longer needed

        if (ConfirmButtonPressedEvent != null)
        {
            ConfirmButtonPressedEvent.Invoke();
        }

        AudioManager.Instance.PlaySoundEffect2D("buttonClickMain2");

        Destroy(gameObject);
    }

    public void CloseButton()
    {
        // Invoke the CloseButtonPressedEvent event, also destroy the panel as it's no longer needed

        if (CloseButtonPressedEvent != null)
        {
            CloseButtonPressedEvent.Invoke();
        }

        AudioManager.Instance.PlaySoundEffect2D("buttonClickMain2");

        Destroy(gameObject);
    }
}
