using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   Joe Allen (see comments)
// Description:         Used to force player to interact with NPC when they enter a trigger
// Development window:  Production phase
// Inherits from:       MonoBehaviour & IPersistentSceneObject

public class ForceNPCInteraction : MonoBehaviour, IPersistentSceneObject
{
    [SerializeField]    private NPC npcToTalkTo;                    // NPC trigger will force conversation with
    [SerializeField]    private bool triggerEveryTime = false;      // Flags if interaction is forced each time player enters volume
                        private NPCManager manager;                 // Ref. to NPC manager
                        private bool hasSpoken = false;             // Flags if player has already interacted with NPC

    private void Start()
    {
        // Sets ref. to NPC manager
        manager = GameObject.FindGameObjectWithTag("QuestManager").GetComponent<NPCManager>();
        
        // Added by Joe - allows save / load functionality
        SaveLoadManager.Instance.SubscribeSceneSaveLoadEvents(OnSceneSave, OnSceneLoadSetup, OnSceneLoadConfigure);
    }

    private void OnDestroy()
    {
        // Added by Joe - allows save / load functionality
        SaveLoadManager.Instance.UnsubscribeSceneSaveLoadEvents(OnSceneSave, OnSceneLoadSetup, OnSceneLoadConfigure);
    }

    private void OnTriggerEnter(Collider other)
    {
        // If player enters trigger vol & hasnt already interacted with NPC, force NPC interaction & flag as having spoken
        if(other.transform.CompareTag("Player") && !hasSpoken)
        {
            manager.InteractWithNPC(npcToTalkTo);
            if(!triggerEveryTime)
            {
                hasSpoken = true;
            }
        }
    }


    // All following added by Joe

    public void OnSceneSave(SaveData saveData)
    {
        saveData.AddData(GetUniquePositionId() + "_forcedNPCHasSpoken", hasSpoken);
    }

    public void OnSceneLoadSetup(SaveData saveData)
    {
        hasSpoken = saveData.GetData<bool>(GetUniquePositionId() + "_forcedNPCHasSpoken");
    }

    public void OnSceneLoadConfigure(SaveData saveData) { }

    private string GetUniquePositionId()
    {
        return transform.position.x + "_" + transform.position.y + "_" + transform.position.z;
    }
}
