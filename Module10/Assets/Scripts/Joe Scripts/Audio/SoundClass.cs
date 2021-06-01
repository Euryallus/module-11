using UnityEngine;

// ||=======================================================================||
// || SoundClass: Contains a selection of AudioClips to be grouped together ||
// ||   as a single sound, and defines how to play the sound (volume/pitch) ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

[CreateAssetMenu(fileName = "Sound Class", menuName = "Audio/Sound Class")]
public class SoundClass : ScriptableObject
{
    // See tooltips below for info
    #region Properties

    public AudioClip[]          AudioClips  { get { return m_audioClips; } }
    public string               Id          { get { return m_id; } }
    public ValueRange<float>    VolumeRange { get { return m_volumeRange; } }
    public ValueRange<float>    PitchRange  { get { return m_pitchRange; } }

    #endregion

    [SerializeField] [Tooltip("All possible audio clips that will randomly be chosen from when this sound effect is played")]
    private AudioClip[]         m_audioClips;

    [SerializeField] [Tooltip("Unique id for this sound effect that will be used to play it in code")]
    private string              m_id;

    [SerializeField] [Tooltip("The sound will be played at a random base volume between these two values, 1 = default, 0 = silent")]
    private ValueRange<float>   m_volumeRange = new ValueRange<float>(1.0f, 1.0f);

    [SerializeField] [Tooltip("The sound will be played at a random pitch between these two values, 1 = default pitch")]
    private ValueRange<float>   m_pitchRange = new ValueRange<float>(1.0f, 1.0f);
}