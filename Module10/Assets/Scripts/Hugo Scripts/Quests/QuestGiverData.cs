using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Stores simple data about each quest giver NPC
// Development window:  Prototype phase & production phase
// Inherits from:       ScriptableObject

[CreateAssetMenu(fileName = "Quest data", menuName = "NPCs/NPC quest data", order = 1)]
public class QuestGiverData : ScriptableObject
{    
    public string QuestGiverName;                                   // Unique name of the quest giver
    public List<QuestData> questsToGive = new List<QuestData>();    // List of quests NPC has to offer at any given point

    [SerializeField] private List<QuestData> saveQuestsToGive = new List<QuestData>();

    public void SaveProgress()
    {
        saveQuestsToGive = new List<QuestData>();

        foreach (QuestData quest in questsToGive)
        {
            saveQuestsToGive.Add(quest);
        }
    }

    public void LoadProgress()
    {
        questsToGive = new List<QuestData>();

        foreach (QuestData quest in saveQuestsToGive)
        {
            questsToGive.Add(quest);
        }
    }
}
