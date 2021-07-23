using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//MusicPlayMode defines all of the different ways music can be played for a certain scene
public enum MusicPlayMode
{
    OrderedPlaylist,    // A playlist where songs are played in the listed order

    RandomPlaylist,     // A playlist where songs are played in a random order
                        //   (if there are multiple songs in the playlist, each song never being played more than once in a row)

    LoopSingleTrack,    // One track is looped for the duration of the scene

    Dynamic,            // The music played in the scene is determined by the placement of DynamicAudioAreas

    None                // No music will be played in the scene
}

// ||=======================================================================||
// || AudioManager: Handles playing music and sound effects                 ||
// ||=======================================================================||
// || Used on prefab: Joe/Audio/_AudioManager                               ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

// Edited for mod11:
// - PlaySoundEffect now returns audiosource, useful for e.g. keeping reference to + changing position of looping source while playing
// - min/max distance can be changed for 3d sources
// - globalVolumeMultiplier, for fading music in/out independent of saved volume values

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private SoundClass[]   sounds;             // All sound effects that can be played
    [SerializeField] private SceneMusic[]   sceneMusicSetup;    // Array of data defining how music should be handled in each game scene

    [SerializeField] private AudioSource    musicSource;        // The audio source used to play non-dynamic music
    [SerializeField] private GameObject     soundSourcePrefab;  // The prefab instantiated when a sound is played

    #endregion

    #region Properties

    public SceneMusic               CurrentSceneMusic       { get { return currentSceneMusic; } }       
    public List<DynamicAudioArea>   ActiveDynamicAudioAreas { get { return activeDynamicAudioAreas; }
                                                              set { activeDynamicAudioAreas = value; } }
    public float            GlobalVolumeMultiplier  { get { return globalVolumeMultiplier; } }

    public int              SavedMusicVolume        { set { savedMusicVolume = value; } }
    public int              SavedSoundEffectsVolume { set { savedSoundEffectsVolume = value; } }

    #endregion

    private Dictionary<string, SoundClass>  soundsDict;                     // Dictionary containing sound effects indexed by their string id's
    private SceneMusic                      currentSceneMusic;              // Defines how music is handled in the loaded scene
    private int                             currentPlaylistIndex;           // If using a playlist, the index of the song that is currently playing in it
    private DynamicAudioArea[,]             dynamicAudioAreas;              // All (if any) dynamic audio areas in the loaded scene
    private List<DynamicAudioArea>          activeDynamicAudioAreas;        // The dynamic audio areas that are currently playing audio
    private List<LoopingSoundSource>        loopingSoundSources;            // List of all active looping sound sources
    private float                           globalVolumeMultiplier = 1.0f;  // Volume multiplier for fading music and sounds in/out without altering saved volume values
    private Coroutine                       fadeGlobalVolumeCoroutine;

    private int savedMusicVolume        = -1;   // Keeps track of the saved music volume so PlayerPrefs don't have to be checked each time (set from SaveLoadManager)
    private int savedSoundEffectsVolume = -1;   // Same as above, but for sound effects. (If these values are set to -1 the actual value will be loaded from PlayerPrefs)

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

        loopingSoundSources = new List<LoopingSoundSource>();

        // Add all sounds to the sound dictionary
        SetupSoundDictionary();

        activeDynamicAudioAreas = new List<DynamicAudioArea>();

        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void SetupSoundDictionary()
    {
        // Add all sounds to the dictionary, indexed by their id's
        //   so they can easily be returned when needed
        soundsDict = new Dictionary<string, SoundClass>();

        for (int i = 0; i < sounds.Length; i++)
        {
            soundsDict.Add(sounds[i].Id, sounds[i]);
        }
    }

    private void Update()
    {
        if(!musicSource.isPlaying)
        {
            if (currentSceneMusic.PlayMode == MusicPlayMode.OrderedPlaylist || currentSceneMusic.PlayMode == MusicPlayMode.RandomPlaylist)
            {
                // If the current music track has finished playing and a playlist is being used, move to the next song (either sequentially or randomly)
                PlayNextMusicFromPlaylist();
            }
        }
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        // Ensure there are no active dynamic audio areas by default, in case any were set in a previous scene
        activeDynamicAudioAreas.Clear();

        // Set playmode to none by default in case none was set in the inspector
        currentSceneMusic = new SceneMusic(scene.name, MusicPlayMode.None, null);

        // Stop the music source if it's playing ready to set up music for the loaded scene
        if (musicSource.isPlaying)
        {
            musicSource.Stop();
        }

        for (int i = 0; i < sceneMusicSetup.Length; i++)
        {
            // Find the scene music setup for the loaded scene
            if(sceneMusicSetup[i].SceneName == scene.name)
            {
                SceneMusic sceneMusic = sceneMusicSetup[i];
                currentSceneMusic = sceneMusic;

                if(sceneMusic.PlayMode == MusicPlayMode.OrderedPlaylist || sceneMusic.PlayMode == MusicPlayMode.RandomPlaylist)
                {
                    // Play a track from the playlist, random or otherwise
                    currentPlaylistIndex = -1;  // Starts at -1 since PlayNextMusicFromPlaylist will begin by incrementing the index to 0
                    PlayNextMusicFromPlaylist();
                }
                else if(sceneMusic.PlayMode == MusicPlayMode.LoopSingleTrack)
                {
                    // Loop the first (and ideally only) track in the playlist for the duration of the scene
                    currentPlaylistIndex = 0;
                    PlayMusic(sceneMusic.Playlist[0], true);
                }
                else if(sceneMusic.PlayMode == MusicPlayMode.Dynamic)
                {
                    // Find all dynamic audio areas in the scene
                    GameObject[] dynamicAudioGameObjs = GameObject.FindGameObjectsWithTag("DynamicAudioArea");

                    // Setup the dynamic audio areas array based on the number of areas in the scene
                    dynamicAudioAreas = new DynamicAudioArea[DynamicAudioArea.MaxDynamicAudioLayers, dynamicAudioGameObjs.Length];

                    for (int j = 0; j < dynamicAudioGameObjs.Length; j++)
                    {
                        DynamicAudioArea areaToAdd = dynamicAudioGameObjs[j].GetComponent<DynamicAudioArea>();

                        // Add all dynamic audio areas to the array
                        dynamicAudioAreas[areaToAdd.DynamicAudioLayer, j] = areaToAdd;
                    }

                    // Play all dynamic audio sources so they are in sync
                    for (int j = 0; j < DynamicAudioArea.MaxDynamicAudioLayers; j++)
                    {
                        PlayAllDynamicSourcesOnLayer(j);
                    }
                }

                break;
            }
        }

        globalVolumeMultiplier = 1.0f;

        UpdateMusicSourcesVolume();
    }

    public void PlayMusic(MusicClass music, bool loop)
    {
        // Play the given music track, adjusting volume/loop to match parameters

        musicSource.clip    = music.AudioClip;
        musicSource.volume  = music.Volume;
        musicSource.loop    = loop;

        musicSource.Play();
    }

    private void PlayNextMusicFromPlaylist()
    {
        // Set the currentPlaylistIndex so that the next song in the playlist is played,
        //   either sequentially for an OrderedPlaylist or randomly for a RandomPlaylist

        switch (currentSceneMusic.PlayMode)
        {
            case MusicPlayMode.OrderedPlaylist:
            {
                if (currentPlaylistIndex < (currentSceneMusic.Playlist.Length - 1))
                {
                    // Go to the next track in the playlist
                    currentPlaylistIndex++;
                }
                else
                {
                    // Final track was played, go back to the first track
                    currentPlaylistIndex = 0;
                }
            }
            break;

            case MusicPlayMode.RandomPlaylist:
            {
                int previousIndex = currentPlaylistIndex;

                if (currentSceneMusic.Playlist.Length > 1)
                {
                    // Pick a random index that is different to the last one so a new track is played
                    while (currentPlaylistIndex == previousIndex)
                    {
                        currentPlaylistIndex = Random.Range(0, currentSceneMusic.Playlist.Length);
                    }
                }
                else
                {
                    // There is only one track to play, no randomisation needed
                    currentPlaylistIndex = 0;
                }
            }
            break;
        }

        // Play a track from the playlist based on the index set above
        PlayMusic(currentSceneMusic.Playlist[currentPlaylistIndex], false);
    }

    private AudioSource PlaySoundEffect(SoundClass sound, LoopType loopType, bool overrideGlobalVolumeMultiplier, bool use3DSpace, bool bypassEffects,
                                        Vector3 sourcePosition = default, float min3dDistance = 0.0f, float max3dDistance = 0.0f, bool useMusicVolume = false)
    {
        // Pick a random volume/sound within the set ranges
        float volume    = Random.Range(sound.VolumeRange.Min, sound.VolumeRange.Max);
        float pitch     = Random.Range(sound.PitchRange.Min, sound.PitchRange.Max);

        // Create the sound source GameObject
        GameObject sourceGameObj = Instantiate(soundSourcePrefab, sourcePosition, Quaternion.identity, transform);
        sourceGameObj.name = "Sound_" + sound.Id;

        // Set AudioSource values based on given parameters
        AudioSource audioSource = sourceGameObj.GetComponent<AudioSource>();

        audioSource.clip        = sound.AudioClips[Random.Range(0, sound.AudioClips.Length)];

        float globalVolume = globalVolumeMultiplier;
        if(overrideGlobalVolumeMultiplier)
        {
            globalVolume = 1.0f;
        }

        float savedVolume;
        if(useMusicVolume)
        {
            savedVolume = SaveLoadManager.Instance.GetIntFromPlayerPrefs("musicVolume");
        }
        else
        {
            savedVolume = SaveLoadManager.Instance.GetIntFromPlayerPrefs("soundEffectsVolume");
        }

        // Multiply the chosen volume value by the saved overall sound effects volume (which is stored as a value from 0 - 20)
        audioSource.volume      = volume * savedVolume * 0.05f * globalVolume;

        audioSource.pitch       = pitch;

        if (use3DSpace)
        {
            // Enable spatialBlend if playing sound in 3D space, so it will sound like it originates from sourcePosition
            audioSource.spatialBlend = 1.0f;

            if(min3dDistance > 0.0f)
            {
                audioSource.minDistance = min3dDistance;
            }
            if (max3dDistance > 0.0f)
            {
                audioSource.maxDistance = max3dDistance;
            }
        }

        audioSource.bypassListenerEffects = bypassEffects;

        if (loopType.LoopEnabled)
        {
            // If looping, set the audioSource to loop and give it an identifiable name so it can later be stopped/deleted
            sourceGameObj.name = "LoopSound_" + loopType.LoopId;
            audioSource.loop = true;

            // Add the source GameObject to the list of looping sounds
            loopingSoundSources.Add(new LoopingSoundSource(audioSource, volume));
        }

        // Play the sound
        audioSource.Play();

        return audioSource;
    }

    private AudioSource PlaySoundEffect(string id, LoopType loopType, bool overrideGlobalVolumeMultiplier, bool use3DSpace, bool bypassEffects,
                                        Vector3 sourcePosition = default, float min3dDistance = 0.0f, float max3dDistance = 0.0f, bool useMusicVolume = false)
    {
        if (soundsDict.ContainsKey(id))
        {
            // Play a sound with the given parameters
            return PlaySoundEffect(soundsDict[id], loopType, overrideGlobalVolumeMultiplier, use3DSpace, bypassEffects, sourcePosition, min3dDistance, max3dDistance, useMusicVolume);
        }
        else
        {
            Debug.LogError("Trying to play sound effect with invalid id: " + id);
            return default;
        }
    }

    public AudioSource PlayLoopingSoundEffect(string soundId, string loopId, bool use3DSpace = false, bool bypassEffects = false,
                                                Vector3 sourcePosition = default, float min3dDistance = 0.0f, float max3dDistance = 0.0f)
    {
        // Starts playing a sound with soundId that will loop until StopLoopingSoundEffect is called with the given loopId
        return PlaySoundEffect(soundId, LoopType.Loop(loopId), false, use3DSpace, bypassEffects, sourcePosition, min3dDistance, max3dDistance);
    }

    public AudioSource PlayLoopingSoundEffect(SoundClass sound, string loopId, bool use3DSpace = false, bool bypassEffects = false,
                                                Vector3 sourcePosition = default, float min3dDistance = 0.0f, float max3dDistance = 0.0f)
    {
        // Starts playing a sound that will loop until StopLoopingSoundEffect is called with the given loopId
        return PlaySoundEffect(sound, LoopType.Loop(loopId), false, use3DSpace, bypassEffects, sourcePosition, min3dDistance, max3dDistance);
    }

    public void StopLoopingSoundEffect(string loopId)
    {
        // Finds a looping sound with the given loopId and destroys its audio source to stop it

        foreach(LoopingSoundSource loopSource in loopingSoundSources)
        {
            if (loopSource.Source.gameObject.name == "LoopSound_" + loopId)
            {
                // Found a matching loop source

                // Remove it from the array of looping source GameObjects
                loopingSoundSources.Remove(loopSource);

                // Destroy the source GameObject to stop the sound
                Destroy(loopSource.Source.gameObject);

                // Done, no need to continue
                return;
            }
        }
    }

    public LoopingSoundSource GetLoopingSoundSourceFromId(string loopId)
    {
        foreach (LoopingSoundSource loopSource in loopingSoundSources)
        {
            if (loopSource.Source.gameObject.name == "LoopSound_" + loopId)
            {
                return loopSource;
            }
        }

        return null;
    }

    public void SetLoopingSoundBaseVolume(LoopingSoundSource loopingSource, float volume)
    {
        // Saved volume is stored as an int between 0 and 20, multiplying to get a float from 0.0 to 1.0
        float volumeMultiplier = GetSavedSoundEffectsVolume() * 0.05f * globalVolumeMultiplier;

        loopingSource.BaseVolume = volume;
        loopingSource.Source.volume = loopingSource.BaseVolume * volumeMultiplier;
    }

    public void StopAllLoopingSoundEffects()
    {
        foreach (LoopingSoundSource loopSource in loopingSoundSources)
        {
            // Destroy all source GameObjects to stop the sounds
            Destroy(loopSource.Source.gameObject);
        }

        // There are now no looping sounds, clear the looping sound sources list
        loopingSoundSources.Clear();
    }

    public void PlaySoundEffect2D(string id, bool overrideGlobalVolumeMultiplier = false)
    {
        // Plays a sound with the given id that is not positioned in 3D space
        PlaySoundEffect(id, LoopType.DoNotLoop, overrideGlobalVolumeMultiplier, false, true);
    }

    public void PlaySoundEffect2D(SoundClass sound, bool overrideGlobalVolumeMultiplier = false)
    {
        // Plays the given sound, not positioned in 3D space
        PlaySoundEffect(sound, LoopType.DoNotLoop, overrideGlobalVolumeMultiplier, false, true);
    }

    public void PlaySoundEffect3D(string id, Vector3 sourcePosition, float min3dDistance = 0.0f, float max3dDistance = 0.0f, bool overrideGlobalVolumeMultiplier = false)
    {
        // Plays a sound with the given id positioned in 3D space at sourcePosition
        PlaySoundEffect(id, LoopType.DoNotLoop, overrideGlobalVolumeMultiplier, true, false, sourcePosition, min3dDistance, max3dDistance);
    }

    public void PlaySoundEffect3D(SoundClass sound, Vector3 sourcePosition, float min3dDistance = 0.0f, float max3dDistance = 0.0f, bool overrideGlobalVolumeMultiplier = false)
    {
        // Plays the given sound, positioned in 3D space at sourcePosition
        PlaySoundEffect(sound, LoopType.DoNotLoop, overrideGlobalVolumeMultiplier, true, false, sourcePosition, min3dDistance, max3dDistance);
    }

    public void PlayMusicInterlude(string id)
    {
        // For playing music that does not act as background music, but instead as a short musical interlude,
        //   for example when a cutscene is being played. Plays like a sound effect on top of background music, hence the use of PlaySoundEffect
        //   Also overrides globalVolumeMultiplier, allowing background music to be faded out while the interlude plays if desired

        PlaySoundEffect(id, LoopType.DoNotLoop, true, false, true, default, 0, 0, true);
    }

    public void PlayAllDynamicSourcesOnLayer(int dynamicAudioLayer)
    {
        if(dynamicAudioLayer < 0 || dynamicAudioLayer > (DynamicAudioArea.MaxDynamicAudioLayers - 1))
        {
            Debug.LogError("Invalid dynamicAudioLayer given to PlayAllDynamicAudioSources: " + dynamicAudioLayer);
            return;
        }

        // Plays all dynamic sources at the same time. Dynamic sources should
        //   always be played at the same time so they remain synchronised
        for (int i = 0; i < dynamicAudioAreas.GetLength(1); i++)
        {
            if(dynamicAudioAreas[dynamicAudioLayer, i] != null)
            {
                dynamicAudioAreas[dynamicAudioLayer, i].MusicSource.Play();
            }
        }
    }

    public void RevertActiveDynamicAudioAreas()
    {
        foreach (DynamicAudioArea area in dynamicAudioAreas)
        {
            if(area != null && area.WaitingForRevert)
            {
                if(!area.Active)
                {
                    area.ActivateAudioArea(false);
                }
                area.WaitingForRevert = false;
            }
        }
    }

    public void FadeGlobalVolumeMultiplier(float fadeTo, float fadeTime)
    {
        if(fadeGlobalVolumeCoroutine != null)
        {
            StopCoroutine(fadeGlobalVolumeCoroutine);
        }
        fadeGlobalVolumeCoroutine = StartCoroutine(FadeGlobalVolumeMultiplierCoroutine(fadeTo, fadeTime, fadeTo > globalVolumeMultiplier));
    }

    private IEnumerator FadeGlobalVolumeMultiplierCoroutine(float fadeTo, float fadeTime, bool fadingIn)
    {
        float increaseMultiplier = fadingIn ? 1.0f : -1.0f;

        while((fadingIn && (globalVolumeMultiplier < fadeTo)) || ((!fadingIn) && (globalVolumeMultiplier > fadeTo)))
        {
            UpdateGlobalVolumeMultiplier(globalVolumeMultiplier + increaseMultiplier * Time.unscaledDeltaTime / fadeTime);
            yield return null;
        }

        UpdateGlobalVolumeMultiplier(fadeTo);

        fadeGlobalVolumeCoroutine = null;
    }

    public void UpdateGlobalVolumeMultiplier(float value)
    {
        if(value < 0.0f)
        {
            value = 0.0f;
        }

        globalVolumeMultiplier = value;

        UpdateMusicSourcesVolume();
        UpdateActiveLoopingSoundsVolume();
    }

    public void UpdateMusicSourcesVolume()
    {
        // Updates the volume of the main music source and any active
        //   dynamic audio sources

        // Saved volume is stored as an int between 0 and 20, multiplying to get a float from 0.0 to 1.0
        float volumeVal = GetSavedMusicVolume() * 0.05f * globalVolumeMultiplier;

        // Set the volume of the main audio source used for playing music
        musicSource.volume = volumeVal;

        // Also update the volume of all active dynamic sources
        if (currentSceneMusic.PlayMode == MusicPlayMode.Dynamic)
        {
            foreach (DynamicAudioArea area in dynamicAudioAreas)
            {
                if(area != null)
                {
                    area.UpdateSourceVolume(volumeVal);
                }
            }
        }
    }

    public void UpdateActiveLoopingSoundsVolume()
    {
        // Updates the volume of all looping sound effects that are currently playing

        // Saved volume is stored as an int between 0 and 20, multiplying to get a float from 0.0 to 1.0
        float volumeVal = GetSavedSoundEffectsVolume() * 0.05f * globalVolumeMultiplier;

        // Update the volume of all active loop sources, multiplying by the base
        //   volume that was chosen when the sound first started playing
        foreach (LoopingSoundSource loopSource in loopingSoundSources)
        {
            loopSource.Source.volume = volumeVal * loopSource.BaseVolume;
        }
    }

    private float GetSavedSoundEffectsVolume()
    {
        if (savedSoundEffectsVolume == -1)
        {
            savedSoundEffectsVolume = SaveLoadManager.Instance.GetIntFromPlayerPrefs("soundEffectsVolume");
        }

        return savedSoundEffectsVolume;
    }

    private float GetSavedMusicVolume()
    {
        if (savedMusicVolume == -1)
        {
            savedMusicVolume = SaveLoadManager.Instance.GetIntFromPlayerPrefs("musicVolume");
        }

        return savedMusicVolume;
    }
}

