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

// Edited for mod11: changed AddButton & SubtractButton from standard unity buttons to PressEffectButton to allow for extra visual options/functionality

public class CustomFloatPropertyPanel : MonoBehaviour
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    public TextMeshProUGUI    ValueText;      // UI text showing the value being edited
    public PressEffectButton  AddButton;      // Button for increasing the float value
    public PressEffectButton  SubtractButton; // Button for decreasing the float value

    #endregion
}