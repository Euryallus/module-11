using UnityEngine;
using TMPro;
using UnityEngine.UI;

// ||=======================================================================||
// || CustomFloatPropertyPanel: Panel for displaying customisable float     ||
// ||   properties when the player is using the customisation table.        ||
// ||=======================================================================||
// || Used on prefab: Joe/UI/Customisation/CustomFloatPropertyPanel         ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public class CustomFloatPropertyPanel : MonoBehaviour
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    public TextMeshProUGUI  ValueText;      // UI text showing the value being edited
    public Button           AddButton;      // Button for increasing the float value
    public Button           SubtractButton; // Button for decreasing the float value

    #endregion
}