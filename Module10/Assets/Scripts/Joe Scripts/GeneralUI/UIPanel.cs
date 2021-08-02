using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ||=======================================================================||
// || UIPanel: A base class for UI panels that can be shown/hidden          ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

// Edited for Mod11: added CanShowUIPanel

[RequireComponent(typeof(CanvasGroup))]
public class UIPanel : MonoBehaviour
{
    #region Properties

    public bool Showing { get { return showing; } set { showing = value; } }

    #endregion

    public event Action UIPanelHiddenEvent;

    protected   bool        showing;                        // Whether or not the panel is currently showing
    protected   bool        isBlockingPanel = true;         // Whether this panel blocks certain other UI related input when being shown
    private     CanvasGroup canvasGroup;                    // CanvasGroup attathed to the panel

    private static List<UIPanel> uiPanels = new List<UIPanel>();    // List of all created UI panels

    protected virtual void Awake()
    {
        // Add this panel to the list of panels
        uiPanels.Add(this);
    }

    protected virtual void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnDestroy()
    {
        // Remove this panel to the list of panels if destroyed
        uiPanels.Remove(this);
    }

    public virtual void Show()
    {
        if (GameSceneUI.Instance != null && GameSceneUI.Instance.ShowingCinematicsCanvas)
        {
            return;
        }

        // Set the canvas group alpha to 1 to fully show the panel
        canvasGroup.alpha = 1.0f;

        // Allow the panel to block raycasts and hence prevent UI behind it from being interacted with
        canvasGroup.blocksRaycasts = true;

        // The panel is now showing
        showing = true;
    }

    public virtual void Hide()
    {
        // Set the canvas group alpha to 0 to fully hide the panel
        canvasGroup.alpha = 0.0f;

        // Stop the panel from blocking raycasts and hence allow UI behind it to be interacted with
        canvasGroup.blocksRaycasts = false;

        // Wait a frame, them mark the panel as hidden
        //  (Waiting a frame first so if multiple panels are opened/closed with esc, they will not all be triggered on the same frame)
        StartCoroutine(HideAfterFrame());

        if(UIPanelHiddenEvent != null)
        {
            UIPanelHiddenEvent.Invoke();
        }
    }

    private IEnumerator HideAfterFrame()
    {
        yield return null;
        showing = false;
    }

    public static bool AnyBlockingPanelShowing(string ignorePanelName = "")
    {
        // Returns true if any UIPanels where isBlockingPanel = true are currently being shown

        // Loop through all UIPanels
        for (int i = 0; i < uiPanels.Count; i++)
        {
            if(uiPanels[i].showing && uiPanels[i].isBlockingPanel && uiPanels[i].gameObject.name != ignorePanelName)
            {
                // Found a blocking panel that is showing

                Debug.Log("Checking AnyBlockingPanelShowing, " + uiPanels[i].gameObject.name + " is showing");
                return true;
            }
        }

        // No blocking panels being shown
        return false;
    }

    public static bool CanShowUIPanel()
    {
        return (!InputFieldSelection.AnyFieldSelected) && (!GameSceneUI.Instance.ShowingCinematicsCanvas) && (!AnyBlockingPanelShowing()) && Time.timeScale > 0.0f;
    }
}