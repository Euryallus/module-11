using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ||=======================================================================||
// || QTEPrompt: A UI prompt shown when a quick time event occurs           ||
// ||   (unused in the final game).                                         ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the production phase (Module 11).                                 ||
// ||=======================================================================||

public class QTEPrompt : MonoBehaviour
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private Image           progressImage; // Image used to show the amount of time left to complete the QTE
    [SerializeField] private TextMeshProUGUI keyText;       // Text that shows which key needs to be pressed to complete the QTE
    [SerializeField] private Animator        animator;      // Handles the animation shown when the correct key is pressed

    #endregion

    // UI Setup:

    public void SetKeyText(string text)
    {
        keyText.text = text;
    }

    public void SetIndicatorProgress(float value)
    {
        progressImage.fillAmount = value;
    }

    // Animation, called on QTE completion:

    public void PlayPressAnimation()
    {
        animator.SetTrigger("Press");
    }
}