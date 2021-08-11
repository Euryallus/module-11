using UnityEngine;
using UnityEngine.Video;

// ||=======================================================================||
// || CinematicsCanvas: The canvas that is shown during cutscenes when      ||
// ||   a fade effect, video or any other UI overlay is needed.             ||
// ||=======================================================================||
// || Used on prefab: Joe/UI/CinematicsCanvas                               ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the production phase (Module 11).                                 ||
// ||=======================================================================||

public class CinematicsCanvas : MonoBehaviour
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private CanvasGroup fadePanelCanvGroup;    // Canvas group that controls the alpha of panels used for fade in/out effects
    [SerializeField] private VideoPlayer videoPlayer;           // For playing a video cutscene
    [SerializeField] private GameObject  videoRendererGameObj;  // GameObject containing a render texture that displays the output of videoPlayer

    #endregion

    private float targetFadeValue;  // Value to fade towards
    private float fadeEffectSpeed;  // How quickly to fade

    void Update()
    {
        if(fadePanelCanvGroup.alpha != targetFadeValue)
        {
            // Fade towards the target value
            fadePanelCanvGroup.alpha = Mathf.MoveTowards(fadePanelCanvGroup.alpha, targetFadeValue, Time.unscaledDeltaTime * fadeEffectSpeed);
        }
    }

    public void FadeInFromBlack(float fadeSpeed = 1.0f)
    {
        // Start fading from 1.0 (black) to 1.0 (fully transparent) over fadeSpeed seconds

        fadePanelCanvGroup.alpha = 1.0f;
        targetFadeValue = 0.0f;

        fadeEffectSpeed = fadeSpeed;
    }

    public void FadeToBlack(float fadeSpeed = 1.0f)
    {
        // Start fading from 0.0 (fully transparent) to 1.0 (black) over fadeSpeed seconds

        fadePanelCanvGroup.alpha = 0.0f;
        targetFadeValue = 1.0f;

        fadeEffectSpeed = fadeSpeed;
    }

    public void CutToBlack()
    {
        // Instantly show a black cover
        
        fadePanelCanvGroup.alpha = 1.0f;
        targetFadeValue = 1.0f;
    }

    public void SetupVideoPlayer(VideoClip clipToPlay)
    {
        // Prepare the video player ready to play the given video clip

        videoPlayer.clip = clipToPlay;
        videoPlayer.Prepare();
    }

    public void PlayVideo()
    {
        // Play the video player and show the renderer that will display the video

        videoPlayer.Play();
        videoRendererGameObj.SetActive(true);
    }
}
