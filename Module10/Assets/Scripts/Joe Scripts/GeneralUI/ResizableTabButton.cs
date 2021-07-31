using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ResizableTabButton : MonoBehaviour
{
    [SerializeField] private float      defaultWidth;
    [SerializeField] private float      selectedWidth;
    [SerializeField] private Image      backgroundImage;   
    [SerializeField] private GameObject textGameObj;   

    private RectTransform rectTransform;

    private TabButtonsGroup tabGroup;

    private float targetWidth;
    private bool animating;

    private const float AnimationSpeed = 15.0f;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if(animating)
        {
            float width = Mathf.Lerp(rectTransform.sizeDelta.x, targetWidth, Time.unscaledDeltaTime * AnimationSpeed);

            rectTransform.sizeDelta = new Vector3(width, rectTransform.sizeDelta.y, rectTransform.sizeDelta.y);

            if(Mathf.Abs(rectTransform.sizeDelta.x - targetWidth) < 0.05f)
            {
                SetToTargetWidth();
                animating = false;
            }
        }
    }

    public void Setup(TabButtonsGroup group)
    {
        tabGroup = group;

        SetSelected(false, true);
    }

    public void SetSelected(bool selected, bool snapToSize = false)
    {
        if(selected)
        {
            targetWidth = selectedWidth;
            backgroundImage.color = tabGroup.TabSelectedColour;
            textGameObj.SetActive(true);
        }
        else
        {
            targetWidth = defaultWidth;
            backgroundImage.color = tabGroup.TabDefaultColour;
            textGameObj.SetActive(false);
        }

        if (snapToSize)
        {
            SetToTargetWidth();
        }
        else
        {
            animating = true;
        }
    }

    private void SetToTargetWidth()
    {
        rectTransform.sizeDelta = new Vector3(targetWidth, rectTransform.sizeDelta.y, rectTransform.sizeDelta.y);
    }
}
