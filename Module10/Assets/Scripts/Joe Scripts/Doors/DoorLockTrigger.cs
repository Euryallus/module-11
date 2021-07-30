using UnityEngine;

public class DoorLockTrigger : MonoBehaviour
{
    [SerializeField] private DoorMain doorToLock;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            doorToLock.SetDoorOpenRestriction(DoorMain.DoorOpenRestriction.Disabled);
        }
    }
}
