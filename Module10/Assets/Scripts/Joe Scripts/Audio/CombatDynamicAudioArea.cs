using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatDynamicAudioArea : DynamicAudioArea
{
    private Collider    triggerCollider;
    private bool        areaEnabled;

    protected override void Start()
    {
        base.Start();

        triggerCollider = GetComponent<Collider>();

        SetAreaEnabled(false);
    }

    public override void OnSceneSave(SaveData saveData)
    {
        base.OnSceneSave(saveData);

        saveData.AddData("combatAudioEnabled_" + GetUniquePositionId(), areaEnabled);
    }

    public override void OnSceneLoadSetup(SaveData saveData)
    {
        base.OnSceneLoadSetup(saveData);

        SetAreaEnabled(saveData.GetData<bool>("combatAudioEnabled_" + GetUniquePositionId()));
    }

    public void SetAreaEnabled(bool enabled)
    {
        areaEnabled = enabled;

        triggerCollider.enabled = enabled;

        if(!enabled)
        {
            TriggerExitEvents();
        }
    }
}