using System.Collections;
using UnityEngine;

// ||=======================================================================||
// || DestroyAfterTime: Automatically destroys any GameObject it's          ||
// ||   attached to after a set delay (more reliable than having to call    ||
// ||   Destroy(object, time) from another script which can cause issues    ||
// ||   if that script is destroyed itself before the delay).               ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public class DestroyAfterTime : MonoBehaviour
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private float  delay = 1.0f;   // Seconds before the object is destroyed
    [SerializeField] private bool   unscaledTime;   // Whether to use unscaled time for the delay (i.e. keep counting when the game is paused/time is slowed)

    #endregion

    void Start()
    {
        StartCoroutine(DestroyAfterDelay());
    }

    private void OnDisable()
    {
        Destroy(gameObject);
    }

    private IEnumerator DestroyAfterDelay()
    {
        // Wait for the delay, then destroy the GameObject

        if(unscaledTime)
        {
            yield return new WaitForSecondsRealtime(delay);
        }
        else
        {
            yield return new WaitForSeconds(delay);
        }

        Destroy(gameObject);
    }
}
