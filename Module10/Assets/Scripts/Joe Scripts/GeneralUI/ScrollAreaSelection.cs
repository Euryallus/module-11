using UnityEngine;
using UnityEngine.EventSystems;

// New for mod11: Detects when the player's mouse pointer is within a scroll area to other scroll-based input can be disabled

public class ScrollAreaSelection : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static bool AnyScrollAreaSelected { get; private set; }    // Whether the pointer is currently over any scroll area

    public void OnPointerEnter(PointerEventData eventData)
    {
        AnyScrollAreaSelected = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        AnyScrollAreaSelected = false;
    }
}