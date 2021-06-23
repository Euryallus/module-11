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

    [SerializeField] private GameObject         loadingPanelPrefab; // UI panel shown when loading the game

    #endregion

    #region Properties

    public bool LoadingAfterDeath { get { return loadingAfterDeath; } set { loadingAfterDeath = value; } }

    #endregion

    private event Action<SaveData>      SaveSceneObjectsEvent;              // Event invoked when objects are saved
    private event Action<SaveData>      LoadSceneObjectsSetupEvent;         // Event invoked for the first stage of object loading
    private event Action<SaveData>      LoadSceneObjectsConfigureEvent;     // Event invoked for the second stage of object loading

    private event Action<SaveData>      SaveGlobalObjectsEvent;             // Event invoked when objects are saved
    private event Action<SaveData>      LoadGlobalObjectsSetupEvent;        // Event invoked for the first stage of object loading
    private event Action<SaveData>      LoadGlobalObjectsConfigureEvent;    // Event invoked for the second stage of object loading

    private string                      baseSaveDirectory;                  // Path where all save files are stored
    private string                      currentScenesDirectory;             // Directory containing all scene save files that may be required for the loaded game
    private bool                        loadingAfterDeath;                  // Whether the game is being reloaded after the player dies

    private readonly Dictionary<string, int> playerPrefsDefaultIntValues = new Dictionary<string, int>()
    {
        { "musicVolume"       , 8 },
        { "soundEffectsVolume", 8 },

        // Ints being used as bool values (1 = true, 0 = false), since PlayerPrefs doesn't work with bools
        { "screenShake", 1 },
        { "viewBobbing", 1 }
    };

    private const string MainSaveDirectory      = "Maps";
    private const string MainStartingSceneName  = "The Village";

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
        baseSaveDirectory = Application.persistentDataPath + "/Saves";

        // Subscribe to the sceneLoaded event so OnSceneLoaded is called each time
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void SubscribeSceneSaveLoadEvents(Action<SaveData> onSave, Action<SaveData> onLoadSetup, Action<SaveData> onLoadConfig)
    {
        // Subscribes to save/load events to the OnSave/OnLoadSetup/OnLoadCOnfigure functions on
        //   an object implementing IPerstentObject will be called when the game is saved/loaded

        SaveSceneObjectsEvent            += onSave;
        LoadSceneObjectsSetupEvent       += onLoadSetup;
        LoadSceneObjectsConfigureEvent   += onLoadConfig;
    }

    public void UnsubscribeSceneSaveLoadEvents(Action<SaveData> onSave, Action<SaveData> onLoadSetup, Action<SaveData> onLoadConfig)
    {
        // Unsubscribes an objects save/load functions from the events

        SaveSceneObjectsEvent           -= onSave;
        LoadSceneObjectsSetupEvent      -= onLoadSetup;
        LoadSceneObjectsConfigureEvent  -= onLoadConfig;
    }

    public void SubscribeGlobalSaveLoadEvents(Action<SaveData> onSave, Action<SaveData> onLoadSetup, Action<SaveData> onLoadConfig)
    {
        SaveGlobalObjectsEvent          += onSave;
        LoadGlobalObjectsSetupEvent     += onLoadSetup;
        LoadGlobalObjectsConfigureEvent += onLoadConfig;
    }

    public void UnsubscribeGlobalSaveLoadEvents(Action<SaveData> onSave, Action<SaveData> onLoadSetup, Action<SaveData> onLoadConfig)
    {
        // Unsubscribes an objects save/load functions from the events

        SaveGlobalObjectsEvent          -= onSave;
        LoadGlobalObjectsSetupEvent     -= onLoadSetup;
        LoadGlobalObjectsConfigureEvent -= onLoadConfig;
    }

    //public bool SaveGame()
    //{
    //    SaveData dataToSave = new SaveData();

    //    // Call the OnSave function on all persistent objects - they will each add some data to the SavaData object
    //    SaveSceneObjectsEvent?.Invoke(dataToSave);

    //    // Create a save data file path based on the name of the scene being saved
    //    string sceneName = SceneManager.GetActiveScene().name;
    //    string saveDataPath = baseSaveDirectory + "/" + sceneName + "_save.dat";

    //    // Try to create the save directory folder
    //    if (!Directory.Exists(baseSaveDirectory))
    //    {
    //        try
    //        {
    //            Directory.CreateDirectory(baseSaveDirectory);
    //        }
    //        catch
    //        {
    //            Debug.LogError("Could not create save directory: " + baseSaveDirectory);
    //            return false;
    //        }
    //    }

    //    FileStream file;

    //    // Try to open the save data file, or create one if it doesn't already exist
    //    try
    //    {
    //        file = File.Open(saveDataPath, FileMode.OpenOrCreate);
    //    }
    //    catch
    //    {
    //        Debug.LogError("Could not open/create file at " + saveDataPath);
    //        return false;
    //    }

    //    // Seralize the save data to the file
    //    BinaryFormatter bf = new BinaryFormatter();
    //    bf.Serialize(file, dataToSave);

    //    // Saving done, close the file
    //    file.Close();

    //    Debug.Log("Game saved!");

    //    return true;
    //}

    public bool SaveGameData(string scenesDirectory = "UseCurrentDirectory")
    {
        if(scenesDirectory == "UseCurrentDirectory")
        {
            scenesDirectory = currentScenesDirectory;
        }

        // Try to create the save directory folder
        if (!Directory.Exists(scenesDirectory))
        {
            try
            {
                Directory.CreateDirectory(scenesDirectory);
            }
            catch
            {
                Debug.LogError("Could not create save directory: " + scenesDirectory);
                return false;
            }
        }

        SaveData infoDataToSave   = new SaveData();
        SaveData globalDataToSave = new SaveData();
        SaveData sceneDataToSave  = new SaveData();

        string loadedSceneName = SceneManager.GetActiveScene().name;

        infoDataToSave.AddData("lastLoadedScene", loadedSceneName);

        SaveGlobalObjectsEvent?.Invoke(globalDataToSave);

        SaveSceneObjectsEvent?.Invoke(sceneDataToSave);

        string infoDataPath     = scenesDirectory + "LoadInfo.save";
        string globalDataPath   = scenesDirectory + "GlobalSave.save";
        string sceneDataPath    = scenesDirectory + loadedSceneName + ".save";

        FileStream infoSaveFile, globalSaveFile, sceneSaveFile;

        // Try to open the save data files, or create them if they don't already exist
        try
        {
            infoSaveFile    = File.Open(infoDataPath,   FileMode.OpenOrCreate);
            globalSaveFile  = File.Open(globalDataPath, FileMode.OpenOrCreate);
            sceneSaveFile   = File.Open(sceneDataPath,  FileMode.OpenOrCreate);
        }
        catch
        {
            Debug.LogError("Could not open/create file saves files in directory: " + scenesDirectory);
            return false;
        }

        // Seralize the save data to the files
        BinaryFormatter bf = new BinaryFormatter();

        bf.Serialize(infoSaveFile,   infoDataToSave);
        bf.Serialize(globalSaveFile, globalDataToSave);
        bf.Serialize(sceneSaveFile,  sceneDataToSave);

        // Saving done, close the files
        infoSaveFile  .Close();
        globalSaveFile.Close();
        sceneSaveFile .Close();

        Debug.Log("Game data saved!");

        return true;
    }

    public void LoadGameData(string saveDirectoryName = MainSaveDirectory, string startingSceneName = MainStartingSceneName)
    {
        StartCoroutine(LoadGameDataCoroutine(saveDirectoryName, startingSceneName));
    }

    private IEnumerator LoadGameDataCoroutine(string saveDirectoryName, string startingSceneName)
    {
        // STEP 1: Attempt to load the global save file for the specified scene group
        //============================================================================

        string directoryPath = baseSaveDirectory + "/" + saveDirectoryName + "/";
        string infoDataPath = directoryPath + "LoadInfo.save";
        string globalDataPath = directoryPath + "GlobalSave.save";

        currentScenesDirectory = directoryPath;

        if (File.Exists(infoDataPath) && File.Exists(globalDataPath))
        {
            // Global and LoadInfo save files exist - open them and load save data

            FileStream infoDataFile, globalDataFile;

            // Try to open the file for reading
            try
            {
                infoDataFile = File.OpenRead(infoDataPath);
                globalDataFile = File.OpenRead(globalDataPath);
            }
            catch
            {
                Debug.LogError("Could not open info or global save file when loading: " + globalDataPath);
                yield break;
            }

            SaveData infoData, globalData;

            // Try to deserialise the Global and LoadInfo save data from the file
            try
            {
                BinaryFormatter bf = new BinaryFormatter();

                infoData = (SaveData)bf.Deserialize(infoDataFile);
                infoDataFile.Close();

                globalData = (SaveData)bf.Deserialize(globalDataFile);
                globalDataFile.Close();
            }
            catch
            {
                infoDataFile.Close();
                globalDataFile.Close();

                Debug.LogError("Could not deserialize save data from: " + infoDataPath + " or " + globalDataPath);
                yield break;
            }

            yield return null;

            string sceneToLoadName = infoData.GetData<string>("lastLoadedScene");

            // TODO: Re-add loadingAfterDeath stuff

            yield return StartCoroutine(LoadSceneCoroutine(sceneToLoadName, directoryPath));

            // Disable the CharacterController component attached to the player to prevent them moving while the game
            //   is loading and allow the player to be teleported to the posision of the last used save point
            PlayerMovement.Instance.Controller.enabled = false;

            // Load global stuff (call global events)

            // Re-enable the character coltroller so the player can move
            PlayerMovement.Instance.Controller.enabled = true;
        }
        else
        {
            // No global/info save files, this should only occur when first loading a scene group with no previously saved data

            Debug.Log("No saved data found for save directory: " + saveDirectoryName);

            StartCoroutine(LoadSceneCoroutine(startingSceneName, directoryPath));
        }
    }

    public void LoadScene(string sceneName, string scenesDirectory = "UseCurrentDirectory")
    {
        StartCoroutine(LoadSceneCoroutine(sceneName, scenesDirectory));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName, string scenesDirectory)
    {
        AsyncOperation sceneLoadOperation = SceneManager.LoadSceneAsync(sceneName);

        if(PlayerMovement.Instance != null)
        {
            PlayerMovement.Instance.Controller.enabled = false;
        }

        while (!sceneLoadOperation.isDone)
        {
            yield return null;
        }

        PlayerMovement.Instance.Controller.enabled = false;

        // Load saved scene data
        //=======================
        yield return StartCoroutine(LoadSceneDataCoroutine(sceneName, scenesDirectory));

        PlayerMovement.Instance.gameObject.transform.position = new Vector3(0.0f, 1.0f, 0.0f);
        PlayerMovement.Instance.Controller.enabled = true;
    }

    private IEnumerator LoadSceneDataCoroutine(string sceneToLoadName, string scenesDirectory)
    {
        if(scenesDirectory == "UseCurrentDirectory")
        {
            scenesDirectory = currentScenesDirectory;
        }

        string sceneDataPath = scenesDirectory + sceneToLoadName + ".save";

        if (File.Exists(sceneDataPath))
        {
            FileStream sceneDataFile;

            // Try to open the file for reading
            try
            {
                sceneDataFile = File.OpenRead(sceneDataPath);
            }
            catch
            {
                Debug.LogError("Could not open scene save file when loading: " + sceneDataPath);
                yield break;
            }

            SaveData sceneData;

            // Try to deserialise the Global and LoadInfo save data from the file
            try
            {
                BinaryFormatter bf = new BinaryFormatter();

                sceneData = (SaveData)bf.Deserialize(sceneDataFile);
                sceneDataFile.Close();
            }
            catch
            {
                sceneDataFile.Close();

                Debug.LogError("Could not deserialize save data from: " + sceneDataPath);
                yield break;
            }

            yield return null;

            Debug.Log("Scene load stage 1: setup");
            LoadSceneObjectsSetupEvent?.Invoke(sceneData);

            yield return null;

            Debug.Log("Scene load stage 2: configure");
            LoadSceneObjectsConfigureEvent?.Invoke(sceneData);

            // Loading is done
            Debug.Log("Scene data loaded for " + sceneToLoadName);
        }
    }

    //public void LoadGame()
    //{
    //    StartCoroutine(LoadGameCoroutine());
    //}

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
        string loadDataPath = baseSaveDirectory + "/" + sceneName + "_save.dat";

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
            LoadSceneObjectsSetupEvent?.Invoke(loadedData);

            yield return null;

            Debug.Log("Load Stage 2: Configure");
            LoadSceneObjectsConfigureEvent?.Invoke(loadedData);

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

        // Re-enable the character coltroller so the player can move
        playerCharControl.enabled = true;

        // Reset loading after death bool
        loadingAfterDeath = false;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Debug.Log("Scene loaded: " + scene.name);

        //// Load the game's save data from a file if the loaded scene has one of the following names:
        //if(scene.name == "CombinedScene" || scene.name == "JoeTestScene" || scene.name == "Noah test scene" ||
        //    scene.name == "DemoScene" || scene.name == "NoahNewScene" || scene.name == "Mod10SubmissionDemo")
        //{
        //    LoadGame();
        //}
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
            return (PlayerPrefs.GetInt(key, playerPrefsDefaultIntValues[key]) == 1);
        }
        else
        {
            Debug.LogWarning("No default int value set for PlayerPref with key: " + key);

            return (PlayerPrefs.GetInt(key) == 1);
        }
    }
}
