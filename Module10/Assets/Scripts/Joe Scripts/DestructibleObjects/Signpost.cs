using UnityEngine;
using TMPro;

// ||=======================================================================||
// || Signpost: When attached to a signpost prefab, allows the object to be ||
// ||   saved/loaded/destroyed and keeps track of the item that was used    ||
// ||   to place the sign, including its specific text customisation.       ||
// ||=======================================================================||
// || Used on prefab: Joe/Environment/Construction/Signpost                 ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public class Signpost : PlaceableDestructible
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private TextMeshPro text;

    #endregion

    #region Properties

    public string RelatedItemId { get { return relatedItemId; } set { relatedItemId = value; } }

    #endregion

    private string relatedItemId = "signpost";  // Id of the item used when placing this sign/to be dropped when destroying it

    public void SetRelatedItem(string itemId)
    {
        relatedItemId = itemId;

        Item item = ItemManager.Instance.GetItemWithId(itemId);

        // Set sign text based on the related item's player-set properties
        SetSignText(    item.GetCustomStringPropertyWithName("line1").Value,
                        item.GetCustomStringPropertyWithName("line2").Value,
                        item.GetCustomStringPropertyWithName("line3").Value,
                        item.GetCustomStringPropertyWithName("line4").Value);
    }

    public void SetSignText(string line1, string line2, string line3, string line4)
    {
        // Sets multiple lines of text on the sign
        text.text = line1 + "\n" + line2 + "\n" + line3 + "\n" + line4;
    }

    protected override void DestroyedByPlayer()
    {
        base.DestroyedByPlayer();

        Destroy(gameObject);

        // Get the player's inventory panel
        InventoryPanel inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<InventoryPanel>();

        // Give the player back the item used to placed the sign, which includes custom text properties
        inventory.AddItemToInventory(relatedItemId);
    }

    public override void AddDataToWorldSave(SaveData saveData)
    {
        base.AddDataToWorldSave(saveData);

        // Save the position and rotation of the sign in the world, as well as the id of the item used to place the sign
        saveData.AddData("sign*",   new SignpostSaveData()
                                    {
                                        Position = new float[3] { transform.position.x, transform.position.y, transform.position.z },
                                        Rotation = new float[3] { transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z },
                                        RelatedItemId = RelatedItemId
                                    });
    }
}

// SignpostSaveData contains data used for saving/loading signposts

[System.Serializable]
public class SignpostSaveData : TransformSaveData
{
    public string   RelatedItemId;
}