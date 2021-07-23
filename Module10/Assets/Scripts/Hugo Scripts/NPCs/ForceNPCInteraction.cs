using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceNPCInteraction : MonoBehaviour, IPersistentSceneObject
{
    [SerializeField]    private NPC npcToTalkTo;
                        private NPCManager manager;

    [SerializeField]    private bool triggerEveryTime = false;
                        private bool hasSpoken = false;

    private void Start()
    {
        manager = GameObject.FindGameObjectWithTag("QuestManager").GetComponent<NPCManager>();

        SaveLoadManager.Instance.SubscribeSceneSaveLoadEvents(OnSceneSave, OnSceneLoadSetup, OnSceneLoadConfigure);
    }

    private void OnDestroy()
    {
        SaveLoadManager.Instance.UnsubscribeSceneSaveLoadEvents(OnSceneSave, OnSceneLoadSetup, OnSceneLoadConfigure);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.transform.CompareTag("Player") && !hasSpoken)
        {
            manager.InteractWithNPC(npcToTalkTo);
            if(!triggerEveryTime)
            {
                hasSpoken = true;
            }
        }
    }

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
