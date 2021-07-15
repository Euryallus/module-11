using System;
using UnityEngine;
using TMPro;

public class InputInfoPanel : UIPanel
{
    [SerializeField] private TextMeshProUGUI    titleText;
    [SerializeField] private TextMeshProUGUI    infoText;
    [SerializeField] private TMP_InputField     inputField;

    public event Action<string> ConfirmButtonPressedEvent;
    public event Action         CloseButtonPressedEvent;

    public void Setup(string title, string info)
    {
        titleText.text = title;
        infoText.text = info;
    }

    public void ConfirmButton()
    {
        if(!string.IsNullOrWhiteSpace(inputField.text))
        {
            if(ConfirmButtonPressedEvent != null)
            {
                ConfirmButtonPressedEvent.Invoke(inputField.text);
            }

            AudioManager.Instance.PlaySoundEffect2D("buttonClickMain2");

            Destroy(gameObject);
        }
    }

    public void CloseButton()
    {
        if(CloseButtonPressedEvent != null)
        {
            CloseButtonPressedEvent.Invoke();
        }

        AudioManager.Instance.PlaySoundEffect2D("buttonClickMain2");

        Destroy(gameObject);
    }
}