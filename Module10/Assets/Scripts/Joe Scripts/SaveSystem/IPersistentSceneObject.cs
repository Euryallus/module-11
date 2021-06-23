// ||=======================================================================||
// || IPersistentObject: An interface that gives objects the ability to be  ||
// ||   saved and loaded with the game. (See SaveSystemExample.cs)          ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

// Mod11: renamed from IPersistentObject to IPersistentSceneObject, now specifically used for objects that
// exist in only one scene (e.g. chest) as opposed to objects that are shared in all scenes (e.g. player inventory)

public interface IPersistentSceneObject
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