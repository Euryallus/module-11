// ||=======================================================================||
// || CustomisingTable: When attached to a customising table prefab, allows ||
// ||   it to be potentially destroyed by the player and saved/loaded       ||
// ||=======================================================================||
// || Used on prefab: Joe/Environment/Crafting & Chests/Customising Table   ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public class CustomisingTable : PlaceableDestructible
{
    public override void AddDataToWorldSave(SaveData saveData)
    {
        base.AddDataToWorldSave(saveData);

        // Save the position, rotation and type of this object in the world
        saveData.AddData("customisingTable*",   new TransformSaveData()
                                                {
                                                    Position = new float[3] { transform.position.x, transform.position.y, transform.position.z },
                                                    Rotation = new float[3] { transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z },
                                                });
    }

    protected override void DestroyedByPlayer()
    {
        base.DestroyedByPlayer();

        // Destroy the customising table GameObject when it's broken by the player
        Destroy(gameObject);
    }
}