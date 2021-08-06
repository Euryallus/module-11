using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonEvents : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public event Action PointerClickEvent;
    public event Action PointerEnterEvent;
    public event Action PointerExitEvent;

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
