using UnityEngine;
using UnityEditor;

// ||=======================================================================||
// || DynamicAudioAreaEditor: Custom editor that displays some info about   ||
// ||   the current DynamicAudioLayer of a DynamicAudioArea.                ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the production phase (Module 11).                                 ||
// ||=======================================================================||

[CustomEditor(typeof(DynamicAudioArea), true)]
public class DynamicAudioAreaEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // DynamicAudioAreas on the same DynamicAudioLayer should contain audio clips that are the same length
        //   to allow all music to loop seamlessley. This editor extension shows all areas on the same layer as
        //   the target, and warns the user if any areas on the layer have audio clips of a different length

        // Update the object being edited
        serializedObject.Update();

        DynamicAudioArea targetArea = (DynamicAudioArea)target;

        // Draw the default editor GUI first
        base.OnInspectorGUI();

        GUILayout.Space(5.0f);

        // Title label
        EditorGUILayout.LabelField("Other areas on layer " + targetArea.DynamicAudioLayer + ":", EditorStyles.boldLabel);

        GameObject[] dynamicAudioAreas = GameObject.FindGameObjectsWithTag("DynamicAudioArea");

        int otherAreaCount = 0; // Keeps track of the number of other areas on the same layer

        for (int i = 0; i < dynamicAudioAreas.Length; i++)
        {
            // Loop through all DynamicAudioAreas that can be found in the scene

            DynamicAudioArea area = dynamicAudioAreas[i].GetComponent<DynamicAudioArea>();

            if (area != null && area != targetArea && area.DynamicAudioLayer == targetArea.DynamicAudioLayer)
            {
                // Found an area on the same layer as the targetArea

                if(area.GetMusicToTriggerLength() == targetArea.GetMusicToTriggerLength())
                {
                    // The areas have audio clips that are the same length - display the area and music names as standard text
                    EditorGUILayout.LabelField(area.gameObject.name + ": " + area.GetMusicToTriggerName(), EditorStyles.label);
                }
                else
                {
                    // Label for warning text
                    GUIStyle warningLabelStyle = new GUIStyle(EditorStyles.label);
                    warningLabelStyle.normal.textColor = Color.red;

                    // The areas have audio clips that are different lengths - display a warning label telling the user this
                    EditorGUILayout.LabelField(area.gameObject.name + ": " + area.GetMusicToTriggerName() + " (Different length)", warningLabelStyle);
                }

                // Increase the counter of other areas
                otherAreaCount++;
            }
        }

        // If there are no other layers on the same area, just show a label saying 'None'
        if(otherAreaCount == 0)
        {
            EditorGUILayout.LabelField("(None)", EditorStyles.label);
        }
    }
}
