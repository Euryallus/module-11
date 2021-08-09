using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class CinematicsCanvas : MonoBehaviour
{
    [SerializeField] private CanvasGroup    fadePanelCanvGroup;
    [SerializeField] private VideoPlayer    videoPlayer;
    [SerializeField] private GameObject     videoRendererGameObj;

    private float targetFadeValue;
    private float fadeEffectSpeed;

    void Update()
    {
        fadePanelCanvGroup.alpha = Mathf.MoveTowards(fadePanelCanvGroup.alpha, targetFadeValue, Time.unscaledDeltaTime * fadeEffectSpeed);
    }

    public void FadeInFromBlack(float fadeSpeed = 1.0f)
    {
        fadePanelCanvGroup.alpha = 1.0f;
        targetFadeValue = 0.0f;

        fadeEffectSpeed = fadeSpeed;
    }

    public void FadeToBlack(float fadeSpeed = 1.0f)
    {
        fadePanelCanvGroup.alpha = 0.0f;
        targetFadeValue = 1.0f;

        fadeEffectSpeed = fadeSpeed;
    }

    public void CutToBlack()
    {
        fadePanelCanvGroup.alpha = 1.0f;
        targetFadeValue = 1.0f;
    }

    public void SetupVideoPlayer(VideoClip clipToPlay)
    {
        videoPlayer.clip = clipToPlay;
        videoPlayer.Prepare();
    }

    public void PlayVideo()
    {
        videoPlayer.Play();
        videoRendererGameObj.SetActive(true);
    }
}
