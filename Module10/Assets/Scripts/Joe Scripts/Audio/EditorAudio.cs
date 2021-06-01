using UnityEngine;

// ||=======================================================================||
// || EditorAudio: Handles previewing sounds in the edior                   ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

[ExecuteInEditMode]
public class EditorAudio : MonoBehaviour
{
    [SerializeField] private AudioSource editorSource;  // The audio source used for playing editor sounds

#if (UNITY_EDITOR)

    public void PlaySound(AudioClip clip, float minVolume, float maxVolume, float minPitch, float maxPitch)
    {
        // Play the audio clip with a random volume/pitch in the given ranges

        editorSource.clip = clip;

        editorSource.volume = Random.Range(minVolume, maxVolume);

        editorSource.pitch = Random.Range(minPitch, maxPitch);

        editorSource.Play();
    }

#endif

}
