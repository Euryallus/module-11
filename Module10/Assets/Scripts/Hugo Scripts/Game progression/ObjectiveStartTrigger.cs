using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Main author(s):      Hugo Bailey & Joe Allen
// Description:         Controls combat encounters (specifically saving & initial trigger)
// Development window:  Production phase
// Inherits from:       InteractableWithOutline & IPersistentSceneObject

public class ObjectiveStartTrigger : InteractableWithOutline, IPersistentSceneObject
{

    [SerializeField]    private SoundClass pickUpNoise;                         // Sound class of audio to play when encounter is started
    [SerializeField]    private UnityEvent onStartEvents = new UnityEvent();    // Functions run when encounter first starts (e.g. close doors) 
                        public AreaObjectiveManager areaObjectiveManager;       // Ref. to objectiveManager instance
                        private bool triggerEnabled = true;                     // Flags if trigger has been entered at least once

    public override void Interact()
    {
        base.Interact();

        // Starts combat encounter via objectiveManager & plays sound effect
        areaObjectiveManager.StartEnounter();
        AudioManager.Instance.PlaySoundEffect2D(pickUpNoise);

        // Initialises screen shake
        GameObject.FindGameObjectWithTag("Player").GetComponent<CameraShake>().ShakeCameraForTime(1.5f, CameraShakeType.ReduceOverTime, 0.12f);
        
        // Runs start functions & disables "start trigger" so player can't do objective more than once
        onStartEvents.Invoke();
        SetTriggerEnabled(false);
    }

    // Enables or disables ability to start encounter
    private void SetTriggerEnabled(bool enabled)
    {
        triggerEnabled = enabled;
        gameObject.GetComponent<ObjectiveStartTrigger>().enabled = enabled;
    }

    // Added & adapted by Joe
    protected override void Start()
    {
        base.Start();
        SaveLoadManager.Instance.SubscribeSceneSaveLoadEvents(OnSceneSave, OnSceneLoadSetup, OnSceneLoadConfigure);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        SaveLoadManager.Instance.UnsubscribeSceneSaveLoadEvents(OnSceneSave, OnSceneLoadSetup, OnSceneLoadConfigure);
    }

    public void OnSceneSave(SaveData saveData)
    {
        saveData.AddData(GetUniquePositionId() + "_triggerEnabled", triggerEnabled);
    }

    public void OnSceneLoadSetup(SaveData saveData)
    {
        bool triggerEnabled = saveData.GetData<bool>(GetUniquePositionId() + "_triggerEnabled");

        SetTriggerEnabled(triggerEnabled);
    }

    public void OnSceneLoadConfigure(SaveData saveData) { }

    private string GetUniquePositionId()
    {
        return transform.position.x + "_" + transform.position.y + "_" + transform.position.z;
    }
}
