using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ||=======================================================================||
// || LoadingPanel: UI Panel shown when loading the game.                   ||
// ||=======================================================================||
// || Used on prefab: Joe/UI/LoadingPanel                                   ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public class LoadingPanel : MonoBehaviour
{
    #region InspectorVariables
    // Variables in this region are set in the inspector. See tooltips for more info.

    [SerializeField] private CanvasGroup        canvasGroup;   // Canvas group for fading the panel in/out
    [SerializeField] private Animator           animator;
    [SerializeField] private Slider             loadProgressSlider;
    [SerializeField] private TextMeshProUGUI    areaNameText;
    [SerializeField] private Image              areaPreviewImage;

    #endregion

    private float loadProgress;

    private void Awake()
    {
        canvasGroup.alpha = 0.0f;

        loadProgress = 0.0f;
        loadProgressSlider.value = 0.0f;
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        loadProgressSlider.value = Mathf.Lerp(loadProgressSlider.value, loadProgress, Time.unscaledDeltaTime * 10.0f);
    }

    public void SetAreaNameText(string name)
    {
        areaNameText.text = name + " | Loading";
    }

    public void SetAreaPreviewSprite(Sprite sprite)
    {
        areaPreviewImage.sprite = sprite;
    }

    public void UpdateLoadProgress(float value)
    {
        loadProgress = value;
    }

    public void LoadDone()
    {
        animator.SetTrigger("Crack");

        StartCoroutine(DestroyAfterDelay());
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSecondsRealtime(1.5f);

        Destroy(gameObject);
    }
}