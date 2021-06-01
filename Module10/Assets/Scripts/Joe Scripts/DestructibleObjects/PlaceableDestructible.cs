// ||=======================================================================||
// || PlaceableDestructible: Base class for all objects that can be placed  ||
// ||   in the world by the player (and hence need to be saved/loaded),     ||
// ||   and can also be destroyed.                                          ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public class PlaceableDestructible : DestructableObject, IPersistentPlacedObject
{
    protected bool placedByPlayer;  //  true = object was placed in the world by the player using the construction system and needs to be loaded menually,
                                    //  false = object was pre-placed in the unity editor and loads with the scene by defauly

    protected virtual void OnDestroy()
    {
        if (placedByPlayer)
        {
            // This object no longer exists in the world, remove it from the save list
            WorldSave.Instance.RemovePlacedObjectFromSaveList(this);
        }
    }

    public virtual void SetupAsPlacedObject()
    {
        placedByPlayer = true;

        // Tell the WorldSave that this is a player-placed object that should be saved with the world
        WorldSave.Instance.AddPlacedObjectToSave(this);
    }

    public override void Destroyed()
    {
        // Only allow the object to be destroyed if it was placed by the player
        if(placedByPlayer)
        {
            DestroyedByPlayer();
        }
        else
        {
            NotificationManager.Instance.AddNotificationToQueue(NotificationMessageType.CantDestroyObject);
        }
    }

    protected virtual void DestroyedByPlayer()
    {
        base.Destroyed();
    }

    public virtual void AddDataToWorldSave(SaveData saveData)
    {
        // Nothing to save by default as this is the base class for all placeable destructible objects
    }
}
