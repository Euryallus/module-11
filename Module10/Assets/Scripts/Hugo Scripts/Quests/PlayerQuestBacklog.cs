using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         ScriptableObject used as a tracker for player's completed quests & quest givers
// Development window:  Prototype phase & production phase
// Inherits from:       QuestObjective

[CreateAssetMenu(fileName = "Player data", menuName = "Player/Player quest data", order = 1)]
public class PlayerQuestBacklog : ScriptableObject
{
    [Header("Quest lists (completed and in-progress)")]

    [HideInInspector]   public QuestData pendingQuest = null;                           // Ref. to quest being offered to the player (default is NULL as no quest is offered yet)
    [HideInInspector]   public QuestGiverData offer = null;                             // Ref. to the quest giver offering the quest to the player (default is NULL)
                        public List<QuestData> questBacklog = new List<QuestData>();    // List of quests player has accepted but not completed / handed in
                        public List<QuestData> completedQuests = new List<QuestData>(); // List of quests player has completed and handed in

    [Header("NPCData obj.s must be added here!")]
    public List<QuestGiverData> questGivers;                                            // List of quest givers

    [Header("Array of all quests")]
    public List<QuestData> quests;                                                      // List of all quests (allows to be reset & loaded)

    [Header("SAVE DATA")]
    // Due to Joe's save system not liking my quest structs, all quest data is stored within the scriptableObjects
    [SerializeField] private List<QuestData> savedQuestBacklog = new List<QuestData>();     // Stores quest backlog at last save
    [SerializeField] private List<QuestData> savedCompletedQuests = new List<QuestData>();  // Stores completed quest list at last save

    public void SaveProgress()
    {
        savedQuestBacklog = new List<QuestData>(questBacklog);          // Copies questBacklog to savedQuestBacklog
        savedCompletedQuests = new List<QuestData>(completedQuests);    // Copies completedQuests to savedCompletedQuests

        foreach(QuestData quest in quests)
        {
            // Cycles each quest & prompts save
            quest.SaveProgress();
        }

        foreach (QuestGiverData questGiver in questGivers)
        {
            // Cycles each quest giver & prompts save
            questGiver.SaveProgress();
        }
    }

    public void LoadProgress()
    {
        questBacklog = new List<QuestData>(savedQuestBacklog);          // Copies savedQuestBacklog to questBacklog
        completedQuests = new List<QuestData>(savedCompletedQuests);    // Copies savedCompletedQuests to completedQuests


        foreach (QuestData quest in quests)
        {
            // Cycles each quest & prompts load
            quest.LoadProgress();
        }

        foreach (QuestGiverData questGiver in questGivers)
        {
            // Cycles each quest giver & prompts load
            questGiver.LoadProgress();
        }
    }

    // Resets all data to default
    public void ResetProgress()
    {
        // Clears all saved quest data
        completedQuests = new List<QuestData>();
        questBacklog = new List<QuestData>();

        savedCompletedQuests = new List<QuestData>();
        savedQuestBacklog = new List<QuestData>();

        // Cycles each list & prompts reset from all quests & quest givers
        foreach (QuestData quest in quests)
        {
            quest.ResetProgress();
        }

        foreach (QuestGiverData questGiver in questGivers)
        {
            questGiver.ResetProgress();
        }
    }
}
