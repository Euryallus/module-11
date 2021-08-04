using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum DoorSymbolIcon
{
    Locked = 0,
    ReadyToUnlock
}

public class DoorLockedSymbol : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGoup;
    [SerializeField] private Image       image;
    [SerializeField] private Sprite[]    sprites;

    public bool Visible { get { return visible; } }

    private bool showing;
    private bool visible;
    private DoorSymbolIcon currentIconType;

    private const float FadeSpeed = 7.0f;

    private void Start()
    {
        showing = false;
        canvasGoup.alpha = 0.0f;
    }

    private void Update()
    {
        if (showing && canvasGoup.alpha < 1.0f)
        {
            canvasGoup.alpha += Time.unscaledDeltaTime * FadeSpeed;
        }
        else if(!showing && canvasGoup.alpha > 0.0f)
        {
            canvasGoup.alpha -= Time.unscaledDeltaTime * FadeSpeed;
        }
        else if(canvasGoup.alpha == 0.0f)
        {
            visible = false;
        }
    }

    public void Show()
    {
        if(!showing)
        {
            canvasGoup.alpha = 0.0f;
            showing = true;
            visible = true;
        }
    }

    public void Hide()
    {
        if(showing)
        {
            showing = false;
        }
    }

    public void SetIcon(DoorSymbolIcon iconType)
    {
        if(currentIconType != iconType)
        {
            image.sprite = sprites[(int)iconType];
        }
    }
}