// SceneMusic: Contains data about how/which music will be played for a certian scene
//===================================================================================

[System.Serializable]
public struct SceneMusic
{
    // Constructor
    public SceneMusic(string sceneName, MusicPlayMode playMode, MusicClass[] playlist)
    {
        SceneName = sceneName;
        PlayMode  = playMode;
        Playlist  = playlist;
    }

    public string           SceneName;  // Name of the scene to use these settings for
    public MusicPlayMode    PlayMode;   // The play mode to use in the scene
    public MusicClass[]     Playlist;   // The playlist used if the chosen PlayMode is OrderedPlaylist or RandomPlaylist
}

// LoopType: Defines whether a sound will loop, and holds a unique id to use if looping
//=====================================================================================

public class LoopType
{
    #region Properties

    public          bool        LoopEnabled { get { return m_loopEnabled; } }
    public          string      LoopId      { get { return m_loopId; } }
    public static   LoopType    DoNotLoop   { get { return m_doNotLoop; } }

    #endregion

    // Member variables

    private         bool     m_loopEnabled  = false;            // Whether the sound should loop
    private         string   m_loopId       = "";               // Unique id used to find/stop a looping sound at any point after it's started playing
    private static  LoopType m_doNotLoop    = new LoopType();   // Standard object to return for setting up non-looping sounds

    // Setup a looping sound type
    public static LoopType Loop(string loopId) { return new LoopType()
                                               {
                                                   m_loopEnabled = true,
                                                   m_loopId = loopId
                                               };}
}

// LoopingSoundSource: Contains info about a sound effect that is currently looping
//=================================================================================

public class LoopingSoundSource
{
    public LoopingSoundSource(AudioSource source, float baseVolume)
    {
        Source = source;
        BaseVolume = baseVolume;
    }

    public AudioSource Source;      // The audio source playing the looping sound
    public float       BaseVolume;  // The volume chosen when the sound was started
}