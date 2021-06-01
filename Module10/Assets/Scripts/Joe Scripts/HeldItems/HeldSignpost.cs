using UnityEngine;

// ||=======================================================================||
// || HeldSignpost: A visual indicator for the placement of signposts       ||
// ||   when the player is holding one as an item.                          ||
// ||=======================================================================||
// || Used on prefab: HeldItems/Placeables/BuildPreview_Signpost            ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public class HeldSignpost : HeldPlaceableItem
{
    protected override GameObject PlaceItem()
    {
        // Place the signpost GameObject as standard
        GameObject signGameObj = base.PlaceItem();

        // Add the item used for placement to the signpost script so it can be used for
        //   customisation and given back to the player if the sign is destroyed
        Signpost signpostScript = signGameObj.GetComponent<Signpost>();
        signpostScript.SetRelatedItem(item.Id);

        return signGameObj;
    }
}