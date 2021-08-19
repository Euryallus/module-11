using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

// ||=======================================================================||
// || PlayerInstance: Keeps a static instance of the active player. Also    ||
// ||    conveniently groups a number of player related scripts together    ||
// ||    in a place that is easy and reliable to access (compared to        ||
// ||    finding the player GameObject in the scene which is slow)          ||
// ||=======================================================================||
// || Used on prefab: Hugo/Player                                           ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the production phase (Module 11).                                 ||
// ||=======================================================================||

public class PlayerInstance : MonoBehaviour
{
    public static PlayerInstance ActivePlayer; // Static instance of the class to ensure only one player ever exists in a scene

    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private string[] dontDestroyScenes;    // Names of all scenes that the player character should stay active in after loading into them
                                                            //   Prevents the player from staying active in scenes where it doesn't belong, e.g. MainMenu

    #endregion

    #region Properties

    public PlayerMovement       PlayerMovement      { get { return playerMovement; } }
    public PlayerStats          PlayerStats         { get { return playerStats; } }
    public CharacterController  PlayerController    { get { return PlayerMovement.Controller; } }

    #endregion

    private PlayerMovement  playerMovement; // Reference to the PlayerMovement script
    private PlayerStats     playerStats;    // Reference to the PlayerStats script

    private bool            inFirstScene;   // Whether this is the first scene the player has been in

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

        // Call OnSceneLoaded each time a scene is loaded
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (ActivePlayer == this)
        {
            // Destroy the player if it should not exist in the loaded scene when loading into any scene after the first

            if (!inFirstScene && !dontDestroyScenes.Contains(scene.name))
            {
                // Moving into a new scene that the player should not exist in - destroy it
                Destroy(gameObject);
            }

            // No longer in the first scene
            inFirstScene = false;
        }
    }
}
