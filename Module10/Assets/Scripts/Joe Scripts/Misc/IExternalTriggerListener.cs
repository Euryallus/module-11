using UnityEngine;

// ||=======================================================================||
// || IExternalTriggerListener: An interface that allows a class to recieve ||
// ||   function calls from trigger events on an ExternalTrigger            ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public interface IExternalTriggerListener
{
    // Note: AddListener must be called on an ExternalTrigger to assign a class as a listener

    // When AddListener has been called on an ExternalTrigger, the following functions
    //  will be called on trigger enter/stay/exit events (see ExternalTrigger.cs)

    public void OnExternalTriggerEnter(string triggerId, Collider other);
    public void OnExternalTriggerStay(string triggerId, Collider other);
    public void OnExternalTriggerExit(string triggerId, Collider other);
}