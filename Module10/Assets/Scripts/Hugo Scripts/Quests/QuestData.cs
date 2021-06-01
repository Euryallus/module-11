using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Used to store data about a quest
// Development window:  Prototype phase
// Inherits from:       ScriptableObject

[CreateAssetMenu(fileName = "Quest data", menuName = "Quests/Quest data/New single quest", order = 1)]
[System.Serializable]
public class QuestData : ScriptableObject
{
    [Header("Quest attributes (Shown to players)")]

    [TextArea(1, 4)]     public string questName;                                               // Stores "name" of quest 
    [TextArea(5, 10)]    public string questDescription;                                        // Stores description of quest (shown to player when offered quest)
    [TextArea(5, 10)]    public string questCompleteDialogue;                                   // Dialogue that's spoken when quest is completed

    [Header("Quest objectives, rewards, quest line progression")]

    [SerializeField]    public List<QuestData> nextQuests = new List<QuestData>();              // List of quests that lead on from this one (quest line functionality)
                        public List<ItemGroup> rewards = new List<ItemGroup>();                 // Rewards given to the player for completing this quest
                        public List<QuestObjective> objectives = new List<QuestObjective>();    // Objectives needed to complete this quest

    [Header("Hand-in data")]

                        public string handInNPCName;                                            // ID of NPC that completes this quest
                        public string handOutNPCName;                                           // ID of NPC that hands out this quest
                        public string questLineName = "";                                       // Name of the "quest line" quest is a part of

                        public bool questCompleted = false;                                     // Flags if all quest objectives have been completed
                        public bool questHandedIn = false;                                      // Flags if quest has been handed in yet
                        public bool handInToGiver = true;                                       // Used to determine if quest needs handing in to NPC or is completed as soon as all objectives are met


    // Returns whether all quest objectives have been completed
    public bool CheckCompleted()
    {
        int objectiveCount = 0;
        // Cycles each objective associated with quest - if it's completed, incriment objectiveCount
        foreach(QuestObjective task in objectives)
        {
            if(task.taskComplete)
            {
                ++objectiveCount;
            }
            else
            {
                // If quest is not already flagged as complete, check if it's just been completed
                if (task.CheckCcompleted())
                {
                    task.taskComplete = true;
                    ++objectiveCount;
                }
            }
        }

        // If all quests have been completed, flag questCompleted as true
        questCompleted = (objectiveCount == objectives.Count);
        return questCompleted;

        //if(objectiveCount == objectives.Count)
        //{
        //    questCompleted = true;
        //    return true;
        //}
        //
        //return false;
        
    }
}
