using UnityEngine;

// ||=======================================================================||
// || HandSlotUI: ContainerSlotUI specifically for the slot used to hold/   ||
// ||   pick up/move items, auto-links to a slot object that is not a child ||
// ||   of any other UI panel, also disables click behaviour since a hand   ||
// ||   slot cannot be interacted with like a standard slot.                ||
// ||=======================================================================||
// || Used on prefab: Joe/UI/HandSlot                                       ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

[RequireComponent(typeof(Animator))]
public class HandSlotUI : ContainerSlotUI
{
    protected override void Awake()
    {
        base.Awake();

        // Link to a new slot object that is not a child of any other UI panel
        LinkToContainerSlot(new ContainerSlot(0, null));
    }
}