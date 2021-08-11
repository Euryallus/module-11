using UnityEngine;
using UnityEngine.UI;

// ||=======================================================================||
// || ResizableTabButton: A tab that can be selected as part of a           ||
// ||   TabButtonsGroup. Tabs change width when selected/deselected.        ||
// ||=======================================================================||
// || Used on various prefabs.                                              ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the production phase (Module 11).                                 ||
// ||=======================================================================||

[RequireComponent(typeof(RectTransform))]
public class ResizableTabButton : MonoBehaviour
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private float      defaultWidth;       // The standard width of the tab when not selected
    [SerializeField] private float      selectedWidth;      // The width of the tab when selected
    [SerializeField] private Image      backgroundImage;    // The image that creates the background colour of the tab
    [SerializeField] private GameObject textGameObj;        // The text displayed on the tab

    #endregion

    private TabButtonsGroup tabGroup;           // The group this tab belongs to
    private RectTransform   rectTransform;      // The RectTransform component used for resizing the tab

    private float           targetWidth;        // The width to grow/shrink towards
    private bool            animating;          // Whether the tab is currently changing size

    private const float AnimationSpeed = 15.0f; // Speed of the resize animation

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if(animating)
        {
            // Tab is currently changing size

            // Lerp towards the target width value
            float width = Mathf.Lerp(rectTransform.sizeDelta.x, targetWidth, Time.unscaledDeltaTime * AnimationSpeed);

            // Set the width of the RectTransform based on the value calculated above
            rectTransform.sizeDelta = new Vector3(width, rectTransform.sizeDelta.y, rectTransform.sizeDelta.y);

            if(Mathf.Abs(rectTransform.sizeDelta.x - targetWidth) < 0.05f)
            {
                // The tab is very close to its target width, snap to the target width and stop animating

                SetToTargetWidth();
                animating = false;
            }
        }
    }

    public void Setup(TabButtonsGroup group)
    {
        tabGroup = group;

        // Deselect the tab by default, snapping straight to the default width
        SetSelected(false, true);
    }

    public void SetSelected(bool selected, bool snapToSize = false)
    {
        if(selected)
        {
            // The tab was selected
            // Show text, set the tab to the selected colour, and the target width to the selectedWidth value

            targetWidth = selectedWidth;
            backgroundImage.color = tabGroup.TabSelectedColour;
            textGameObj.SetActive(true);
        }
        else
        {
            // The tab was deselected
            // Hide text, set the tab to the standard colour, and the target width to the default value

            targetWidth = defaultWidth;
            backgroundImage.color = tabGroup.TabDefaultColour;
            textGameObj.SetActive(false);
        }

        if (snapToSize)
        {
            // Snap straight to the target width
            SetToTargetWidth();
            animating = false;
        }
        else
        {
            // Start animating towards the target width
            animating = true;
        }
    }

    private void SetToTargetWidth()
    {
        // Instantly sets the tab to the target width
        rectTransform.sizeDelta = new Vector3(targetWidth, rectTransform.sizeDelta.y, rectTransform.sizeDelta.y);
    }
}
