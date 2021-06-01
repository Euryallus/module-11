using System;
using UnityEngine;

// ||=======================================================================||
// || OptionsToggle: A button with an on/off state used to toggle something ||
// ||   in an options menu.                                                 ||
// ||=======================================================================||
// || Used on prefab: Joe/UI/OptionsToggle                                  ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public class OptionsToggle : MonoBehaviour
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private GameObject toggleIcon; // The icon shown when the button is toggled on

    #endregion

    #region Properties

    public bool Selected { get { return selected; } }

    #endregion

    public event Action<bool> ToggleEvent;  // Event invoked when the button is toggled

    private bool selected;                  // Whether the button is currently selected/toggled on

    private void Awake()
    {
        // Toggles are not selected by default
        SetSelected(false);
    }

    public void Toggle()
    {
        // Invert the toggle state to select/deselect it
        SetSelected(!selected);

        // Button was toggled, invoke the ToggleEvent
        ToggleEvent.Invoke(selected);
    }

    public void SetSelected(bool selected)
    {
        // Set the selected bool to the new value
        this.selected = selected;

        // Show the toggle icon if selected, or hide it otherwise
        toggleIcon.SetActive(selected);
    }
}
