// ||=======================================================================||
// || IPersistentPlacedObject: An interface used objects placed in the      ||
// ||   by the player so they can be saved/loaded with the world.           ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public interface IPersistentPlacedObject
{
    public abstract void AddDataToWorldSave(SaveData saveData); // Used to add the placed object's SaveData to the world save data
}
