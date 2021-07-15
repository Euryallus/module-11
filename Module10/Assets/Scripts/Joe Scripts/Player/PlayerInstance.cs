using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerInstance : MonoBehaviour
{
    public static PlayerInstance ActivePlayer; // Static instance of the class to ensure only one player ever exists in a scene

    [SerializeField] private string[] dontDestroyScenes;    // Names of all scenes that the player character should stay active in after loading into them
                                                            //   Prevents the player from staying active in scenes where it doesn't belong, e.g. MainMenu
    public PlayerMovement       PlayerMovement      { get { return playerMovement; } }
    public PlayerStats          PlayerStats         { get { return playerStats; } }
    public CharacterController  PlayerController    { get { return PlayerMovement.Controller; } }

    private PlayerMovement  playerMovement;
    private PlayerStats     playerStats;

    private bool inFirstScene;

    private void Awake()
    {
        // Ensure that an instance of the class does not already exist
        if (ActivePlayer == null)
        {
            // Set this class as the instance and ensure that it stays when changing scenes
            ActivePlayer = this;
            DontDestroyOnLoad(gameObject);
            inFirstScene = true;
        }
        // If there is an existing instance that is not this, destroy the GameObject this script is connected to
        else if (ActivePlayer != this)
        {
            Destroy(gameObject);
            return;
        }

        playerMovement  = GetComponent<PlayerMovement>();
        playerStats     = GetComponent<PlayerStats>();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (ActivePlayer == this)
        {
            // Destroy the player if it should not exist in the loaded scene when loading into any scene after the first

            if (!inFirstScene && !dontDestroyScenes.Contains(scene.name))
            {
                Destroy(gameObject);
            }

            inFirstScene = false;
        }
    }
}
