using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Handles NPC dialogue UI
// Development window:  Prototype phase
// Inherits from:       MonoBehaviour

public class DialogueUI : MonoBehaviour
{
    [SerializeField]    private CanvasGroup cg;         // Ref. to NPC Dialogue UI canvas group
    [SerializeField]    private TMP_Text dialogueText;  // Ref to dialogue text box

    public void ShowDialogue(string dialogue)
    {
        // Sets canvas group alpha to 1 and changes dialogue text component to display string passed
        Cursor.lockState = CursorLockMode.None;
        dialogueText.text = dialogue;

        // Allows player to see & interact w/ dialogue box
        cg.alpha = 1;
        cg.blocksRaycasts = true;
        cg.interactable = true;

    }

    //Hides dialogue
    public void HideDialogue()
    {
        // Stops player from interacting w/ dialogue box & sets cursor back to "locked" state
        Cursor.lockState = CursorLockMode.Locked;
        cg.alpha = 0;
        cg.blocksRaycasts = false;
        cg.interactable = false;

        // Resets dialogue in text box
        dialogueText.text = "";
    }
}
