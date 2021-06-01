using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Stores data retaining to the NPC component is attached to. Manages dialogue & flags if NPC is also a quest giver
// Development window:  Prototype phase
// Inherits from:       MonoBehaviour

public class NPC : MonoBehaviour
{
    [Header("NPC data")]

                        public string npcName;                      // Stores Name of NPC
                        public bool isQuestGiver = false;           // Flags if NPC has quests associated withh them

    [Header("Dialogue data")]

    [TextArea(1, 5)]
    [SerializeField]    private string[] dialogueLines;             // List of dialogue lines NPC will say if interacted with
    [HideInInspector]   public bool haveTalkedToBefore = false;     // Flags if player has talked to the NPC before 
                        private int dialogueProgression = 0;        // Stores player progression through dialogue lines

    [Header("Focus camera point")]

                        public Transform cameraFocusPoint;          // Stores transform point associated with NPC (point camera focuses on while talking)

    // Output next line of dialogue NPC has
    public string ReturnDialoguePoint()
    {
        // Checks if NPC has more to say using dialogueProgression pointer
        if (dialogueProgression < dialogueLines.Length)
        {
            // Returns next line from list & incriments pointer
            string returnString = dialogueLines[dialogueProgression];
            ++dialogueProgression;
            return returnString;
        }
        else
        {
            // If end of dialogue is met and NPC is a quest giver, dont repeat the lines
            if(isQuestGiver)
            {
                haveTalkedToBefore = true;
            }
        }

        // If no dialogue is found, return NULL
        return null;
    }

    //Resets dialogue after conversation if NPC is not a quest giver
    public void ResetDialogue()
    {
        // Called when player exits conversation, checks if "point in conversation" needs returning to 0
        if(!haveTalkedToBefore)
        {
            dialogueProgression = 0;
        }
    }

}
