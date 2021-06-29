using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles global saving for all portals

public class PortalsSave : MonoBehaviour, IPersistentGlobalObject
{
    public static PortalsSave Instance; // Static instance of the class for simple access

    List<PortalSaveInfo> portalSaveInfo = new List<PortalSaveInfo>();

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
        SaveLoadManager.Instance.SubscribeGlobalSaveLoadEvents(OnGlobalSave, OnGlobalLoadSetup, OnGlobalLoadConfigure);
    }

    private void OnDestroy()
    {
        SaveLoadManager.Instance.UnsubscribeGlobalSaveLoadEvents(OnGlobalSave, OnGlobalLoadSetup, OnGlobalLoadConfigure);
    }

    public void OnGlobalSave(SaveData saveData)
    {
        saveData.AddData("portalSaveCount", portalSaveInfo.Count);

        for (int i = 0; i < portalSaveInfo.Count; i++)
        {
            Debug.Log("Saving portal info for " + portalSaveInfo[i].Id);

            saveData.AddData("portalSave_" + i, portalSaveInfo[i]);
        }
    }

    public void OnGlobalLoadSetup(SaveData saveData)
    {
        portalSaveInfo.Clear();

        int saveCount = saveData.GetData<int>("portalSaveCount");

        for (int i = 0; i < saveCount; i++)
        {
            PortalSaveInfo loadedInfo = saveData.GetData<PortalSaveInfo>("portalSave_" + i);

            Debug.Log("Loading portal info for " + loadedInfo.Id);

            portalSaveInfo.Add(loadedInfo);
        }
    }

    public void OnGlobalLoadConfigure(SaveData saveData) { }

    public bool IsPortalShowing(string portalId)
    {
        foreach (PortalSaveInfo info in portalSaveInfo)
        {
            if(info.Id == portalId)
            {
                Debug.Log("Checking IsPortalShowing for " + portalId + ": " + info.Showing);
                return info.Showing;
            }
        }

        Debug.Log("Checking IsPortalShowing for " + portalId + ": No data (false)");
        return false;
    }

    public void SetPortalShowing(string portalId, bool showing)
    {
        foreach (PortalSaveInfo info in portalSaveInfo)
        {
            if (info.Id == portalId)
            {
                info.Showing = showing;
                return;
            }
        }

        portalSaveInfo.Add(new PortalSaveInfo(portalId, showing));
    }
}

[System.Serializable]
public class PortalSaveInfo
{
    public string Id { get { return m_id; } }
    public bool Showing { get { return m_showing; } set { m_showing = value; } }

    private string  m_id;
    private bool    m_showing;

    public PortalSaveInfo(string id, bool showing)
    {
        m_id = id;
        m_showing = showing;
    }
}

