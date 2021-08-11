using UnityEngine;
using UnityEngine.EventSystems;

// ||=======================================================================||
// || ScrollAreaSelection: Detects when the player's mouse pointer is       ||
// ||   within a scroll area so other scroll-based input can be disabled.   ||
// ||=======================================================================||
// || Used on various prefabs.                                              ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the production phase (Module 11).                                 ||
// ||=======================================================================||

public class ScrollAreaSelection : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static bool AnyScrollAreaSelected { get; private set; } // Whether the pointer is currently over any scroll area

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Pointer is within a scroll area
        AnyScrollAreaSelected = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Pointer has left a scroll area
        AnyScrollAreaSelected = false;
    }
}