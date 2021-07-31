using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabButtonsGroup : MonoBehaviour
{
    [SerializeField] private string groupName;
    [SerializeField] private ResizableTabButton[] tabs;

    [SerializeField] private Color tabDefaultColour;
    [SerializeField] private Color tabSelectedColour;

    public Color TabDefaultColour  { get { return tabDefaultColour; } }
    public Color TabSelectedColour { get { return tabSelectedColour; } }

    public event Action<int, bool> TabSelectedEvent;

    private ResizableTabButton selectedTab;

    private void Start()
    {
        foreach (ResizableTabButton tab in tabs)
        {
            tab.Setup(this);
        }

        SelectTab(SaveLoadManager.Instance.GetIntFromPlayerPrefs("optionsSelectedTab"), false);
    }

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

            SaveLoadManager.Instance.SaveIntToPlayerPrefs("optionsSelectedTab", tabIndex);
        }
    }
}
