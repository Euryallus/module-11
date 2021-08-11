using UnityEngine;
using TMPro;

// ||=======================================================================||
// || CustomFloatPropertyPanel: Panel for displaying customisable float     ||
// ||   properties when the player is using the customisation table.        ||
// ||=======================================================================||
// || Used on prefab: Joe/UI/Customisation/CustomFloatPropertyPanel         ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || originally for the prototype phase.                                   ||
// ||                                                                       ||
// || Changes made during the production phase (Module 11):                 ||
// ||                                                                       ||
// || - Changed AddButton & SubtractButton from standard Unity Buttons to   ||
// ||    PressEffectButtons, allows for extra visual options/functionality. ||
// ||=======================================================================||

public class CustomFloatPropertyPanel : MonoBehaviour
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    public TextMeshProUGUI    ValueText;      // UI text showing the value being edited
    public PressEffectButton  AddButton;      // Button for increasing the float value
    public PressEffectButton  SubtractButton; // Button for decreasing the float value

    #endregion
}