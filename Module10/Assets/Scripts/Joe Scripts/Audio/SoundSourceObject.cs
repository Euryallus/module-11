using UnityEngine;

// ||=======================================================================||
// || SoundSourceObject: When attached to a prefab that is playing a sound  ||
// ||   effect, destroys the GameObject once the sound is done              ||
// ||=======================================================================||
// || Used on prefab: Joe/Audio/SoundSource                                 ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

[RequireComponent(typeof(AudioSource))]
public class SoundSourceObject : MonoBehaviour
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private AudioSource audioSource;

    #endregion

    void Update()
    {
        // Destroy this audio source gameobject once the sound is done playing
        if (!audioSource.isPlaying)
        {
            Destroy(gameObject);
        }
    }
}