using UnityEngine;

// ||=======================================================================||
// || DoorCollider: Allows the player to open/close a door by interacting   ||
// ||   with the GameObject/collider this script is attached to.            ||
// ||=======================================================================||
// || Used on prefab: Joe/Environment/Doors/Door                            ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public class DoorCollider : InteractableWithOutline
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [Header("Door Collider")]

    [SerializeField] private DoorMain doorMainScript;   // The main script attached to the door than handles opening/closing events

    #endregion

    public override void Interact()
    {
        base.Interact();

        // Tell the door to open/close/notify the player if they cannot open it
        doorMainScript.Interact();
    }
}
