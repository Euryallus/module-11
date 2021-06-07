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
    public List<QuestGiverData> questGivers;                                            // List of quest givers in the scene

    [Header("Array of all quests")]
    public List<QuestData> quests;

    [Header("SAVE DATA")]
    [SerializeField] private List<QuestData> savedQuestBacklog = new List<QuestData>();
    [SerializeField] private List<QuestData> savedCompletedQuests = new List<QuestData>();

    public void SaveProgress()
    {
        savedQuestBacklog = new List<QuestData>();
        savedCompletedQuests = new List<QuestData>();

        foreach(QuestData quest in questBacklog)
        {
            savedQuestBacklog.Add(quest);
        }

        foreach(QuestData quest in completedQuests)
        {
            savedCompletedQuests.Add(quest);
        }

        savedQuestBacklog = questBacklog;
        savedCompletedQuests = completedQuests;

        foreach(QuestData quest in quests)
        {
            quest.SaveProgress();
        }

        foreach (QuestGiverData questGiver in questGivers)
        {
            questGiver.SaveProgress();
        }
    }

    public void LoadProgress()
    {
        questBacklog = new List<QuestData>();
        completedQuests = new List<QuestData>();

        foreach (QuestData quest in savedQuestBacklog)
        {
            questBacklog.Add(quest);
        }

        foreach (QuestData quest in savedCompletedQuests)
        {
            completedQuests.Add(quest);
        }

        foreach (QuestData quest in quests)
        {
            quest.LoadProgress();
        }

        foreach (QuestGiverData questGiver in questGivers)
        {
            questGiver.LoadProgress();
        }
    }
}
