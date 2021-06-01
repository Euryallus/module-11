using System;
using System.Collections.Generic;
using UnityEngine;

// ||=======================================================================||
// || SaveData: Contains data for all saved objects.                        ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

[Serializable]
public class SaveData
{
    // Dictionary containing save data for all objects indexed by a unique id for each one:
    private Dictionary<string, object> saveDataEntries = new Dictionary<string, object>();

    public void AddData<T>(string id, T data)
    {
        // Adds generic save data for an object to the dictionary

        //When multiple items of the same type (and hence same id, for example player-placed items) are saved, the * symbol can be used at the end of the id
        //  to show that duplication is intended and not an error (e.g. saving over the same inventory slot multiple times would be unintended behaviour)

        //Since a unique id is needed for the dictionary entry, an extra '*' is appended to the end of each id to differentiate between them

        if (id[id.Length - 1] == '*')
        {
            while (saveDataEntries.ContainsKey(id))
            {
                id += "*";
            }
        }

        if (!saveDataEntries.ContainsKey(id))
        {
            // Id is unique, add the data
            saveDataEntries.Add(id, data);
        }
        else
        {
            // Data with the given id already exists, this should never happen
            Debug.LogError("Trying to save multiple values with same id: " + id);
        }
    }

    public T GetData<T>(string id, out bool loadSuccess)
    {
        if (saveDataEntries.ContainsKey(id))
        {
            // Return save data with the given id
            loadSuccess = true;
            return (T)saveDataEntries[id];
        }
        else
        {
            // No data found with the given id
            loadSuccess = false;
            Debug.LogError("Trying to load data with invalid id: " + id);
            return default;
        }
    }

    public T GetData<T>(string id)
    {
        // Overload for the above function if the loadSuccess parameter is not required
        return GetData<T>(id, out _);
    }

    public Dictionary<string, object> GetSaveDataEntries()
    {
        // Returns the dictionary containing all save data
        return saveDataEntries;
    }
}