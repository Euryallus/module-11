using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class ConfirmInfoPanel : UIPanel
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private GameObject      cancelButtonGameObj;
    [SerializeField] private TextMeshProUGUI button1Text;
    [SerializeField] private TextMeshProUGUI button2Text;

    public event Action ConfirmButtonPressedEvent;
    public event Action CloseButtonPressedEvent;

    public void Setup(string title, string info, bool onlyShowConfirm = false, string cancelText = "Cancel", string confirmText = "Yes")
    {
        cancelButtonGameObj.SetActive(!onlyShowConfirm);

        titleText.text   = title;
        infoText.text    = info;
        button1Text.text = cancelText;
        button2Text.text = confirmText;
    }

    public void ConfirmButton()
    {
        if(ConfirmButtonPressedEvent != null)
        {
            ConfirmButtonPressedEvent.Invoke();
        }

        AudioManager.Instance.PlaySoundEffect2D("buttonClickMain2");

        Destroy(gameObject);
    }

    public void CloseButton()
    {
        if (CloseButtonPressedEvent != null)
        {
            CloseButtonPressedEvent.Invoke();
        }

        AudioManager.Instance.PlaySoundEffect2D("buttonClickMain2");

        Destroy(gameObject);
    }
}
