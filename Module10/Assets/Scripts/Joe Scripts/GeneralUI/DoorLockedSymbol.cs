using UnityEngine;
using UnityEngine.UI;

// ||=======================================================================||
// || DoorLockedSymbol: A padlock symbol that is shown on locked doors.     ||
// ||=======================================================================||
// || Used on prefab: Joe/UI/DoorLockedSymbol                               ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the production phase (Module 11).                                 ||
// ||=======================================================================||

public class DoorLockedSymbol : MonoBehaviour
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private CanvasGroup canvasGoup;    // Canvas group for setting the symbol's alpha
    [SerializeField] private Image       image;         // Image used to display the lock sprite
    [SerializeField] private Sprite[]    sprites;       // The possible sprites that can be shown: a red 'locked' and green 'ready to unlock' icon

    #endregion

    #region Properties

    public bool Visible { get { return visible; } }

    #endregion

    private bool showing;                   // Whether the icon should be shown
    private bool visible;                   // Whether the icon is at all visible (for example, an icon may not be 'showing', but can be in the
                                            //   process of fading out, so is still slightly visible (and as such still needs to be visually updated)

    private bool usingUnlockIcon;           // Whether the green 'ready to unlock' icon is being used (rather than the standard red padlock)

    private const float FadeSpeed = 7.0f;   // How quickly the symbol fades in/out

    private void Start()
    {
        // Hide the symbol by default
        showing = false;
        canvasGoup.alpha = 0.0f;
    }

    private void Update()
    {
        if (showing && canvasGoup.alpha < 1.0f)
        {
            // Symbol should be showing, fade it in
            canvasGoup.alpha += Time.unscaledDeltaTime * FadeSpeed;
        }
        else if(!showing && canvasGoup.alpha > 0.0f)
        {
            // Symbol should be hidden, fade it out
            canvasGoup.alpha -= Time.unscaledDeltaTime * FadeSpeed;
        }
        else if(canvasGoup.alpha == 0.0f)
        {
            // Set visible to false once the icon is fully hidden
            visible = false;
        }
    }

    public void Show()
    {
        if(!showing)
        {
            // Show the icon if not already showing

            canvasGoup.alpha = 0.0f;
            showing = true;
            visible = true;
        }
    }

    public void Hide()
    {
        if(showing)
        {
            // Hide the icon if it's currently being shown

            showing = false;
        }
    }

    public void SetIcon(bool useUnlockIcon)
    {
        if(useUnlockIcon != usingUnlockIcon)
        {
            // Change the sprite being displayed if a different icon should be shown

            image.sprite = sprites[useUnlockIcon ? 1 : 0];

            usingUnlockIcon = useUnlockIcon;
        }
    }
}
