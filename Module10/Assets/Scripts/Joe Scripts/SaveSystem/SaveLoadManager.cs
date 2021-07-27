using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;

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

    [SerializeField] private GameObject    loadingCanvasPrefab; // UI canvas instantiated when loading a scene
    [SerializeField] private GameObject    titleCardPrefab;     // UI element that shows the name of a scene when one is loaded
    [SerializeField] private SceneUIData[] sceneUIData;         // Array of data determining what names and images will be shown in the UI to represent each scene

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

    private static ISavePoint lasUsedSavePoint = null;

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

    #endregion

    public static void SetLastUsedSavePoint(ISavePoint savePoint)
    {
        if(lasUsedSavePoint != null)
        {
            lasUsedSavePoint.SetAsUnused();
        }

        lasUsedSavePoint = savePoint;
    }

    public bool SaveGameData(string returnToSceneName = "UseActiveScene", string scenesDirectory = "UseCurrentDirectory")
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

        if(returnToSceneName == "UseActiveScene")
        {
            returnToSceneName = loadedSceneName;
        }

        infoDataToSave.AddData("lastLoadedScene",   returnToSceneName);
        infoDataToSave.AddData("loadedSceneUIName", returnToSceneName);

        if (PlayerInstance.ActivePlayer != null)
        {
            infoDataToSave.AddData("playerName", PlayerStats.PlayerName);
        }

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

    public bool LoadGameInfo(out string sceneToLoadName, out string savedPlayerName, out string savedAreaName,
                                string saveDirectoryName = MainSaveDirectory, string defaultStartingSceneName = MainStartingSceneName)
    {
        // STEP 1: Attempt to load the global save file for the specified scene group
        //============================================================================

        string directoryPath = baseSaveDirectory + "/" + saveDirectoryName + "/";
        string infoDataPath = directoryPath + "LoadInfo.save";

        sceneToLoadName = defaultStartingSceneName;
        savedPlayerName = null;
        savedAreaName   = null;

        currentScenesDirectory = directoryPath;

        if (File.Exists(infoDataPath))
        {
            // Global and LoadInfo save files exist - open them and load save data

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

            // Try to deserialise the LoadInfo save data from the file
            try
            {
                BinaryFormatter bf = new BinaryFormatter();

                infoData = (SaveData)bf.Deserialize(infoDataFile);
                infoDataFile.Close();
            }
            catch
            {
                infoDataFile.Close();

                Debug.LogError("Could not deserialize save data from: " + infoDataPath);
                return false;
            }

            sceneToLoadName = infoData.GetData<string>("lastLoadedScene");
            savedPlayerName = infoData.GetData<string>("playerName");
            savedAreaName   = infoData.GetData<string>("loadedSceneUIName");

            PlayerStats.PlayerName = savedPlayerName;

            return true;
        }
        else
        {
            // No info save file, this should only occur when first loading a scene group with no previously saved data

            Debug.Log("No saved data found for save directory: " + saveDirectoryName);

            return false;
        }
    }

    public void LoadGameScene(string sceneName, string scenesDirectory = "UseCurrentDirectory")
    {
        StartCoroutine(LoadGameSceneCoroutine(sceneName, scenesDirectory));
    }

    private IEnumerator LoadGameSceneCoroutine(string sceneName, string scenesDirectory)
    {
        lasUsedSavePoint = null;

        // Pause time

        float previousTimeScale = Time.timeScale;
        Time.timeScale = 0.0f;

        // Setup loading panel

        LoadingPanel loadingPanel = Instantiate(loadingCanvasPrefab).GetComponent<LoadingPanel>();

        SceneUIData sceneUIData = GetUIDataForScene(sceneName);

        loadingPanel.SetAreaNameText(sceneUIData.UIName);
        loadingPanel.SetAreaPreviewSprite(sceneUIData.UISprite);

        // Mute global music/sound

        AudioManager audioManager = AudioManager.Instance;

        audioManager.FadeGlobalVolumeMultiplier(0.0f, 1.0f);

        // Disable player controller

        PlayerInstance activePlayer = PlayerInstance.ActivePlayer;

        if (activePlayer != null && activePlayer.PlayerController != null)
        {
            activePlayer.PlayerController.enabled = false;
        }

        // Wait a second for the loading panel to fade in
        yield return new WaitForSecondsRealtime(1.2f);

        AsyncOperation sceneLoadOperation = SceneManager.LoadSceneAsync(sceneName);

        while (!sceneLoadOperation.isDone)
        {
            UpdateLoadingProgress(loadingPanel, sceneLoadOperation.progress * 0.555f); // Converts from 0.0 - 0.9, to 0.0 - 0.5

            yield return null;
        }

        audioManager.UpdateGlobalVolumeMultiplier(0.0f);

        // Scene loaded

        // Load and setup saved global/scene data
        //========================================
        yield return StartCoroutine(LoadDataForSceneCoroutine(sceneName, sceneUIData, scenesDirectory, loadingPanel));

        loadingPanel.LoadDone();

        Time.timeScale = previousTimeScale;
    }

    private IEnumerator LoadDataForSceneCoroutine(string sceneToLoadName, SceneUIData sceneUIData, string scenesDirectory, LoadingPanel loadingPanel = null)
    {
        loadingSceneData = true;

        // Wait a frame to ensure the player instance is ready
        yield return null;

        // In case no active player was found before loading the scene, get a reference to it now a
        //   scene containing the player has been loaded so its controller can definitely be disabled
        PlayerInstance activePlayer = PlayerInstance.ActivePlayer;
        PlayerMovement playerMovement = null;

        if(activePlayer != null)
        {
            activePlayer.PlayerController.enabled = false;

            playerMovement = activePlayer.GetComponent<PlayerMovement>();

            // Enable the player camera ready to be used in the new scene
            playerMovement.GetPlayerCamera().SetActive(true);
        }

        if (scenesDirectory == "UseCurrentDirectory")
        {
            scenesDirectory = currentScenesDirectory;
        }

        string globalDataPath   = scenesDirectory + "GlobalSave.save";
        string sceneDataPath    = scenesDirectory + sceneToLoadName + ".save";

        SaveData globalData = null;
        SaveData sceneData = null;

        if(File.Exists(globalDataPath))
        {
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

            yield return null;

            Debug.Log(">>> Attempting to load global data from: " + globalDataPath);

            // Try to deserialise the Global save data from the file
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

            yield return null;
        }

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

            yield return null;

            Debug.Log(">>> Attempting to load scene data from: " + sceneDataPath);

            // Try to deserialise the scene save data from the file
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
        }

        UpdateLoadingProgress(loadingPanel, 0.55f);

        // Setup scene objects
        if (sceneData != null)
        {
            Debug.Log(">>> Scene load stage 1: setup");
            LoadSceneObjectsSetupEvent?.Invoke(sceneData);

            yield return null;
        }

        UpdateLoadingProgress(loadingPanel, 0.6f);

        // Setup global objects
        if (globalData != null)
        {
            Debug.Log(">>> Global load stage 1: setup");
            LoadGlobalObjectsSetupEvent?.Invoke(globalData);

            yield return null;
        }

        UpdateLoadingProgress(loadingPanel, 0.7f);

        // Configure scene objects
        if (sceneData != null)
        {
            Debug.Log(">>> Scene load stage 2: configure");
            LoadSceneObjectsConfigureEvent?.Invoke(sceneData);

            yield return null;
        }

        UpdateLoadingProgress(loadingPanel, 0.8f);

        // Configure global objects
        if (globalData != null)
        {
            Debug.Log(">>> Global load stage 2: configure");
            LoadGlobalObjectsConfigureEvent?.Invoke(globalData);
        }

        UpdateLoadingProgress(loadingPanel, 0.9f);

        if (WorldSave.Instance != null && WorldSave.Instance.MovePlayerToSpawnPoint())
        {
            // Player was moved to a loaded spawn point
        }
        else
        {
            // No scene save data to control player position, instead try moving the player to a default spawn point if one exists

            GameObject defaultSpawnPoint = GameObject.Find("DefaultSpawnPoint");
            if (defaultSpawnPoint != null)
            {
                // Adding Vector3.up to the spawn position so the player doesn't spawn halfway in the ground
                PlayerInstance.ActivePlayer.gameObject.transform.position = (defaultSpawnPoint.transform.position + Vector3.up);

                Debug.Log(">> Moving player to default spawn point");
            }
        }

        if(activePlayer != null)
        {
            // Reset the player movement state in case they died in a non-standard state (e.g. in water)
            playerMovement.ResetMovementState();

            // Allow the player to move now loading is done
            activePlayer.PlayerController.enabled = true;
        }

        UpdateLoadingProgress(loadingPanel, 1.0f);

        // Show the title card if loading into the scene (unless reloading after death)
        if (!loadingAfterDeath)
        {
            if(sceneUIData != null && !string.IsNullOrEmpty(sceneUIData.UIName))
            {
                ShowTitleCardForScene(sceneUIData.UIName);
            }
        }

        loadingAfterDeath = false;

        // Scene data loading done
        Debug.Log(">>> Finished loading data for " + sceneToLoadName);

        loadingSceneData = false;
    }

    private void UpdateLoadingProgress(LoadingPanel loadingPanel, float value)
    {
        if(loadingPanel != null)
        {
            loadingPanel.UpdateLoadProgress(value);
        }
    }

    public SceneUIData GetUIDataForScene(string sceneName)
    {
        foreach (SceneUIData data in sceneUIData)
        {
            if (data.SceneName == sceneName)
            {
                return data;
            }
        }

        return null;
    }

    private void ShowTitleCardForScene(string sceneUIName)
    {
        Transform titleCardTransform = Instantiate(titleCardPrefab, GameObject.Find("JoeCanvas").transform).transform;

        titleCardTransform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = sceneUIName;
        titleCardTransform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = sceneUIName;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        #if UNITY_EDITOR

        if (string.IsNullOrEmpty(currentScenesDirectory))
        {
            if (scene.name == "JoeTestScene" || scene.name == "JoeTestScene2")
            {
                Debug.LogWarning("No save directory was set before entering the scene - using Debug_JoeTestScenes");

                currentScenesDirectory = baseSaveDirectory + "/" + "Debug_JoeTestScenes" + "/";
                StartCoroutine(LoadDataForSceneCoroutine(scene.name, GetUIDataForScene(scene.name), currentScenesDirectory));
            }
            else if (scene.name == "The Village" || scene.name == "Desert" || scene.name == "Flooded City" || scene.name == "Catacombs")
            {
                Debug.LogWarning("No save directory was set before entering the scene - using Debug_MapScenes");

                currentScenesDirectory = baseSaveDirectory + "/" + "Debug_MapScenes" + "/";
                StartCoroutine(LoadDataForSceneCoroutine(scene.name, GetUIDataForScene(scene.name), currentScenesDirectory));
            }
            else
            {
                Debug.LogWarning("No save directory was set before entering the scene - using Debug_" + scene.name);

                currentScenesDirectory = baseSaveDirectory + "/" + "Debug_" + scene.name + "/";
                StartCoroutine(LoadDataForSceneCoroutine(scene.name, GetUIDataForScene(scene.name), currentScenesDirectory));
            }
        }

        #endif
    }

    #region PlayerPrefs

    // Saving/loading PlayerPrefs
    //============================

    public void SaveIntToPlayerPrefs(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
        PlayerPrefs.Save();

        if(key == "musicVolume")
        {
            AudioManager.Instance.SavedMusicVolume = value;
        }
        else if(key == "soundEffectsVolume")
        {
            AudioManager.Instance.SavedSoundEffectsVolume = value;
        }
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

    #endregion

    [Serializable]
    public class SceneUIData
    {
        public string SceneName { get { return sceneName; } }
        public string UIName { get { return uiName; } }
        public Sprite UISprite { get { return uiSprite; } }

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