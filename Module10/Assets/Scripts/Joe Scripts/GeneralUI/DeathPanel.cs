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

        // The game is being reloaded after a player death
        SaveLoadManager.Instance.LoadingAfterDeath = true;

        // Reload the active scene and reset progress to where the player last saved
        SaveLoadManager.Instance.LoadGameScene(SceneManager.GetActiveScene().name);

        AudioManager.Instance.PlaySoundEffect2D("buttonClickMain2", true);
    }
}