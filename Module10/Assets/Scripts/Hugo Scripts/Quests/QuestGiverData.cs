using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Stores simple data about each quest giver NPC
// Development window:  Prototype phase
// Inherits from:       ScriptableObject

[CreateAssetMenu(fileName = "Quest data", menuName = "NPCs/NPC quest data", order = 1)]
public class QuestGiverData : ScriptableObject
{    
    public string QuestGiverName;                                   // Unique name of the quest giver
    public List<QuestData> questsToGive = new List<QuestData>();    // List of quests NPC has to offer at any given point

}
