using System.Collections.Generic;
using UnityEngine;

// ||=======================================================================||
// || ExternalTrigger: A trigger that can be listened to by classes that    ||
// ||   implement the IExternalTriggerListener interface, so that collision ||
// ||   events on colliders not attached to the main GameObject can easily  ||
// ||   be registered with another script.                                  ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public class ExternalTrigger : MonoBehaviour
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private string triggerId;   // Id used by IExternalTriggerListeners to identify the trigger event source

    #endregion

    private List<IExternalTriggerListener> listeners = new List<IExternalTriggerListener>(); // List of all IExternalTriggerListeners to notify when a something enters/enters/stays in the trigger

    public void AddListener(IExternalTriggerListener listener)
    {
        // Adds an IExternalTriggerListener to the list of listeners, meaning
        //  OnExternalTriggerEnter, OnExternalTriggerStay and OnExternalTriggerExit will be called on it
        listeners.Add(listener);
    }

    private void OnTriggerEnter(Collider other)
    {
        for (int i = 0; i < listeners.Count; i++)
        {
            // Trigger was entered, call OnExternalTriggerEnter on all listeners
            listeners[i].OnExternalTriggerEnter(triggerId, other);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        for (int i = 0; i < listeners.Count; i++)
        {
            // A collider is in the trigger, call OnExternalTriggerStay on all listeners
            listeners[i].OnExternalTriggerStay(triggerId, other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        for (int i = 0; i < listeners.Count; i++)
        {
            // Trigger was exited, call OnExternalTriggerExit on all listeners
            listeners[i].OnExternalTriggerExit(triggerId, other);
        }
    }
}
