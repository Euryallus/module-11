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

    #endregion

    private bool loadDone;  // Whether the loading process is complete

    private void Update()
    {
        if(loadDone)
        {
            // Fade the panel out when loading is done

            canvasGroup.alpha -= Time.unscaledDeltaTime * 3.0f;

            // The panel is fully faded out, destroy it as it's no longer needed
            if(canvasGroup.alpha <= 0.0f)
            {
                Destroy(gameObject);
            }
        }
    }

    public void LoadDone()
    {
        loadDone = true;
    }
}