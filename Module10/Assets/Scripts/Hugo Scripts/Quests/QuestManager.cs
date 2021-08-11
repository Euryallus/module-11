using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Used to process player interactions with quest givers
// Development window:  Prototype phase & production phase
// Inherits from:       MonoBehaviour

public class QuestManager : MonoBehaviour, IPersistentGlobalObject
{
    [SerializeField]    private PlayerQuestBacklog playerQuestData; // Ref. to player's quest data, stored as ScriptableObject

                        private QuestUI UI;                         // Ref. to QuestUI, controls UI relating to player quests

                        private PlayerMovement playerMove;          // Ref. to PlayerMovement controller
                        private NPCManager npcManager;              // Ref to NPCManager in scene

                        private bool runupdate = true;              // Flags when checking quests completion isnt needed (e.g. when handing in a different quest)

    [SerializeField] private QuestMenuUI questMenuUI;

    private void Awake()
    {
        // Added by Joe: Load progress to restore default quest completion values if there is no save file
        playerQuestData.LoadProgress();
    }

    private void Start()
    {
        SaveLoadManager.Instance.SubscribeGlobalSaveLoadEvents(OnGlobalSave, OnGlobalLoadSetup, OnGlobalLoadConfigure);

        // Assigns refs. to UI, player movement and npc manager
        UI = gameObject.GetComponent<QuestUI>();
        playerMove = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        npcManager = gameObject.GetComponent<NPCManager>();
    }

    private void OnDestroy()
    {
        SaveLoadManager.Instance.UnsubscribeGlobalSaveLoadEvents(OnGlobalSave, OnGlobalLoadSetup, OnGlobalLoadConfigure);
    }

    // Called when an NPC is flagged as a quest giver & doesn't have any more dialogue to say
    public void OfferQuest(QuestData questToOffer, QuestGiverData offerer)
    {
        // Stops player from moving, sets ref to pending quest in playerQuestData to quest offered
        playerMove.StopMoving();
        playerQuestData.pendingQuest = questToOffer;
        // Displays quest UI (allows player to see details & accept or decline)
        UI.DisplayQuestAccept(playerQuestData.pendingQuest, questToOffer.forceAccept);

        // Sets ref. to NPC that's giving out the quest
        playerQuestData.offer = offerer;
    }

    // Called when "accept" UI button is pressed from quest UI
    public void AcceptQuest()
    {
        // Allows player to move again
        playerMove.StartMoving();

        // If nothing's gone wrong & player is being offered an actual quest, add to backlog
        if(playerQuestData.pendingQuest != null)
        {
            playerQuestData.questBacklog.Add(playerQuestData.pendingQuest);
            
            // Removes quest from questGiver's list of quests to give out
            playerQuestData.offer.questsToGive.RemoveAt(0);
            // Add quest data to HUD
            UI.AddHUDQuestName(playerQuestData.pendingQuest);
            questMenuUI.AddQuestButton(playerQuestData.pendingQuest);

            // Resets refs to quest giver & quest being offered (as quest has been accepted)
            playerQuestData.offer = null;
            playerQuestData.pendingQuest = null;
        }

        if (npcManager.interactingWith.gameObject.GetComponent<WalkingNonInteractable>() != null)
        {
            npcManager.interactingWith.gameObject.GetComponent<WalkingNonInteractable>().StartMovingAgain();
        }

        // Hides quest UI
        UI.HideQuestAccept();
        // Stops camera from focusing on NPC
        npcManager.StopFocusCamera();
    }

    // Called when "decline" UI button is pressed from quest UI
    public void DeclineQuest()
    {
        // Allows player to move again & hides quest UI
        playerMove.StartMoving();
        UI.HideQuestAccept();
        // Removes refs to quest giver & quest being offered (as quest was declined)
        playerQuestData.pendingQuest = null;
        playerQuestData.offer = null;

        if (npcManager.interactingWith.gameObject.GetComponent<WalkingNonInteractable>() != null)
        {
            npcManager.interactingWith.gameObject.GetComponent<WalkingNonInteractable>().StartMovingAgain();
        }

        // Stops cam. from focusing on NPC
        npcManager.StopFocusCamera();
    }

    // Used to check the completion of quests in player's backlog
    private void Update()
    {
        // So long as there are quests in the backlog & has been flagged that quests should be checked, see if any have been completed
        if(playerQuestData.questBacklog.Count > 0 && runupdate)
        {
            for(int i = 0; i < playerQuestData.questBacklog.Count; i++)
            {
                // Cycles each quest in the backlog and assins a temp ref. to it
                QuestData quest = playerQuestData.questBacklog[i];
                if(!(quest.questCompleted))
                {
                    // Checks if quest is already completed, & if not checks if it's just been completed
                    quest.CheckCompleted();
                }
            }
            // Updates HUD quest list to reflect completion
            UI.SetHUDQuestNameCompleted(playerQuestData.questBacklog[0]);
        }
    }

