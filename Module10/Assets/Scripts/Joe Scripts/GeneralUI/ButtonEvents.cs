using System;
using UnityEngine;
using UnityEngine.EventSystems;

// ||=======================================================================||
// || ButtonEvents: Contains generic button click/enter/exit events, for    ||
// ||   when button events need to be easily accessed from another script.  ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the production phase (Module 11).                                 ||
// ||=======================================================================||


public class ButtonEvents : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public event Action PointerClickEvent;
    public event Action PointerEnterEvent;
    public event Action PointerExitEvent;

    // Events are invoked when the pointer clicks on or enters/exits the button

    public void OnPointerClick(PointerEventData eventData)
    {
        PointerClickEvent.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PointerEnterEvent.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PointerExitEvent.Invoke();
    }
}
