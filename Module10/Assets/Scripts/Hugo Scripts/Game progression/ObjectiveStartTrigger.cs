using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ObjectiveStartTrigger : InteractableWithOutline, IPersistentSceneObject
{
    public AreaObjectiveManager areaObjectiveManager;

    [SerializeField] private SoundClass pickUpNoise;
    [SerializeField] private UnityEvent onStartEvents = new UnityEvent();

    private bool triggerEnabled = true;

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

    public override void Interact()
    {
        base.Interact();

        areaObjectiveManager.StartEnounter();
        AudioManager.Instance.PlaySoundEffect2D(pickUpNoise);

        GameObject.FindGameObjectWithTag("Player").GetComponent<CameraShake>().ShakeCameraForTime(1.5f, CameraShakeType.ReduceOverTime, 0.12f);

        onStartEvents.Invoke();

        SetTriggerEnabled(false);
    }

    private void SetTriggerEnabled(bool enabled)
    {
        triggerEnabled = enabled;

        gameObject.GetComponent<ObjectiveStartTrigger>().enabled = enabled;
    }

    public void OnSceneSave(SaveData saveData)
    {
        saveData.AddData(GetUniquePositionId() + "_triggerEnabled", triggerEnabled);
    }

    public void OnSceneLoadSetup(SaveData saveData)
    {
        SetTriggerEnabled(saveData.GetData<bool>(GetUniquePositionId() + "_triggerEnabled"));
    }

    public void OnSceneLoadConfigure(SaveData saveData) { }

    private string GetUniquePositionId()
    {
        return transform.position.x + "_" + transform.position.y + "_" + transform.position.z;
    }
}
