using System.Collections.Generic;
using UnityEngine;

// ||=======================================================================||
// || DynamicAudioArea: Triggers a dynamic audio track when the player      ||
// ||   enters its area if the active scene is using MusicPlayMode.Dynamic  || 
// ||=======================================================================||
// || Used on prefab: Joe/Audio/DynamicAudioArea                            ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

// Edited for mod 11: added dynamicAudioLayer functionality

public class DynamicAudioArea : MonoBehaviour, IPersistentSceneObject
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    // See tooltips for comments

    [Header("Dynamic Audio Area")]

    [SerializeField] private AudioSource musicSource; // The audio source used to play the triggered music

    [SerializeField] [Tooltip("The music to be triggered when the player enters this area." +
                              "If left empty, this area will be treated as a trigger for silence")]
    private MusicClass musicToTrigger;
    
    [SerializeField]
    [Tooltip("If true, any active DynamicAudioAreas will be faded out when this one is entered. " +
               "If false, only areas with the same dynamicAudioLayer will be faded out")]
    private bool fadeOutOtherLayers = true;


    [SerializeField]
    [Tooltip("If true, the DynamicAudioArea that was active before entering this one will be re-enabled " +
             "once the trigger area is exited (or if no area was previously active, audio will fade to " +
             "silence). If false, exiting the area will have no effect.")]
    private bool revertAudioOnExit = false;


    [Space] [Space]
    [Range(0, (MaxDynamicAudioLayers - 1))]
    [SerializeField]
    [Tooltip("The dynamic audio layer this area belongs to. DynamicAudioAreas on the same layer will " +
         "always be forced to stay in sync, so all areas with music of the same length/tempo " +
         "should be put on the same dynamicAudioLayer")]
    private int dynamicAudioLayer;

    #endregion

    #region Properties

    public AudioSource  MusicSource         { get { return musicSource; } }
    public int          DynamicAudioLayer   { get { return dynamicAudioLayer; } }
    public bool         Active              { get { return active; } }
    public bool         WaitingForRevert    { get { return waitingForRevert; } set { waitingForRevert = value; } }

    #endregion

    private float           baseVolume;     // Volume of the music before fading is applied
    private bool            fadingIn;       // Whether the audio source should be fading in
    private bool            fadingOut;      // Whether the audio source should be fading out
    private bool            active;
    private bool            waitingForRevert;

    public const int MaxDynamicAudioLayers = 10; // The number of layers that can be chosen from when setting a dynamicAudioLayer for the area

    private void Awake()
    {
        if(musicToTrigger != null)
        {
            // Set defaults
            musicSource.clip = musicToTrigger.AudioClip;

            // Mute the source by default until the player enters it
            musicSource.volume = 0.0f;

            // Allow volume to be adjusted while the game is paused
            musicSource.velocityUpdateMode = AudioVelocityUpdateMode.Dynamic;
        }

        // Areas without music to trigger simply trigger silence, so no audio source properties need to be set
    }

    private void Start()
    {
        // Subscribe to save/load events so the active value can be saved/loaded with the game
        SaveLoadManager.Instance.SubscribeSceneSaveLoadEvents(OnSceneSave, OnSceneLoadSetup, OnSceneLoadConfigure);
    }

    private void OnDestroy()
    {
        // Unsubscribe from save/load events if the area GameObject is destroyed to prevent null ref. errors
        SaveLoadManager.Instance.UnsubscribeSceneSaveLoadEvents(OnSceneSave, OnSceneLoadSetup, OnSceneLoadConfigure);
    }

    public void OnSceneSave(SaveData saveData)
    {
        // Get a unique id for this audio area using its location
        string locationId = GetLocationId();

        Debug.Log("Saving data for DynamicAudioArea with location id: " + locationId);

        // Save whether the area is active (currently playing music)
        saveData.AddData("audioAreaActive_"  + locationId, active);
        saveData.AddData("audioAreaWaiting_" + locationId, waitingForRevert);
    }

    public void OnSceneLoadSetup(SaveData saveData)
    {
        // Load whether this audio area was active or waiting when the player last saved

        waitingForRevert  = saveData.GetData<bool>("audioAreaWaiting_" + GetLocationId());

        bool loadedActive = saveData.GetData<bool>("audioAreaActive_"  + GetLocationId());

        if(loadedActive)
        {
            // The area should be active, activate it
            ActivateAudioArea(false);
        }
    }

    public void OnSceneLoadConfigure(SaveData saveData) { } // Nothing to configure

    public string GetMusicToTriggerName()
    {
        if(musicToTrigger != null)
        {
            return musicToTrigger.name;
        }

        return "[Silence]";
    }

    public float GetMusicToTriggerLength()
    {
        if(musicToTrigger != null)
        {
            return musicToTrigger.AudioClip.length;
        }

        return 0;
    }

    private void Update()
    {
        if(musicToTrigger != null)
        {
            if (musicSource.timeSamples >= (musicToTrigger.AudioClip.samples - 2048))
            {
                // If the current audio clip will be done playing roughly in the next frame,
                //   restart all dynamic audio sources on the same dynamicAudioLayer so their music loops seamlessly
                AudioManager.Instance.PlayAllDynamicSourcesOnLayer(dynamicAudioLayer);
            }
        }

        if(fadingIn)
        {
            // Slowly increase the audio source volume each frame to fade music in
            musicSource.volume += Time.unscaledDeltaTime * 0.5f * baseVolume;

            // Once music is fully faded in, stop fading
            if(musicSource.volume >= baseVolume)
            {
                fadingIn = false;
                musicSource.volume = baseVolume;
            }
        }
        else if(fadingOut)
        {
            // Slowly decrease the audio source volume each frame to fade music out
            musicSource.volume -= Time.unscaledDeltaTime * 0.5f * baseVolume;

            // Once music is fully faded out, stop fading
            if (musicSource.volume <= 0.0f)
            {
                fadingOut = false;
                musicSource.volume = 0.0f;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only accept trigger events once loading is done and the player has been moved to the correct location

        if (!SaveLoadManager.Instance.LoadingSceneData)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                // The player entered the trigger area, activate this dynamic audio
                ActivateAudioArea();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Only accept trigger events once loading is done and the player has been moved to the correct location

        if (!SaveLoadManager.Instance.LoadingSceneData)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                if (revertAudioOnExit)
                {
                    AudioManager.Instance.RevertActiveDynamicAudioAreas();

                    DeactivateAudioArea();
                }
            }
        }
    }

    public void ActivateAudioArea(bool affectOtherAreas = true)
    {
        AudioManager audioManager = AudioManager.Instance;

        // Player has entered the audio area trigger

        if (audioManager.CurrentSceneMusic.PlayMode == MusicPlayMode.Dynamic)
        {
            // The loaded scene is using dynamic audio

            List<DynamicAudioArea> currentActiveAreas = audioManager.ActiveDynamicAudioAreas;

            if(revertAudioOnExit)
            {
                foreach (DynamicAudioArea activeArea in currentActiveAreas)
                {
                    activeArea.waitingForRevert = true;
                }
            }

            if (affectOtherAreas)
            {
                for (int i = 0; i < currentActiveAreas.Count; i++)
                {
                    DynamicAudioArea area = currentActiveAreas[i];

                    if (area != null)
                    {
                        if (fadeOutOtherLayers || area.dynamicAudioLayer == dynamicAudioLayer)
                        {
                            // If the player previously entered another area, fade its music out for a cross-fade effect
                            area.DeactivateAudioArea();

                            // Remove the area from the ActiveDynamicAudioAreas list as it is no longer active
                            currentActiveAreas.Remove(area);

                            i--;
                        }
                    }
                }
            }

            // This is now the current/most recent dynamic audio area
            currentActiveAreas.Add(this);

            // Fade music for this are in
            FadeIn();

            active = true;
        }
    }

    public void DeactivateAudioArea()
    {
        // Deactivate this area and fade its music out

        FadeOut();

        active = false;
    }

    public void UpdateSourceVolume(float volume)
    {
        if(musicToTrigger != null)
        {
            // Update the base volume, and instantly set the music source to have the
            //   new volume if it's already active/playing

            // Multiplying from the set MusicClass volume so the saved volume and default
            //  volume level are both used to determine the overall volume
            baseVolume = musicToTrigger.Volume * volume;

            if (active)
            {
                musicSource.volume = baseVolume;
            }
        }
    }

    private void FadeIn()
    {
        // Start fading in, set fading out to false in case the source is in the process of fading out
        fadingOut = false;
        fadingIn = true;
    }

    private void FadeOut()
    {
        // Start fading out, set fading in to false in case the source is in the process of fading in
        fadingOut = true;
        fadingIn = false;
    }

    private string GetLocationId()
    {
        // Returns a unique string id based on object location in the world
        return (int)transform.position.x + "_" + (int)transform.position.y + "_" + (int)transform.position.z;
    }

    #if UNITY_EDITOR

    // Debug visualisation, draws a wire cube to represent the collider areaa

    private BoxCollider boxCollider;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        if(boxCollider == null)
        {
            boxCollider = GetComponent<BoxCollider>();
        }
        Gizmos.DrawWireCube(transform.position + boxCollider.center, boxCollider.size);
    }

    #endif
}