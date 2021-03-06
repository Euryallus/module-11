using System.Collections.Generic;
using UnityEngine;

// ||=======================================================================||
// || ExternalTrigger: A trigger that can be listened to by classes that    ||
// ||   implement the IExternalTriggerListener interface, so that collision ||
// ||   events on colliders not attached to the main GameObject can easily  ||
// ||   be registered with another script.                                  ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || originally for the prototype phase.                                   ||
// ||                                                                       ||
// || Changes made during the production phase (Module 11):                 ||
// ||                                                                       ||
// || - ExternalTriggers can now be enabled/disabled by setting             ||
// ||    TriggerEnabled, and can return their Collider component            ||
// ||=======================================================================||

[RequireComponent(typeof(Collider))]
public class ExternalTrigger : MonoBehaviour
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private string triggerId;   // Id used by IExternalTriggerListeners to identify the trigger event source

    #endregion

    #region Properties

    public bool     TriggerEnabled  { get { return triggerEnabled; } set { triggerEnabled = value; } }
    public Collider TriggerCollider { get { return triggerCollider; } }

    #endregion

    private List<IExternalTriggerListener> listeners = new List<IExternalTriggerListener>(); // List of all IExternalTriggerListeners to notify when a something enters/enters/stays in the trigger

    private bool        triggerEnabled = true;  // Whether the trigger is currently enabled
    private Collider    triggerCollider;        // The collider component that detects trigger events

    private void Awake()
    {
        triggerCollider = GetComponent<Collider>();
    }

    public void AddListener(IExternalTriggerListener listener)
    {
        // Adds an IExternalTriggerListener to the list of listeners, meaning
        //  OnExternalTriggerEnter, OnExternalTriggerStay and OnExternalTriggerExit will be called on it
        listeners.Add(listener);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(triggerEnabled)
        {
            // The trigger is enabled, OnTriggerEnter events

            for (int i = 0; i < listeners.Count; i++)
            {
                // Trigger was entered, call OnExternalTriggerEnter on all listeners
                listeners[i].OnExternalTriggerEnter(triggerId, other);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(triggerEnabled)
        {
            // The trigger is enabled, OnTriggerStay events

            for (int i = 0; i < listeners.Count; i++)
            {
                // A collider is in the trigger, call OnExternalTriggerStay on all listeners
                listeners[i].OnExternalTriggerStay(triggerId, other);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(triggerEnabled)
        {
            // The trigger is enabled, OnTriggerExit events

            for (int i = 0; i < listeners.Count; i++)
            {
                // Trigger was exited, call OnExternalTriggerExit on all listeners
                listeners[i].OnExternalTriggerExit(triggerId, other);
            }
        }
    }
}
