using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// ||=======================================================================||
// || DeathPanel: A panel shown when the player dies, displays a            ||
// ||   death message.                                                      ||
// ||=======================================================================||
// || Used on prefab: Joe/UI/DeathPanel                                     ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public class DeathPanel : MonoBehaviour
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private TextMeshProUGUI deathCauseText; // UI text showing the cause of death

    #endregion

    private void Start()
    {
        // Play a sound when the death panel is first shown
        AudioManager.Instance.PlaySoundEffect2D("sealExplosion");
    }

    public void SetDeathCauseText(string cause)
    {
        // Set the text showing cause of death
        deathCauseText.text = cause;
    }

    public void ButtonRespawn()
    {
        Debug.Log("===== PLAYER DEATH: RELOADING SCENE =====");

        // Re-hide the cursor and reset time scale to its default value to un-pause all movement
        Cursor.visible = false;
        Time.timeScale = 1.0f;

        // Reload the active scene and hence reset progress to where the player last saved
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }
}