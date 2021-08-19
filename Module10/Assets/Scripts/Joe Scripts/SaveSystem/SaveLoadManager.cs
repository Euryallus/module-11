using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// ||=======================================================================||
// || SaveLoadManager: Handles saving/loading all game data.                ||
// ||=======================================================================||
// || Used on prefab: Joe/SaveSystem/_SaveLoadManager                       ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || originally for the prototype phase.                                   ||
// ||                                                                       ||
// || Changes made during the production phase (Module 11):                 ||
// ||                                                                       ||
// || - All save/load logic has been rewritten to be more flexible and      ||
// ||    support multiple scenes with different files                       ||
// || - Various refinements and fixes                                       ||
// ||=======================================================================||

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance; // Static instance of the class for simple access

    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private GameObject    loadingCanvasPrefab; // UI canvas instantiated when loading a scene
    [SerializeField] private GameObject    titleCardPrefab;     // UI element that shows the name of a scene when one is loaded
    [SerializeField] private SceneUIData[] sceneUIData;         // Array of data determining what names and images will be shown in the UI to represent each scene

    [SerializeField] private PlayerQuestBacklog playerData;     // Reference to the player quest backlog ScriptableObject

    #endregion

    #region Properties

    public bool LoadingAfterDeath   { get { return loadingAfterDeath; } set { loadingAfterDeath = value; } }
    public bool LoadingSceneData    { get { return loadingSceneData; } }

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
    private bool                        loadingSceneData;                   // Whether scene data is currently being loaded

    // This dictionary contains the default values for all settings saved using PlayerPrefs, indexed by the names used to save/load the values
    //   (For example, if musicVolume is loaded but no value has yet been saved, the default value of 8 will be fetched from this dictionary and returned)
    private readonly Dictionary<string, int> playerPrefsDefaultValues = new Dictionary<string, int>()
    {
        { "optionsSelectedTab", 0 },

        { "musicVolume"       , 8 },
        { "soundEffectsVolume", 8 },

        // The following ints represent bool values, where 0 = false and 1 = true (done this way because PlayerPrefs doesn't support bools directly)
        { "screenShake", 1 },
        { "viewBobbing", 1 }
    };

    private const string MainSaveDirectory      = "Maps";        // The default folder that will be used to store save files within baseSaveDirectory
    private const string MainStartingSceneName  = "The Village"; // The name of the scene that will be loaded if no other name is specified

    private static ISavePoint lastUsedSavePoint = null;          // Keeps track of the save point that was most recently used by the player

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

        // Disable debug logging in builds for better performance
        #if UNITY_EDITOR
            Debug.unityLogger.logEnabled = true;
        #else
            Debug.unityLogger.logEnabled = false;
        #endif
    }

    #region Save/Load Event Setup

    public void SubscribeSceneSaveLoadEvents(Action<SaveData> onSave, Action<SaveData> onLoadSetup, Action<SaveData> onLoadConfig)
    {
        // Subscribes to global save/load events so the OnSave/OnLoadSetup/OnLoadConfigure functions on an
        //   object implementing IPerstentSceneObject will be called when the game is saved/loaded in a certain scene

        SaveSceneObjectsEvent += onSave;
        LoadSceneObjectsSetupEvent       += onLoadSetup;
        LoadSceneObjectsConfigureEvent   += onLoadConfig;
    }

    public void UnsubscribeSceneSaveLoadEvents(Action<SaveData> onSave, Action<SaveData> onLoadSetup, Action<SaveData> onLoadConfig)
    {
        // Unsubscribes an object's save/load functions from the events

        SaveSceneObjectsEvent           -= onSave;
        LoadSceneObjectsSetupEvent      -= onLoadSetup;
        LoadSceneObjectsConfigureEvent  -= onLoadConfig;
    }

    public void SubscribeGlobalSaveLoadEvents(Action<SaveData> onSave, Action<SaveData> onLoadSetup, Action<SaveData> onLoadConfig)
    {
        // Subscribes to global save/load events so the OnSave/OnLoadSetup/OnLoadConfigure functions on an
        //   object implementing IPerstentGlobalObject will be called when the game is saved/loaded in any scene

        SaveGlobalObjectsEvent += onSave;
        LoadGlobalObjectsSetupEvent     += onLoadSetup;
        LoadGlobalObjectsConfigureEvent += onLoadConfig;
    }

    public void UnsubscribeGlobalSaveLoadEvents(Action<SaveData> onSave, Action<SaveData> onLoadSetup, Action<SaveData> onLoadConfig)
    {
        // Unsubscribes an object's save/load functions from the events

        SaveGlobalObjectsEvent          -= onSave;
        LoadGlobalObjectsSetupEvent     -= onLoadSetup;
        LoadGlobalObjectsConfigureEvent -= onLoadConfig;
    }

    #endregion

    public static void SetLastUsedSavePoint(ISavePoint savePoint)
    {
        if(lastUsedSavePoint != null)
        {
            // If another save point was previously used, set it as unused
            lastUsedSavePoint.SetAsUnused();
        }

        // Set the last used save point to the savePoint that was given
        lastUsedSavePoint = savePoint;
    }

    public bool SaveGameData(string returnToSceneName = "UseActiveScene", string scenesDirectory = "UseCurrentDirectory")
    {
        if(scenesDirectory == "UseCurrentDirectory")
        {
            // Use the current scenes directory to load data if nothing else was specified
            scenesDirectory = currentScenesDirectory;
        }

        // Try to create the save directory folder if it doesn't already exist
        if (!Directory.Exists(scenesDirectory))
        {
            try
            {
                Directory.CreateDirectory(scenesDirectory);
            }
            catch
            {
                // Data not saved - no directory exists and one could not be created
                Debug.LogError("Could not create save directory: " + scenesDirectory);
                return false;
            }
        }

        // Instantiate the objects used to store data that will be saved:

        // Info data: Anything that will be displayed in the main menu before main game data is loaded - the scene the player is in, and the player's name
        SaveData infoDataToSave   = new SaveData();

        // Global data: Game data that persists through all scenes - the player's inventory contents, player health etc.
        SaveData globalDataToSave = new SaveData();

        // Scene data: Game data specific to the scene being saved - the contents of chests in the scene, door states etc.
        SaveData sceneDataToSave  = new SaveData();

        string loadedSceneName = SceneManager.GetActiveScene().name;

        if(returnToSceneName == "UseActiveScene")
        {
            // If the player should return to the active scene when data is loaded, use the loaded scene name for returnToSceneName
            returnToSceneName = loadedSceneName;
        }

        // Add the scene the player should return to, to the info save data
        infoDataToSave.AddData("lastLoadedScene", returnToSceneName);

        if (PlayerInstance.ActivePlayer != null)
        {
            // Also add the set PlayerName to the save data
            infoDataToSave.AddData("playerName", PlayerStats.PlayerName);
        }

        // Invoke global save events on all subscribed IPersistentGlobalObjects
        SaveGlobalObjectsEvent?.Invoke(globalDataToSave);

        // Invoke scene-specific save events on all subscribed IPersistentGlobalObjects
        SaveSceneObjectsEvent?.Invoke(sceneDataToSave);

        FileStream infoSaveFile, globalSaveFile, sceneSaveFile;

        // Try to open all the save data files, or create them if they don't already exist
        try
        {
            infoSaveFile    = File.Open(scenesDirectory + "LoadInfo.save",            FileMode.OpenOrCreate);
            globalSaveFile  = File.Open(scenesDirectory + "GlobalSave.save",          FileMode.OpenOrCreate);
            sceneSaveFile   = File.Open(scenesDirectory + loadedSceneName + ".save",  FileMode.OpenOrCreate);
        }
        catch
        {
            // Data not saved - save files could not be opened or created
            Debug.LogError("Could not open/create file saves files in directory: " + scenesDirectory);
            return false;
        }

        // Use a BinaryFormatter to seralize the save data to the three files
        BinaryFormatter bf = new BinaryFormatter();

        bf.Serialize(infoSaveFile,   infoDataToSave);
        bf.Serialize(globalSaveFile, globalDataToSave);
        bf.Serialize(sceneSaveFile,  sceneDataToSave);

        // Saving is done, close the files
        infoSaveFile  .Close();
        globalSaveFile.Close();
        sceneSaveFile .Close();

        Debug.Log("Game data saved!");

        // Data saved successfully
        return true;
    }

    public bool LoadGameInfo(out string sceneToLoadName, out string savedPlayerName, out string savedAreaName,
                                string saveDirectoryName = MainSaveDirectory, string defaultStartingSceneName = MainStartingSceneName)
    {
        // Loads data from the infoSaveFile for a specific save directory

        // STEP 1: Attempt to load the global save file for the specified directory
        //=========================================================================

        string directoryPath = baseSaveDirectory + "/" + saveDirectoryName + "/";
        string infoDataPath  = directoryPath + "LoadInfo.save";

        // The specified default starting scene will be loaded if there is no existing save file
        sceneToLoadName = defaultStartingSceneName;

        // Default the saved player/area names to null in case nothing is loaded
        savedPlayerName = null;
        savedAreaName   = null;

        // The current scenes directory is now the directory being used to load game info
        currentScenesDirectory = directoryPath;

        if (File.Exists(infoDataPath))
        {
            // An info save files exists - open it and load data

            FileStream infoDataFile;

            // Try to open the file for reading
            try
            {
                infoDataFile = File.OpenRead(infoDataPath);
            }
            catch
            {
                Debug.LogError("Could not open info save file when loading: " + infoDataPath);
                return false;
            }

            SaveData infoData;

            // Try to deserialise the LoadInfo save data from the file using a BinaryFormatter
            try
            {
                BinaryFormatter bf = new BinaryFormatter();

                infoData = (SaveData)bf.Deserialize(infoDataFile);

                // Close the file once data has been deserialised
                infoDataFile.Close();
            }
            catch
            {
                // Data could not be loaded - deserialisation failed
                infoDataFile.Close();

                Debug.LogError("Could not deserialise save data from: " + infoDataPath);
                return false;
            }

            // Get the scene name from the loaded data
            sceneToLoadName = infoData.GetData<string>("lastLoadedScene");

            // Get the player name from the loaded data
            savedPlayerName = infoData.GetData<string>("playerName");

            // Get the area name to be displayed by getting the UI name from the UI data for the sceneToLoadName
            savedAreaName = GetUIDataForScene(sceneToLoadName).UIName;

            // Set the player name to the value that was previously saved
            PlayerStats.PlayerName = savedPlayerName;

            // Info data loaded successfully
            return true;
        }
        else
        {
            // No info save file, this should only occur when first loading from a directory with no previously saved data

            Debug.Log("No saved data found for save directory: " + saveDirectoryName);

            return false;
        }
    }

    public void LoadGameScene(string sceneName, string scenesDirectory = "UseCurrentDirectory")
    {
        // Start loading a game scene asynchronously
        StartCoroutine(LoadGameSceneCoroutine(sceneName, scenesDirectory));
    }

    private IEnumerator LoadGameSceneCoroutine(string sceneName, string scenesDirectory)
    {
        // Scene data is being loaded
        loadingSceneData = true;

        // Reset the last used save point so it doesn't carry over from a previous scene
        lastUsedSavePoint = null;

        // Pause time, storing the previous time value so it can be restored when loading is done
        float previousTimeScale = Time.timeScale;
        Time.timeScale = 0.0f;

        // Setup the loading panel which shows load progress
        LoadingPanel loadingPanel = Instantiate(loadingCanvasPrefab).GetComponent<LoadingPanel>();

        // Try and get UI data for the scene being loaded
        SceneUIData sceneUIData = GetUIDataForScene(sceneName);

        if(sceneUIData != null)
        {
            // UI data found, display the area UI name and sprite on the loading panel
            loadingPanel.SetAreaNameText(sceneUIData.UIName);
            loadingPanel.SetAreaPreviewSprite(sceneUIData.UISprite);
        }

        AudioManager audioManager = AudioManager.Instance;

        // Fade out global music/sound
        audioManager.FadeGlobalVolumeMultiplier(0.0f, 1.0f);

        PlayerInstance activePlayer = PlayerInstance.ActivePlayer;

        // If an active player is in the scene, disable the player controller so no movement happens while loading
        if (activePlayer != null && activePlayer.PlayerController != null)
        {
            activePlayer.PlayerController.enabled = false;
        }

        // Short delay while the loading panel fades in
        yield return new WaitForSecondsRealtime(1.2f);

        // Start loading the Unity scene asynchronously
        AsyncOperation sceneLoadOperation = SceneManager.LoadSceneAsync(sceneName);

        while (!sceneLoadOperation.isDone)
        {
            // Update the displayed progress bar while the scene loads
            UpdateLoadingProgress(loadingPanel, sceneLoadOperation.progress * 0.555f); // Converts from 0.0 - 0.9, to 0.0 - 0.5

            yield return null;
        }

        // Fade main audio back in
        audioManager.UpdateGlobalVolumeMultiplier(0.0f);

        // The scene was loaded - now data needs to be loaded from files

        // Load and setup saved global/scene data
        //========================================
        yield return StartCoroutine(LoadDataForSceneCoroutine(sceneName, scenesDirectory, loadingPanel));

        if (!loadingAfterDeath)
        {
            // Show an animated title card if loading into the scene (unless reloading after death)
            if (sceneUIData != null && !string.IsNullOrEmpty(sceneUIData.UIName))
            {
                ShowTitleCardForScene(sceneUIData.UIName);
            }
        }

        // Reset loadingAfterDeath bool in case it was set to true before the load process
        loadingAfterDeath = false;

        // Hide the loading panel now all loading is complete
        loadingPanel.LoadDone();

        // Restore time to its previous speed
        Time.timeScale = previousTimeScale;
    }

    private IEnumerator LoadDataForSceneCoroutine(string sceneToLoadName, string scenesDirectory, LoadingPanel loadingPanel = null)
    {
        // Scene data is being loaded
        loadingSceneData = true;

        // Load data from the current directory (set when game info is loaded) if no other directory was specified
        if (scenesDirectory == "UseCurrentDirectory") { scenesDirectory = currentScenesDirectory; }

        // Setup paths to be used to load the global and scene files
        string globalDataPath = scenesDirectory + "GlobalSave.save";
        string sceneDataPath = scenesDirectory + sceneToLoadName + ".save";

        #region Disable Player Movement

        PlayerInstance activePlayer = PlayerInstance.ActivePlayer;

        if(activePlayer != null)
        {
            // An active player exists in the new scene, ensure its controller is disabled so no movement can occur while loading
            activePlayer.PlayerController.enabled = false;

            // Enable the player camera ready to be used in the new scene
            activePlayer.PlayerMovement.GetPlayerCamera().SetActive(true);
        }

        #endregion

        yield return null; // Wait a frame to minimise stuttering while loading

        #region Read Global Data

        SaveData globalData = null; // Will be used to store loaded global data

        if (File.Exists(globalDataPath))
        {
            // A global save file exists

            FileStream globalDataFile;

            // Try to open the file for reading
            try
            {
                globalDataFile = File.OpenRead(globalDataPath);
            }
            catch
            {
                Debug.LogError("Could not open global save file when loading: " + globalDataPath);
                yield break;
            }

            yield return null; // Wait a frame to minimise stuttering while loading

            Debug.Log(">>> Attempting to load global data from: " + globalDataPath);

            // Try to deserialise the global save data from the file using a BinaryFormatter
            //   Otherwise, close the file and break from loading
            try
            {
                BinaryFormatter bf = new BinaryFormatter();

                globalData = (SaveData)bf.Deserialize(globalDataFile);
                globalDataFile.Close();
            }
            catch
            {
                globalDataFile.Close();

                Debug.LogError("Could not deserialize save data from: " + globalDataPath);
                yield break;
            }
        }

        #endregion

        yield return null; // Wait a frame to minimise stuttering while loading

        #region Read Scene Data

        SaveData sceneData = null; // Will be used to store loaded scene data

        // Same process as above, but now for the file containing scene-specific save data:

        if (File.Exists(sceneDataPath))
        {
            // A scene save file exists

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

            // Wait a frame to minimise stuttering as data is loaded
            yield return null;

            Debug.Log(">>> Attempting to load scene data from: " + sceneDataPath);

            // Try to deserialise the scene save data from the file using a BinaryFormatter
            //   Otherwise, close the file and break from loading
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
        }

        #endregion

        yield return null; // Wait a frame to minimise stuttering while loading

        // Done reading data from files, increase load progress
        UpdateLoadingProgress(loadingPanel, 0.55f);

        #region Setup Objects

        // Setup scene objects if scene data was found
        if (sceneData != null)
        {
            Debug.Log(">>> Scene load stage 1: setup");
            LoadSceneObjectsSetupEvent?.Invoke(sceneData);

            yield return null;
        }

        // Done setting up scene objects, increase load progress
        UpdateLoadingProgress(loadingPanel, 0.6f);

        // Setup global objects if scene data was found
        if (globalData != null)
        {
            Debug.Log(">>> Global load stage 1: setup");
            LoadGlobalObjectsSetupEvent?.Invoke(globalData);

            yield return null;
        }

        #endregion

        // Done setting up global objects, increase load progress
        UpdateLoadingProgress(loadingPanel, 0.7f);

        #region Configure Objects

        // Configure scene objects if scene data was found
        if (sceneData != null)
        {
            Debug.Log(">>> Scene load stage 2: configure");
            LoadSceneObjectsConfigureEvent?.Invoke(sceneData);

            yield return null;
        }

        // Done configuring scene objects, increase load progress
        UpdateLoadingProgress(loadingPanel, 0.8f);

        // Configure global objects if global data was found
        if (globalData != null)
        {
            Debug.Log(">>> Global load stage 2: configure");
            LoadGlobalObjectsConfigureEvent?.Invoke(globalData);
        }

        #endregion

        // Done configuring global objects, increase load progress
        UpdateLoadingProgress(loadingPanel, 0.9f);

        #region Move Player To Spawn

        if (WorldSave.Instance != null && WorldSave.Instance.MovePlayerToSpawnPoint())
        {
            // The player was moved to a loaded spawn point
        }
        else
        {
            // No scene save data to control player position, instead try moving the player to a default spawn point if one exists

            // Find the default spawn point

            GameObject defaultSpawnPoint = GameObject.FindGameObjectWithTag("DefaultSpawnPoint");
            if (defaultSpawnPoint != null)
            {
                // Move the plauer to the default spawn, adding Vector3.up to the spawn position so the player doesn't spawn halfway in the ground
                PlayerInstance.ActivePlayer.gameObject.transform.position = (defaultSpawnPoint.transform.position + Vector3.up);

                Debug.Log(">> Moving player to default spawn point");
            }
        }

        #endregion

        // Wait a couple of frames to allow objects to be set up before loadingSceneData is set back to false
        //   (some objects check loadingSceneData in their setup code to determine if certain behaviour should occur)
        yield return null;
        yield return null;

        #region Enable Player Movement

        if (activePlayer != null)
        {
            // Reset the player movement state in case they previously died in a non-standard state (e.g. in water)
            activePlayer.PlayerMovement.ResetMovementState();

            // Allow the player to move now loading is done
            activePlayer.PlayerController.enabled = true;
        }

        #endregion

        // Update progress to show loading is complete
        UpdateLoadingProgress(loadingPanel, 1.0f);

        // Scene data loading done
        Debug.Log(">>> Finished loading data for " + sceneToLoadName);

        loadingSceneData = false;
    }

    private void UpdateLoadingProgress(LoadingPanel loadingPanel, float value)
    {
        // Updates the load progress slider on the loading panel if one exists
        if(loadingPanel != null)
        {
            loadingPanel.UpdateLoadProgress(value);
        }
    }

    public SceneUIData GetUIDataForScene(string sceneName)
    {
        // Loop through all sceneUIData
        foreach (SceneUIData data in sceneUIData)
        {
            // Find and return data with the given scene name
            if (data.SceneName == sceneName)
            {
                return data;
            }
        }

        // No UI data for a scene with sceneName
        return null;
    }

    private void ShowTitleCardForScene(string sceneUIName)
    {
        // Instantiate title card UI
        Transform titleCardTransform = Instantiate(titleCardPrefab, GameObject.Find("JoeCanvas").transform).transform;

        // Display the UI name of the scene (setting two text objects because the title card uses an animation where text is duplicated)
        titleCardTransform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = sceneUIName;
        titleCardTransform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = sceneUIName;
    }

    public bool ResetGameData()
    {
        // Deletes any existing save files and resets quest data

        string deleteDirectory;

        if (!string.IsNullOrEmpty(currentScenesDirectory))
        {
            // The current directory has been set - delete files from it
            deleteDirectory = currentScenesDirectory;
        }
        else
        {
            // Delete files from the main directory used in builds
            deleteDirectory = baseSaveDirectory + "/" + MainSaveDirectory + "/";
        }

        Debug.LogWarning("Resetting game data - attempting to delete directory: " + deleteDirectory);

        if(Directory.Exists(deleteDirectory))
        {
            // Attempt to delete the directory containing save files if it exists
            try
            {
                Directory.Delete(deleteDirectory, true);
            }
            catch
            {
                // Directory could not be deleted
                Debug.LogError("Failed to delete directory: " + deleteDirectory);
                return false;
            }
        }

        // Also reset quest data if files were deleted
        playerData.ResetProgress();

        // Save data deleted/reset successfully
        return true;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
#if UNITY_EDITOR

        // Editor only debug code - if someone loads into a scene before currentScenesDirectory has been set,
        //   there are various predefined directories set up to make debugging easier. This way the main save
        //   files used in builds will not be read/written to when doing tests or building scenes

        if (string.IsNullOrEmpty(currentScenesDirectory))
        {
            // If entering JoeTestScene, use the directory: Debug_JoeTestScenes
            if (scene.name == "JoeTestScene")
            {
                Debug.Log("No save directory was set before entering the scene - using Debug_JoeTestScenes");

                currentScenesDirectory = baseSaveDirectory + "/" + "Debug_JoeTestScenes" + "/";
                StartCoroutine(LoadDataForSceneCoroutine(scene.name, currentScenesDirectory));
            }

            // If entering one of the maps, use the directory: Debug_MapScenes
            else if (scene.name == "The Village" || scene.name == "Desert" || scene.name == "Flooded City" || scene.name == "Catacombs")
            {
                Debug.Log("No save directory was set before entering the scene - using Debug_MapScenes");

                currentScenesDirectory = baseSaveDirectory + "/" + "Debug_MapScenes" + "/";
                StartCoroutine(LoadDataForSceneCoroutine(scene.name, currentScenesDirectory));
            }
        }

        #endif
    }

    #region PlayerPrefs

    // Saving/loading PlayerPrefs
    //============================

    public void SaveIntToPlayerPrefs(string key, int value)
    {
        // Saves the int value to PlayerPrefs

        PlayerPrefs.SetInt(key, value);
        PlayerPrefs.Save();

        if(key == "musicVolume")
        {
            // If the music volume is being saved, also update the value in AudioManager
            //   (more efficient than AudioManager having to call GetIntFromPlayerPrefs each time the saved volume is needed)
            AudioManager.Instance.SavedMusicVolume = value;
        }
        else if(key == "soundEffectsVolume")
        {
            // If the sound effect volume is being saved, also update the value in AudioManager
            //   (more efficient than AudioManager having to call GetIntFromPlayerPrefs each time the saved volume is needed)
            AudioManager.Instance.SavedSoundEffectsVolume = value;
        }
    }

    public int GetIntFromPlayerPrefs(string key)
    {
        if(playerPrefsDefaultValues.ContainsKey(key))
        {
            // Return the saved int value, or the default value from playerPrefsDefaultValues if not yet saved
            return PlayerPrefs.GetInt(key, playerPrefsDefaultValues[key]);
        }
        else
        {
            Debug.LogWarning("No default int value set for PlayerPref with key: " + key);

            // No default value in playerPrefsDefaultValues, return the saved value or the PlayerPrefs default of 0
            return PlayerPrefs.GetInt(key);
        }
    }

    public void SaveBoolToPlayerPrefs(string key, bool value)
    {
        // Saves the bool value to PlayerPrefs by converting to 1 (true) or 0 (false) since PlayerPrefs does not support bools

        PlayerPrefs.SetInt(key, value == true ? 1 : 0);
        PlayerPrefs.Save();
    }

    public bool GetBoolFromPlayerPrefs(string key)
    {
        // Note: PlayerPrefs does not directly support bools, so an int value of either 1 or 0
        //   is loaded and the comparison (value == 1) is used to return a true/false value

        if (playerPrefsDefaultValues.ContainsKey(key))
        {
            // Return the saved value, or the default value from playerPrefsDefaultValues if not yet saved
            return (PlayerPrefs.GetInt(key, playerPrefsDefaultValues[key]) == 1);
        }
        else
        {
            Debug.LogWarning("No default int value set for PlayerPref with key: " + key);

            // No default value in playerPrefsDefaultValues, return the saved value or the PlayerPrefs default of 0
            return (PlayerPrefs.GetInt(key) == 1);
        }
    }

    #endregion

    // SceneUIData contains the display name and sprite to be displayed for a scene
    //   with name: [SceneName] when info has to be shown about it in the user interface
    // =================================================================================

    [Serializable]
    public class SceneUIData
    {
        #region Properties

        public string SceneName { get { return sceneName; } }
        public string UIName    { get { return uiName; } }
        public Sprite UISprite  { get { return uiSprite; } }

        #endregion

        // See tooltips for comments

        [SerializeField]
        [Tooltip("The name used for the scene within the Unity editor")]
        private string sceneName;

        [SerializeField]
        [Tooltip("The name to be displayed in the UI when loading/displaying the scene")]
        private string uiName;

        [SerializeField]
        [Tooltip("The sprite to be displayed in the UI when loading/displaying the scene")]
        private Sprite uiSprite;
    }
}