using UnityEngine;

// ||=======================================================================||
// || MusicClass: Contains data about an AudioClip being used as music;     ||
// ||   an id to identify it and the base volume to use when playing it.    ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

[CreateAssetMenu(fileName = "Music Class", menuName = "Audio/Music Class")]
public class MusicClass : ScriptableObject
{
    // See tooltips below for info
    #region Properties

    public AudioClip    AudioClip { get { return m_audioClip; } }
    public string       Id { get { return m_id; } }              
    public float        Volume { get { return m_volume; } }

    #endregion

    [SerializeField]
    [Tooltip("The audio clip to be played")]
    private AudioClip m_audioClip;

    [SerializeField]
    [Tooltip("Unique id for this music that will be used to play it in code")]
    private string m_id;

    [SerializeField]
    [Tooltip("The music will be played at this base volume (multiplied by the player-set volume), 1 = default, 0 = silent")]
    private float m_volume = 1.0f;
}