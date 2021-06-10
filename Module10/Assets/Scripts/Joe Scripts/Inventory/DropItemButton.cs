using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// Module 11

public class DropItemButton : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    public ItemContainer ParentContainer { get { return parentContainer; } }

    [SerializeField] private InventoryPanel inventoryPanel;
    [SerializeField] private ItemContainer  parentContainer;

    public void OnPointerDown(PointerEventData eventData)
    {
        // Called when the button is clicked

        inventoryPanel.DropItemsInHand(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        parentContainer.ItemInfoPopup.ShowPopupWithText("Drop On Ground", "Click while holding item(s)");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        parentContainer.ItemInfoPopup.HidePopup();
    }
}
