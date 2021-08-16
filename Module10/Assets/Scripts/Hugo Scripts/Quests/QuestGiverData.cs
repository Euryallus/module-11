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
    [SerializeField] private List<QuestData> initialQuestList = new List<QuestData>(); // List of initial NPC quests

    public List<QuestData> questsToGive = new List<QuestData>();    // List of quests NPC has to offer at any given point

    [SerializeField] private List<QuestData> saveQuestsToGive = new List<QuestData>();

    // Saves list of quests to distribute at save point
    public void SaveProgress()
    {
        saveQuestsToGive = new List<QuestData>(questsToGive);
    }

    // Loads list of saved quests when prompted
    public void LoadProgress()
    {
        questsToGive = new List<QuestData>(saveQuestsToGive);
    }


    // Resets QuestsToGive to default value @ start of game
    public void ResetProgress()
    {
        saveQuestsToGive = new List<QuestData>(initialQuestList);
        questsToGive = new List<QuestData>(initialQuestList);
    }
}
