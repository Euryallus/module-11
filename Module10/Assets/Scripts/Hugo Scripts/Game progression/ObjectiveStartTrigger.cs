using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ObjectiveStartTrigger : InteractableWithOutline
{
    public AreaObjectiveManager areaObjectiveManager;

    [SerializeField] private SoundClass pickUpNoise;
    [SerializeField] private UnityEvent onStartEvents = new UnityEvent();
    public override void Interact()
    {
        base.Interact();

        areaObjectiveManager.StartEnounter();
        AudioManager.Instance.PlaySoundEffect2D(pickUpNoise);

        GameObject.FindGameObjectWithTag("Player").GetComponent<CameraShake>().ShakeCameraForTime(0.5f, CameraShakeType.StopAfterTime, 0.1f);

        onStartEvents.Invoke();
        gameObject.GetComponent<ObjectiveStartTrigger>().enabled = false;
    }
}
