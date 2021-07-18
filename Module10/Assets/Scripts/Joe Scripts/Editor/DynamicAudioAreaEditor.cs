using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DynamicAudioArea))]
public class DynamicAudioAreaEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Update the object being edited
        serializedObject.Update();

        DynamicAudioArea targetArea = (DynamicAudioArea)target;

        // Draw the default editor GUI first
        base.OnInspectorGUI();

        GUILayout.Space(5.0f);

        EditorGUILayout.LabelField("Other areas on layer " + targetArea.DynamicAudioLayer + ":", EditorStyles.boldLabel);
        GameObject[] dynamicAudioAreas = GameObject.FindGameObjectsWithTag("DynamicAudioArea");

        int otherAreaCount = 0;

        for (int i = 0; i < dynamicAudioAreas.Length; i++)
        {
            DynamicAudioArea area = dynamicAudioAreas[i].GetComponent<DynamicAudioArea>();
            if (area != null && area != targetArea && area.DynamicAudioLayer == targetArea.DynamicAudioLayer)
            {
                if(area.GetMusicToTriggerLength() == targetArea.GetMusicToTriggerLength())
                {
                    EditorGUILayout.LabelField(area.gameObject.name + ": " + area.GetMusicToTriggerName(), EditorStyles.label);
                }
                else
                {
                    // Label for warning text
                    GUIStyle warningLabelStyle = new GUIStyle(EditorStyles.label);
                    warningLabelStyle.normal.textColor = Color.red;

                    EditorGUILayout.LabelField(area.gameObject.name + ": " + area.GetMusicToTriggerName() + " (Different length)", warningLabelStyle);
                }
                otherAreaCount++;
            }
        }

        if(otherAreaCount == 0)
        {
            EditorGUILayout.LabelField("(None)", EditorStyles.label);
        }
    }
}
