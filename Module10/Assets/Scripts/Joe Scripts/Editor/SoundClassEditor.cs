using UnityEditor;
using UnityEngine;

// ||=======================================================================||
// || SoundClassEditor: Custom editor that adds a 'preview sound' button    ||
// ||   to the inspector window for sound classes.                          ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

[CustomEditor(typeof(SoundClass))]
public class SoundClassEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Update the object being edited
        serializedObject.Update();

        SoundClass soundClass = (SoundClass)target;

        // Draw the default editor GUI first
        base.OnInspectorGUI();

        GUILayout.Space(20.0f);

        // Style of preview sound button
        GUIStyle largeButtonStyle = new GUIStyle("largeButton")
        {
            fontSize    = 16,
            fontStyle   = FontStyle.Bold,
            fixedHeight = 45.0f
        };

        if (GUILayout.Button("Preview Sound", largeButtonStyle))
        {
            if (soundClass.AudioClips != null && soundClass.AudioClips.Length > 0)
            {
                // The preview button was pressed and the sound class contains at least 1 audio clip

                // Choose a random clip
                AudioClip chosenClip = soundClass.AudioClips[Random.Range(0, soundClass.AudioClips.Length)];

                if(chosenClip != null)
                {
                    GameObject audioManagerGameObj = GameObject.Find("_AudioManager");

                    if(audioManagerGameObj != null)
                    {
                        // If the clip is not null and an AudioManager can be found in the scene, play the clip using the EditorAudio script

                        audioManagerGameObj.GetComponent<EditorAudio>().PlaySound(chosenClip, soundClass.VolumeRange.Min, soundClass.VolumeRange.Max,
                                                                                        soundClass.PitchRange.Min, soundClass.PitchRange.Max);
                    }
                    else
                    {
                        // No AudioManager in the scene
                        Debug.LogWarning("Could not preview sound - no AudioManager found in the scene!");
                    }
                }
            }
        }

        // Apply any properties that have been changed
        EditorUtility.SetDirty(target);
        serializedObject.ApplyModifiedProperties();
    }

}