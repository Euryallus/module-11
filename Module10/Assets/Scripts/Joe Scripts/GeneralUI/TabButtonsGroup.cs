using System;
using UnityEngine;

// ||=======================================================================||
// || TabButtonsGroup: Handles a collection of ResizableTabs that can be    ||
// ||   toggled between and used to trigger other UI events.                ||
// ||=======================================================================||
// || Used on various prefabs.                                              ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the production phase (Module 11).                                 ||
// ||=======================================================================||

public class TabButtonsGroup : MonoBehaviour
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private string                 groupName;          // Unique name for this tab group (used for saving)
    [SerializeField] private ResizableTabButton[]   tabs;               // All tabs in the group
    [SerializeField] private Color                  tabDefaultColour;   // Colour for unselected tabs
    [SerializeField] private Color                  tabSelectedColour;  // Colour for selected tabs

    #endregion

    #region Properties

    public Color TabDefaultColour  { get { return tabDefaultColour; } }
    public Color TabSelectedColour { get { return tabSelectedColour; } }

    #endregion

    public  event Action<int, bool> TabSelectedEvent;   // Invoked when a tab is selected. Parameters: tab index (int), player selected (bool)

    private ResizableTabButton      selectedTab;        // The tab that is currently selected

    private void Start()
    {
        // Setup all tabs in the group on start
        foreach (ResizableTabButton tab in tabs)
        {
            tab.Setup(this);
        }

        // Select the tab that was selected last time this group was used (defaults to index 0 if never used)
        SelectTab(SaveLoadManager.Instance.GetIntFromPlayerPrefs(groupName + "_selectedTab"), false);
    }

    // Called by individual tabs when clicked
    public void SelectTabButton(int tabIndex)
    {
        SelectTab(tabIndex, true);
    }

    private void SelectTab(int tabIndex, bool playerSelected)
    {
        if(selectedTab != tabs[tabIndex])
        {
            // A new tab was selected

            // Deselect the previous tab if there was one
            if (selectedTab != null)
            {
                selectedTab.SetSelected(false, !playerSelected);
            }

            // Set the new tab as the selected tab
            selectedTab = tabs[tabIndex];
            selectedTab.SetSelected(true, !playerSelected);

            // Trigger the tab selected event
            TabSelectedEvent.Invoke(tabIndex, playerSelected);

            // Save the selected tab index
            SaveLoadManager.Instance.SaveIntToPlayerPrefs(groupName + "_selectedTab", tabIndex);
        }
    }
}
