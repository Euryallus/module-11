using UnityEngine;

// ||=======================================================================||
// || ISavePoint: An interface used for save points that allows WorldSave   ||
// ||   to find them based on their id and get the position to respawn      ||
// ||   the player at.                                                      ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || originally for the prototype phase.                                   ||
// ||                                                                       ||
// || Changes made during the production phase (Module 11):                 ||
// ||                                                                       ||
// || - Added SetAsUsed and SetAsUnused, used to keep track of the save     ||
// ||    point that was most recently used                                  ||
// ||=======================================================================||

public interface ISavePoint
{
    public abstract string  GetSavePointId();       // Used to return the unique id of the save point

    public abstract Vector3 GetRespawnPosition();   // Used to return the position to respawn the player at on load if this save point was used

    public abstract void    SetAsUsed();            // Called when this point is used to set it as the most recently used save point
                            
    public abstract void    SetAsUnused();          // Called when this point is no longer the most recently used save point
}