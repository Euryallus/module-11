// ||=======================================================================||
// || IPersistentObject: An interface that gives objects the ability to be  ||
// ||   saved and loaded with the game. (See SaveSystemExample.cs)          ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public interface IPersistentObject
{
    //OnLoadSetup is the first function called on this object when the game is loaded,
    //  it should be used for loading initial data that other objects depend on
    public abstract void OnLoadSetup(SaveData saveData);

    //OnLoadConfigure is the second function called on this object when the game is loaded,
    //  it should be used for configuring objects/data that are dependent on data loaded in the setup stage
    public abstract void OnLoadConfigure(SaveData saveData);

    //OnSave is called on this object when the game is saved
    public abstract void OnSave(SaveData saveData);

}