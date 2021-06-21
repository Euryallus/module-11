using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveStartTrigger : InteractableWithOutline
{
    public AreaObjectiveManager areaObjectiveManager;
    public override void Interact()
    {
        base.Interact();

        areaObjectiveManager.StartEnounter();
        gameObject.GetComponent<ObjectiveStartTrigger>().enabled = false;
    }
}
