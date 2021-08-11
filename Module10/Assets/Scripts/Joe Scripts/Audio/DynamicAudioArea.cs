using System.Collections.Generic;
using UnityEngine;

// ||=======================================================================||
// || DynamicAudioArea: Triggers a dynamic audio track when the player      ||
// ||   enters its area if the active scene is using MusicPlayMode.Dynamic  || 
// ||=======================================================================||
// || Used on prefab: Joe/Audio/DynamicAudioArea                            ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || originally for the prototype phase.                                   ||
// ||                                                                       ||
// || Changes made during the production phase (Module 11):                 ||
// ||                                                                       ||
// || - Added dynamicAudioLayer functionality: DynamicAudioAreas can now    ||
// ||    exist on separate layers, meaning dynamic audio can overlap and    ||
// ||    be triggered in more complex ways.                                 ||
// ||=======================================================================||

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

    private float           baseVolume;         // Volume of the music before fading is applied
    private bool            fadingIn;           // Whether the audio source should be fading in
    private bool            fadingOut;          // Whether the audio source should be fading out
    private bool            active;             // Whether the area should be playing music (unless waitingForRevert)
    private bool            waitingForRevert;   // If true, music is temporarily disabled and will be re-enabled when RevertActiveDynamicAudioAreas is called on AudioManager

    public const int MaxDynamicAudioLayers = 10; // The number of layers that can be chosen from when setting a dynamicAudioLayer in the inspector

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

    protected virtual void Start()
    {
        // Subscribe to save/load events so the active value can be saved/loaded with the game
        SaveLoadManager.Instance.SubscribeSceneSaveLoadEvents(OnSceneSave, OnSceneLoadSetup, OnSceneLoadConfigure);
    }

    protected virtual void OnDestroy()
    {
        // Unsubscribe from save/load events if the area GameObject is destroyed to prevent null ref. errors
        SaveLoadManager.Instance.UnsubscribeSceneSaveLoadEvents(OnSceneSave, OnSceneLoadSetup, OnSceneLoadConfigure);
    }

    public virtual void OnSceneSave(SaveData saveData)
    {
        // Get a unique id for this audio area using its position in the world
        string locationId = GetUniquePositionId();

        Debug.Log("Saving data for DynamicAudioArea with location id: " + locationId);

        // Save whether the area is active (currently playing music)
        saveData.AddData("audioAreaActive_"  + locationId, active);
        saveData.AddData("audioAreaWaiting_" + locationId, waitingForRevert);
    }

    public virtual void OnSceneLoadSetup(SaveData saveData)
    {
        // Load whether this audio area was active or waiting when the player last saved

        waitingForRevert  = saveData.GetData<bool>("audioAreaWaiting_" + GetUniquePositionId());

        bool loadedActive = saveData.GetData<bool>("audioAreaActive_"  + GetUniquePositionId());

        if(loadedActive)
        {
            // The area should be active, activate it
            ActivateAudioArea(false);
        }
    }

    public virtual void OnSceneLoadConfigure(SaveData saveData) { } // Nothing to configure

    public string GetMusicToTriggerName()
    {
        // Returns the name of the music this area will trigger, or '[Silence]' if no music will be triggered

        if(musicToTrigger != null)
        {
            return musicToTrigger.name;
        }

        return "[Silence]";
    }

    public float GetMusicToTriggerLength()
    {
        // Returns the length of the audio clip that this area plays

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
                TriggerExitEvents();
            }
        }
    }

    protected void TriggerExitEvents()
    {
        // The player has left the trigger area

        if (revertAudioOnExit)
        {
            // Stop playing music and revert any areas that are waiting if revertAudioOnExit = true

            AudioManager.Instance.RevertActiveDynamicAudioAreas();

            DeactivateAudioArea();
        }
    }

    public void ActivateAudioArea(bool affectOtherAreas = true)
    {
        AudioManager audioManager = AudioManager.Instance;

        if (audioManager.CurrentSceneMusic.PlayMode == MusicPlayMode.Dynamic)
        {
            // The loaded scene is using dynamic audio

            // Get a list of all DynamicAudioAreas that are currently active
            List<DynamicAudioArea> currentActiveAreas = audioManager.ActiveDynamicAudioAreas;

            if(revertAudioOnExit)
            {
                // Tell all active areas to wait for a revert (essentially wait until the player leaves this area)
                //   before they can continue to play music
                foreach (DynamicAudioArea activeArea in currentActiveAreas)
                {
                    activeArea.waitingForRevert = true;
                }
            }

            if (affectOtherAreas)
            {
                // affectOtherAreas = true, meaning other areas can be forced to fade out when this one is entered

                for (int i = 0; i < currentActiveAreas.Count; i++)
                {
                    DynamicAudioArea area = currentActiveAreas[i];

                    if (area != null && area != this)
                    {
                        if (fadeOutOtherLayers || area.dynamicAudioLayer == dynamicAudioLayer)
                        {
                            // Found an area on the same layer (OR any area if fadeOutOtherLayers is allowed)

                            // Deactivate the area to fade out its music
                            area.DeactivateAudioArea();

                            // Remove the area from the ActiveDynamicAudioAreas list since it is no longer active
                            currentActiveAreas.Remove(area);

                            // Decrement the iterator since an area was removed from the list
                            i--;
                        }
                    }
                }
            }

            // Fade this area's music in
            FadeIn();

            // This is now an active audio area

            if (!currentActiveAreas.Contains(this))
            {
                currentActiveAreas.Add(this);
            }

            active = true;
        }
    }

    public void DeactivateAudioArea()
    {
        // Deactivate this area and fade its music out

        if(AudioManager.Instance.ActiveDynamicAudioAreas.Contains(this))
        {
            AudioManager.Instance.ActiveDynamicAudioAreas.Remove(this);
        }

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

    protected string GetUniquePositionId()
    {
        return transform.position.x + "_" + transform.position.y + "_" + transform.position.z;
    }

#if UNITY_EDITOR

    // Debug visualisation, draws a wire cube to represent the collider areaa

    private Collider areaCollider;

    private void OnDrawGizmos()
    {
        // Matrix code taken from https://forum.unity.com/threads/gizmo-rotation.4817/

        Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        Gizmos.matrix = rotationMatrix;

        if (areaCollider == null)
        {
            areaCollider = GetComponent<Collider>();
        }

        if (areaCollider.enabled)
        {
            Gizmos.color = Color.red;
        }
        else
        {
            Gizmos.color = Color.blue;
        }

        if (areaCollider is SphereCollider sphereCollider)
        {
            Gizmos.DrawWireSphere(sphereCollider.center, sphereCollider.radius);
        }
        else if (areaCollider is BoxCollider boxCollider)
        {
            Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
        }
    }

    #endif
}