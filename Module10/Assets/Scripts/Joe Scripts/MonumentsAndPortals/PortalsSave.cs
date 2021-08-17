using System.Collections.Generic;
using UnityEngine;

// ||=======================================================================||
// || PortalsSave: Handles global saving for all portals, to allow the      ||
// ||    state of portals that are not in the active scene to still be      ||
// ||    changed, saved and loaded.                                         ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the production phase (Module 11).                                 ||
// ||=======================================================================||

public class PortalsSave : MonoBehaviour, IPersistentGlobalObject
{
    public static PortalsSave Instance; // Static instance of the class for simple access

    // List containing save data for each portal
    private List<PortalSaveInfo> portalSaveInfo = new List<PortalSaveInfo>();

    private void Awake()
    {
        // Ensure that an instance of the class does not already exist
        if (Instance == null)
        {
            // Set this class as the instance
            Instance = this;
        }
        // If there is an existing instance that is not this, destroy the GameObject this script is connected to
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Subscribe to save/load events so data will be saved/loaded with the game
        SaveLoadManager.Instance.SubscribeGlobalSaveLoadEvents(OnGlobalSave, OnGlobalLoadSetup, OnGlobalLoadConfigure);
    }

    private void OnDestroy()
    {
        // Unsubscribe from save/load events to prevent null ref errors if the script is destroyed
        SaveLoadManager.Instance.UnsubscribeGlobalSaveLoadEvents(OnGlobalSave, OnGlobalLoadSetup, OnGlobalLoadConfigure);
    }

    public void OnGlobalSave(SaveData saveData)
    {
        // Save the number of portals that have stored save info
        saveData.AddData("portalSaveCount", portalSaveInfo.Count);

        for (int i = 0; i < portalSaveInfo.Count; i++)
        {
            // Add the data to be saved for each portal

            Debug.Log("Saving portal info for " + portalSaveInfo[i].Id);

            saveData.AddData("portalSave_" + i, portalSaveInfo[i]);
        }
    }

    public void OnGlobalLoadSetup(SaveData saveData)
    {
        // Clear the list containing save info ready to load new data
        portalSaveInfo.Clear();

        // Get the number of portals to load data for
        int saveCount = saveData.GetData<int>("portalSaveCount");

        for (int i = 0; i < saveCount; i++)
        {
            // Load data for each portal and add it to the list
            PortalSaveInfo loadedInfo = saveData.GetData<PortalSaveInfo>("portalSave_" + i);

            portalSaveInfo.Add(loadedInfo);

            Debug.Log("Loading portal info for " + loadedInfo.Id);
        }
    }

    public void OnGlobalLoadConfigure(SaveData saveData) { }

    public bool IsPortalShowing(string portalId)
    {
        // Checks whether a portal with id: portalId is currently showing

        foreach (PortalSaveInfo info in portalSaveInfo)
        {
            // Loop through save info for each portal and find one with a matching id to the one that was given
            if(info.Id == portalId)
            {
                // Portal with matching id found, return its Showing bool
                Debug.Log("Checking IsPortalShowing for " + portalId + ": " + info.Showing);
                return info.Showing;
            }
        }

        // No portal data found with the given id
        Debug.Log("Checking IsPortalShowing for " + portalId + ": No data (false)");
        return false;
    }

    public void SetPortalShowing(string portalId, bool showing)
    {
        foreach (PortalSaveInfo info in portalSaveInfo)
        {
            // Loop through save info for each portal and find one with a matching id to the one that was given
            if (info.Id == portalId)
            {
                // Portal with matching id found, update its showing bool to the value given
                info.Showing = showing;
                return;
            }
        }

        // No entry in the portalSaveInfo list exists with the given id, add a new one instead 
        portalSaveInfo.Add(new PortalSaveInfo(portalId, showing));
    }
}

// PortalSaveInfo stores data to be saved for each portal: its unique id and whether it is showing/hidden
// ======================================================================================================

[System.Serializable]
public class PortalSaveInfo
{
    public string   Id      { get { return m_id; } }
    public bool     Showing { get { return m_showing; } set { m_showing = value; } }

    // Member variables

    private string  m_id;       // Unique id of the portal
    private bool    m_showing;  // Whether the portal is showing

    // Constructor
    public PortalSaveInfo(string id, bool showing)
    {
        m_id = id;
        m_showing = showing;
    }
}

