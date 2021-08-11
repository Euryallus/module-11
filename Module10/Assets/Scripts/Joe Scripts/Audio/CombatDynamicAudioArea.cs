using UnityEngine;

// ||=======================================================================||
// || CombatDynamicAudioArea: A DynamicAudioArea specifically suited to     ||
// ||    work with enemy combat arenas. Unlike standard DynamicAudioAreas,  ||
// ||    this can be dynamically enabled/disabled when combat starts/stops. ||
// ||=======================================================================||
// || Used on prefab: Joe/Audio/CombatDynamicAudioArea                      ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the production phase (Module 11).                                 ||
// ||=======================================================================||

public class CombatDynamicAudioArea : DynamicAudioArea
{
    private Collider    triggerCollider;    // The collider that enables/disables this area when entered/exited
    private bool        areaEnabled;        // Whether this area is currently enabled

    protected override void Start()
    {
        base.Start();

        triggerCollider = GetComponent<Collider>();

        // Disable the area by default
        SetAreaEnabled(false);
    }

    public override void OnSceneSave(SaveData saveData)
    {
        base.OnSceneSave(saveData);

        // Save whether the area is enabled
        saveData.AddData("combatAudioEnabled_" + GetUniquePositionId(), areaEnabled);
    }

    public override void OnSceneLoadSetup(SaveData saveData)
    {
        base.OnSceneLoadSetup(saveData);

        // Load whether the area should be enabled
        SetAreaEnabled(saveData.GetData<bool>("combatAudioEnabled_" + GetUniquePositionId()));
    }

    public void SetAreaEnabled(bool enabled)
    {
        areaEnabled = enabled;

        // When the area is disabled, the trigger collider will be turned off
        //   so the dynamic audio cannot be turned on by the player
        triggerCollider.enabled = enabled;

        if(!enabled)
        {
            // If the area was previously enabled, trigger the exit events that would usually be
            //   triggered when the player exits triggerCollider on a standard DynamicAudioArea
            TriggerExitEvents();
        }
    }
}