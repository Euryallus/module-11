using System.Collections.Generic;
using UnityEngine;

// ||=======================================================================||
// || This is an example of how the save system can be used. For actual     ||
// ||   save system code, see SaveLoadManager and related classes.          ||
// ||=======================================================================||
// || Written by Joe for Module 10                                          ||
// ||=======================================================================||

public class SaveSystemExample : MonoBehaviour, IPersistentObject
{
    // Example variables that could be saved
    private int             exampleInt;
    private List<string>    exampleList  = new List<string>()   { "thing1", "thing2", "thing3" };
    private float[]         exampleArray = new float[]          { 0.1f, 7.8f, 69.0f };

    private void Start()
    {
        // Subscribe to save and load events on start
        SaveLoadManager.Instance.SubscribeSaveLoadEvents(OnSave, OnLoadSetup, OnLoadConfigure);
    }

    private void OnDestroy()
    {
        // Unsubscribe from save and load events if the object is destroyed
        SaveLoadManager.Instance.UnsubscribeSaveLoadEvents(OnSave, OnLoadSetup, OnLoadConfigure);
    }

    public void OnSave(SaveData saveData)
    {
        // OnSave is called each time game data is saved to a file
           
        // This is where you add any values that you want to save to the save data
           
        // You can save any serializable data type. See this for a full list: https://docs.microsoft.com/en-us/dotnet/standard/serialization/binary-serialization
           
        // Example:

        saveData.AddData("intToSave", exampleInt);

        saveData.AddData("listToSave", exampleList);

        saveData.AddData("arrayToSave", exampleArray);
    }

    public void OnLoadSetup(SaveData saveData)
    {
        // OnLoadSetup is called each time game data is loaded from a file
           
        // This is where you initialise variables with the loaded values
           
        // Example:

        exampleInt = saveData.GetData<int>("intToSave");

        exampleList = saveData.GetData<List<string>>("listToSave");

        exampleArray = saveData.GetData<float[]>("arrayToSave");
    }

    public void OnLoadConfigure(SaveData saveData)
    {
        // OnLoadConfigure is called each time game data is loaded from a file AFTER OnLoadSetup has been called on all persistent objects
           
        // This is where you call any functions/setup any data that is dependent on other data that was initialised in OnLoadSetup on this object or another object
    }
}