    // Called when quest is handed in to quest giver
    public void CompleteQuest(QuestData quest)
    {
        // Removes quest from UI and flags quest as having been handed in

        if(playerQuestData.questBacklog[0] == quest)
        {
            UI.RemoveHUDQuestName();
        }

        questMenuUI.RemoveButton(quest);

        Debug.Log("REMOVED " + quest.questName);

        quest.questHandedIn = true;

        // Removes quest from backlog & puts in completed list
        playerQuestData.questBacklog.Remove(quest);
        playerQuestData.completedQuests.Add(quest);

        if(playerQuestData.questBacklog.Count > 0)
        {
            UI.AddHUDQuestName(playerQuestData.questBacklog[0]);
        }

        //Stops player from moving while talking to NPC
        playerMove.StopMoving();
        runupdate = false;

        // Displays quest complete UI (rewards & handInDialogue)
        UI.DisplayQuestComplete(quest);

        InventoryPanel inventory = GameSceneUI.Instance.PlayerInventory;

        //Cycles each objective in quest, if any "collect" quests exist make a note of what needs removing from player inventory
        foreach (QuestObjective objective in quest.objectives)
        {
            if(objective.objectiveType == QuestObjective.Type.Collect)
            {
                GatherItemsQuestObjective ob = (GatherItemsQuestObjective)objective;

                // Attempts to remove items from inventory
                for (int i = 0; i < ob.toCollect.Quantity; i++)
                {
                    if (!inventory.TryRemoveItem(ob.toCollect.Item))
                    {
                        // Warns dev if item isnt in inventory or hotbar
                        Debug.LogWarning("none in inventory or hotbar");
                    }
                }
            }
        }

        // If quest has rewards, cycle each item in rewards & add to inventory
        if (quest.rewards.Count != 0)
        {
            // Added by Joe: Keeps track of any item groups that cannot be added to the player's inventory and should instead be dropped
            List<ItemGroup> dropItemGroups = new List<ItemGroup>();

            foreach (ItemGroup stack in quest.rewards)
            {
                ItemGroup dropItemGroup = new ItemGroup(stack.Item, 0);
                int inventoryItemsAddedCount = 0;

                for (int i = 0; i < stack.Quantity; i++)
                {
                    if(inventory.TryAddItem(stack.Item))
                    {
                        inventoryItemsAddedCount++;
                    }
                    else
                    {
                        dropItemGroup.Quantity++;
                    }
                }

                dropItemGroups.Add(dropItemGroup);

                // Show a notification telling the player what items they gained in the current group
                NotificationManager.Instance.AddNotificationToQueue(NotificationMessageType.GivenItem, new string[] { stack.Quantity.ToString(), stack.Item.UIName });
            }

            if (dropItemGroups.Count > 0)
            {
                // Drop any item groups that couldn't be added to the player's inventory
                inventory.DropItemGroups(dropItemGroups, true, true);
            }
        }
    }

    // Returns true if player interacts with quest giver & they have quests to give / recieve
    public bool InteractWith(string questGiverName)
    {
        // Cycles each quest giver in list saved in PlayerQuestData
        foreach(QuestGiverData giver in playerQuestData.questGivers)
        {
            // Compares name of quest giver with NPC name currently interacting with
            if (giver.QuestGiverName == questGiverName)
            {
                // Cycles each quest in the player's backlog
                for (int i = 0; i < playerQuestData.questBacklog.Count; i++)
                {
                    QuestData quest = playerQuestData.questBacklog[i];

                    // If the quest hasn't been handed in but is completed 
                    if (quest.questCompleted && !quest.questHandedIn && quest.handInToGiver)
                    {
                        // And if the quest giver name matches the hand-in point on the quest
                        if (giver.QuestGiverName == quest.handInNPCName)
                        {
                            // Complete the quest
                            CompleteQuest(quest);

                            // Checks if quest just handed in had any leading on from it (quest line)
                            if (quest.nextQuests.Count != 0)
                            {
                                // Cycles each quest that comes after that which was just handed in
                                foreach (QuestData nextquest in quest.nextQuests)
                                {
                                    // Finds which quest givers are meant to give out quest
                                    foreach (QuestGiverData q in playerQuestData.questGivers)
                                    {
                                        // If quest giver name fits "hand out NPC name" of new quest in questline, add to the list of quests they give out
                                        if (q.QuestGiverName == nextquest.handOutNPCName)
                                        {
                                            q.questsToGive.Add(nextquest);
                                        }
                                    }
                                }
                            }

                            //quest has been processed & completed
                            return true;

                        }
                    }
                }

                // If no quest was handed in, instead check if NPC has quest to give out
                if(giver.questsToGive.Count != 0)
                {
                    OfferQuest(giver.questsToGive[0], giver);
                    return true;
                }

            }
        }

        // No quests were available from this NPC so return false
        return false;
    }

    // Called from the "close" button in quest hand-in UI
    public void CloseQuestHandIn()
    {
        // Hides quest hand-in UI
        UI.HideQuestComplete();

        // Flags update as available, returns camera to normal & allows player to move
        runupdate = true;
        npcManager.StopFocusCamera();
        playerMove.StartMoving();
    }

    public void OnGlobalLoadSetup(SaveData saveData)
    {
        //playerQuestData.LoadProgress();
        //
        //Debug.Log("LOADED SAVES");
    }

    public void OnGlobalLoadConfigure(SaveData saveData)
    {
        playerQuestData.LoadProgress();

        // Cycles each quest in player's backlog adds to the HUD UI and marks as completed if quest is flagged as completed

        if (playerQuestData.questBacklog.Count != 0)
        {
            foreach (QuestData quest in playerQuestData.questBacklog)
            {
                // Adds to the HUD UI and marks as completed if quest is flagged as completed
                questMenuUI.AddQuestButton(quest);
            }
            
            UI.AddHUDQuestName(playerQuestData.questBacklog[0]);
            UI.SetHUDQuestNameCompleted(playerQuestData.questBacklog[0]);
            
        }
    }

    public void OnGlobalSave(SaveData saveData)
    {
        Debug.Log("SAVED SAVES");
        playerQuestData.SaveProgress();
    }
}
