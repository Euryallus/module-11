// ||=======================================================================||
// || IPersistentSceneObject: An interface that gives objects the ability   ||
// ||   to be saved/loaded with a specific scene.                           ||
// ||   (Also see: SaveSystemExample.cs, IPersistentGlobalObject.cs)        ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || originally for the prototype phase.                                   ||
// ||                                                                       ||
// || Changes made during the production phase (Module 11):                 ||
// ||                                                                       ||
// || - Renamed from IPersistentObject to IPersistentSceneObject - now used ||
// ||    specifically for objects that exist in (and should save/load data  ||
// ||    in) only one scene. e.g. the contents of chests                    ||
// ||=======================================================================||

public interface IPersistentSceneObject
{

    // OnSceneSave is called on this object when a certain scene is saved
    public abstract void OnSceneSave(SaveData saveData);


    // OnSceneLoadSetup is the first function called on this object when a certain scene is loaded,
    //   it should be used for loading initial data that other objects depend on
    public abstract void OnSceneLoadSetup(SaveData saveData);


    // OnSceneLoadConfigure is the second function called on this object when a certain scene is loaded,
    //   it should be used for configuring objects/data that are dependent on data loaded in the setup stage
    public abstract void OnSceneLoadConfigure(SaveData saveData);

}