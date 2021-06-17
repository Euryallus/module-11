using UnityEngine;
using UnityEngine.EventSystems;

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