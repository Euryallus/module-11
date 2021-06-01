using UnityEngine;
using TMPro;

// ||=======================================================================||
// || InputFieldSelection: Detects when input fields are selected and sets  ||
// ||   a public flag that can be checked to prevent other input events     ||
// ||   being triggered while the player is typing.                         ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

[RequireComponent(typeof(TMP_InputField))]
public class InputFieldSelection : MonoBehaviour
{
    public static bool AnyFieldSelected;    // Whether any input field is currently selected/being typed in
                                               
    private TMP_InputField inputField;      // The input field this script 'listens' to

    private void Awake()
    {
        inputField = GetComponent<TMP_InputField>();

        // Trigger Select/Deselect on certain input field events
        inputField.onSelect     .AddListener(Select);
        inputField.onDeselect   .AddListener(Deselect);
        inputField.onEndEdit    .AddListener(Deselect);
    }

    public void Select(string s)
    {
        // The field was selected
        AnyFieldSelected = true;
    }

    public void Deselect(string s)
    {
        // The field was deselected/is no longer being edited
        AnyFieldSelected = false;
    }

}
