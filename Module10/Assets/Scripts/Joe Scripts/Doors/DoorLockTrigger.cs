using UnityEngine;

// ||=======================================================================||
// || DoorLockTrigger: A trigger that disables a door (prevents it from     ||
// ||    being opened) when entered.                                        ||
// ||=======================================================================||
// || Used on prefab: Joe/UI/AbilityUnlock/AbilityUnlockPanel               ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the production phase (Module 11).                                 ||
// ||=======================================================================||

public class DoorLockTrigger : MonoBehaviour
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private DoorMain doorToLock;

    #endregion

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            // Disable the door when the player enters the trigger area
            doorToLock.SetDoorOpenRestriction(DoorMain.DoorOpenRestriction.Disabled);
        }
    }
}