using UnityEngine;

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

    [SerializeField] private CanvasGroup canvasGroup;   // Canvas group for fading the panel in/out
    [SerializeField] private Animator    animator; 

    #endregion

    private bool loadDone;  // Whether the loading process is complete

    private void Awake()
    {
        canvasGroup.alpha = 0.0f;
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if(!loadDone)
        {
            if(canvasGroup.alpha < 1.0f)
            {
                canvasGroup.alpha += Time.unscaledDeltaTime * 4.0f;
            }
        }
    }

    public void LoadDone()
    {
        loadDone = true;
        animator.SetTrigger("Crack");

        Destroy(gameObject, 1.3f);
    }
}