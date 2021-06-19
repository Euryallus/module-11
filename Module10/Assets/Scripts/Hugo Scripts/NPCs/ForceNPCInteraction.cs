using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceNPCInteraction : MonoBehaviour
{
    [SerializeField]    private NPC npcToTalkTo;
                        private NPCManager manager;

    [SerializeField]    private bool triggerEveryTime = false;
                        private bool hasSpoken = false;

    private void Start()
    {
        manager = GameObject.FindGameObjectWithTag("QuestManager").GetComponent<NPCManager>();
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
}
