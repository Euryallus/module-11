using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QTEPrompt : MonoBehaviour
{
    [SerializeField] private Image           progressImage;
    [SerializeField] private TextMeshProUGUI keyText;
    [SerializeField] private Animator        animator;

    public void SetKeyText(string text)
    {
        keyText.text = text;
    }

    public void SetIndicatorProgress(float value)
    {
        progressImage.fillAmount = value;
    }

    public void PlayPressAnimation()
    {
        animator.SetTrigger("Press");
    }
}