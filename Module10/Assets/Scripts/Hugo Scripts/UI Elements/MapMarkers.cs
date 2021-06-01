using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Allows player to drag & drop markers on map
// Development window:  Prototype phase
// Inherits from:       MonoBehaviour, IDragHandler, IEndDragHandler 

public class MapMarkers : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public void OnDrag(PointerEventData eventData)
    {
        // Allows player to move pointer to whereever mouse position is
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //Func. is needed to operate but does not have any functionality yet
    }


}
