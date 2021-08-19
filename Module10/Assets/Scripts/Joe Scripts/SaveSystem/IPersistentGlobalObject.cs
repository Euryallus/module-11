// ||=======================================================================||
// || IPersistentGlobalObject: An interface that gives objects the ability  ||
// ||   to be saved/loaded globally with the game.                          ||
// ||   (Also see: SaveSystemExample.cs, IPersistentSceneObject.cs)         ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the production phase (Module 11).                                 ||
// ||=======================================================================||

public interface IPersistentGlobalObject
{

    //OnGlobalSave is called on this object when any scene is saved
    public abstract void OnGlobalSave(SaveData saveData);


    // OnGlobalLoadSetup is the first function called on this object when any scene is loaded,
    //   it should be used for loading initial data that other objects depend on
    public abstract void OnGlobalLoadSetup(SaveData saveData);

    // OnGlobalLoadConfigure is the second function called on this object when any scene is loaded,
    //   it should be used for configuring objects/data that are dependent on data loaded in the setup stage
    public abstract void OnGlobalLoadConfigure(SaveData saveData);

}