using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

// ||=======================================================================||
// || SaveLoadManager: Handles saving/loading all game data.                ||
// ||=======================================================================||
// || Used on prefab: Joe/SaveSystem/_SaveLoadManager                       ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance; // Static instance of the class for simple access

    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private GameObject loadingPanelPrefab; // UI panel shown when loading the game

    #endregion

    public event Action<SaveData>   SaveObjectsEvent;           // Event invoked when objects are saved
    public event Action<SaveData>   LoadObjectsSetupEvent;      // Event invoked for the first stage of object loading
    public event Action<SaveData>   LoadObjectsConfigureEvent;  // Event invoked for the second stage of object loading

    private string saveDirectory;                       // Path where save files are stored

    private Dictionary<string, int> playerPrefsDefaultIntValues = new Dictionary<string, int>()
    {
        { "musicVolume"       , 8 },
        { "soundEffectsVolume", 8 },

        // Ints being used as bool values (1 = true, 0 = false), since PlayerPrefs doesn't work with bools
        { "screenShake", 1 },
        { "viewBobbing", 1 }
    };

    private const string SaveDataFileName = "save.dat"; // File name used for save data files (before scene name is prepended)

    private void Awake()
    {
        // Ensure that an instance of the class does not already exist
        if (Instance == null)
        {
            // Set this class as the instance and ensure that it stays when changing scenes
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        // If there is an existing instance that is not this, destroy the GameObject this script is connected to
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Setup the save directory using persistentDataPath (AppData/LocalLow on Windows)
        saveDirectory = Application.persistentDataPath + "/Saves";

        // Subscribe to the sceneLoaded event so OnSceneLoaded is called each time
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void SubscribeSaveLoadEvents(Action<SaveData> onSave, Action<SaveData> onLoadSetup, Action<SaveData> onLoadConfig)
    {
        // Subscribes to save/load events to the OnSave/OnLoadSetup/OnLoadCOnfigure functions on
        //   an object implementing IPerstentObject will be called when the game is saved/loaded

        SaveObjectsEvent          += onSave;
        LoadObjectsSetupEvent     += onLoadSetup;
        LoadObjectsConfigureEvent += onLoadConfig;
    }

    public void UnsubscribeSaveLoadEvents(Action<SaveData> onSave, Action<SaveData> onLoadSetup, Action<SaveData> onLoadConfig)
    {
        // Unsubscribes an objects save/load functions from the events

        SaveObjectsEvent          -= onSave;
        LoadObjectsSetupEvent     -= onLoadSetup;
        LoadObjectsConfigureEvent -= onLoadConfig;
    }

    public bool SaveGame()
    {
        SaveData dataToSave = new SaveData();

        // Call the OnSave function on all persistent objects - they will each add some data to the SavaData object
        SaveObjectsEvent?.Invoke(dataToSave);

        // Create a save data file path based on the name of the scene being saved
        string sceneName = SceneManager.GetActiveScene().name;
        string saveDataPath = saveDirectory + "/" + sceneName + "_" + SaveDataFileName;

        // Try to create the save directory folder
        if (!Directory.Exists(saveDirectory))
        {
            try
            {
                Directory.CreateDirectory(saveDirectory);
            }
            catch
            {
                Debug.LogError("Could not create save directory: " + saveDirectory);
                return false;
            }
        }

        FileStream file;

        // Try to open the save data file, or create one if it doesn't already exist
        try
        {
            file = File.Open(saveDataPath, FileMode.OpenOrCreate);
        }
        catch
        {
            Debug.LogError("Could not open/create file at " + saveDataPath);
            return false;
        }

        // Seralize the save data to the file
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, dataToSave);

        // Saving done, close the file
        file.Close();

        Debug.Log("Game saved!");

        return true;
    }

    public void LoadGame()
    {
        StartCoroutine(LoadGameCoroutine());
    }

    private IEnumerator LoadGameCoroutine()
    {
        Debug.Log("Attempting to load game");

        Transform canvasTransform = GameObject.FindGameObjectWithTag("JoeCanvas").transform;

        // Instantiate the loading panel, using the canvas as a parent
        LoadingPanel        loadingPanel        = Instantiate(loadingPanelPrefab, canvasTransform).GetComponent<LoadingPanel>();

        // Disable the CharacterController component attached to the player to prevent them moving while the game
        //   is loading and allow the player to be teleported to the posision of the last used save point
        CharacterController playerCharControl   = GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterController>();
        playerCharControl.enabled = false;

        // Get the path of the file to be loaded based on the current scene
        string sceneName = SceneManager.GetActiveScene().name;
        string loadDataPath = saveDirectory + "/" + sceneName + "_" + SaveDataFileName;

        if (File.Exists(loadDataPath))
        {
            // Save file exists - open it and load save data

            FileStream file;

            // Try to open the file for reading
            try
            {
                file = File.OpenRead(loadDataPath);
            }
            catch
            {
                Debug.LogError("Could not open file when loading: " + loadDataPath);
                yield break;
            }

            BinaryFormatter bf = new BinaryFormatter();
            SaveData loadedData;

            // Try to deserialise the save data from the file
            try
            {
                loadedData = (SaveData)bf.Deserialize(file);
                file.Close();
            }
            catch
            {
                file.Close();
                Debug.LogError("Could not deserialize save data from " + loadDataPath);
                yield break;
            }

            // Wait a couple of frames to ensure objects are set up before calling load functions
            yield return null;
            yield return null;

            // Setup then configure all persistent objects with the loaded data

            Debug.Log("Load Stage 1: Setup");
            LoadObjectsSetupEvent?.Invoke(loadedData);

            yield return null;

            Debug.Log("Load Stage 2: Configure");
            LoadObjectsConfigureEvent?.Invoke(loadedData);

            // Loading is done
            Debug.Log("Game loaded!");
        }
        else
        {
            // No save file found
            Debug.LogWarning("No save file exists at: " + loadDataPath);
        }

        // Loading is done, hide the load panel and re-enable player controls
        loadingPanel.LoadDone();

        playerCharControl.enabled = true;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene loaded: " + scene.name);

        // Load the game's save data from a file if the loaded scene has one of the following names:
        if(scene.name == "CombinedScene" || scene.name == "JoeTestScene" || scene.name == "Noah test scene" ||
            scene.name == "DemoScene" || scene.name == "NoahNewScene" || scene.name == "Mod10SubmissionDemo")
        {
            LoadGame();
        }
    }

    // Saving/loading PlayerPrefs
    //============================

    public void SaveIntToPlayerPrefs(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
        PlayerPrefs.Save();
    }

    public int GetIntFromPlayerPrefs(string key)
    {
        if(playerPrefsDefaultIntValues.ContainsKey(key))
        {
            return PlayerPrefs.GetInt(key, playerPrefsDefaultIntValues[key]);
        }
        else
        {
            Debug.LogWarning("No default int value set for PlayerPref with key: " + key);

            return PlayerPrefs.GetInt(key);
        }
    }

    public void SaveBoolToPlayerPrefs(string key, bool value)
    {
        PlayerPrefs.SetInt(key, value == true ? 1 : 0);
        PlayerPrefs.Save();
    }

    public bool GetBoolFromPlayerPrefs(string key)
    {
        if (playerPrefsDefaultIntValues.ContainsKey(key))
        {
            return (PlayerPrefs.GetInt(key, playerPrefsDefaultIntValues[key]) == 1 ? true : false);
        }
        else
        {
            Debug.LogWarning("No default int value set for PlayerPref with key: " + key);

            return (PlayerPrefs.GetInt(key) == 1 ? true : false);
        }
    }
}
