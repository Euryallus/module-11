using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         A base class for quest objectives
// Development window:  Prototype phase & production phase
// Inherits from:       QuestObjective

[System.Serializable]
public class QuestObjective : ScriptableObject
{    
    public enum Type    // Possible "types" objective could be (easier reference when using lists of different objectives)
    {
        GoTo,
        Collect
    }

    [HideInInspector]       public Type objectiveType;  // Used to save objective type (defined in each subclass)
                            public string taskName;     // Name of task (displayed on quest when first issued)
                            public bool taskComplete = false;   // Flags if player has completed the objective

    [SerializeField] bool saveTaskComplete = false;

    public void SaveProgress()
    {
        saveTaskComplete = taskComplete;
    }

    public void LoadProgress()
    {
        taskComplete = saveTaskComplete;
    }

    // Virtual func. that returns if player has completed the objective
    public virtual bool CheckCcompleted()
    {
        return false;
    }
}
