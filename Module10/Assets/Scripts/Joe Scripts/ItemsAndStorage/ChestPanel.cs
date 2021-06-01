using System.Collections.Generic;
using UnityEngine;
using TMPro;

// ||=======================================================================||
// || ChestPanel: A panel that displays the contents of a chest.            ||
// ||=======================================================================||
// || Used on prefab: Joe/UI/Chest Panel                                    ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public class ChestPanel : UIPanel
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] protected TextMeshProUGUI          chestNameText;  // UI text for displaying the chest name
    [SerializeField] protected List<ContainerSlotUI>    slotsUI;        // The slots for displaying items

    #endregion

    #region Properties

    public List<ContainerSlotUI> SlotsUI { get { return slotsUI; } }

    #endregion

    protected override void Start()
    {
        base.Start();

        // Hide the panel by default
        Hide();
    }

    public void Show(bool linkedChest)
    {
        // Set the chest name text based on whether it's a linked chest
        if (linkedChest)
        {
            chestNameText.text = "Linked Chest";
        }
        else
        {
            chestNameText.text = "Chest";
        }

        // Show the UI panel
        base.Show();
    }

    public override void Show()
    {
        // The base UI panel show function should not be used for chest panels, see above function
        Debug.LogError("Should not use default Show function for ChestPanel - use overload that takes a linkedChest bool instead");
    }

    private void Update()
    {
        CheckForShowHideInput();
    }

    private void CheckForShowHideInput()
    {
        // Block keyboard input if an input field is selected
        if (!InputFieldSelection.AnyFieldSelected)
        {
            // Hide the panel if I/Esc is pressed while it's open
            if (showing && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.I)))
            {
                Hide();
            }
        }
    }
}
