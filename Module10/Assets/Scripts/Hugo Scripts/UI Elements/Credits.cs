using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Functionality for button in credits
// Development window:  Production phase
// Inherits from:       MonoBehaviour

public class Credits : MonoBehaviour
{
    public MusicClass bgMusic; // Ref. to music to play in credits
    private void Start()
    {
        // Plays music defined
        AudioManager.Instance.PlayMusic(bgMusic, true);
    }

    public void LoadMainMenu()
    {
        // Loads main menu scene (index 0)
        SceneManager.LoadScene(0);
    }
}
