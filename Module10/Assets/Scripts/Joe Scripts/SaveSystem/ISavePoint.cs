using UnityEngine;

// ||=======================================================================||
// || ISavePoint: An interface used for save points that allows WorldSave   ||
// ||   to find them based on their id and get the position to respawn      ||
// ||   the player at.                                                      ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public interface ISavePoint
{
    public abstract string GetSavePointId();        // Used to return the unique id of the save point

    public abstract Vector3 GetRespawnPosition();   // Used to return the position to respawn the player at on load if this save point was used
}
