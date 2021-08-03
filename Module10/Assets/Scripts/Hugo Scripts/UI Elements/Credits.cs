using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Credits : MonoBehaviour
{
    public MusicClass bgMusic;
    private void Start()
    {
        AudioManager.Instance.PlayMusic(bgMusic, true);
    }

    public void LoadMainMenu()
    {
        // load main menu needs adding

        SceneManager.LoadScene(0);
    }
}
