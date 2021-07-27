using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Handles NPC dialogue UI
// Development window:  Prototype phase
// Inherits from:       MonoBehaviour

public class DialogueUI : UIPanel
{
    [SerializeField]    private CanvasGroup cg;         // Ref. to NPC Dialogue UI canvas group
    [SerializeField]    private TMP_Text dialogueText;  // Ref to dialogue text box

    [SerializeField] private float timeBetweenLetterPrint = 0.005f;
    private WaitForSeconds wait;

    private string displayedDialogue;
    private string playerName;


    protected override void Start()
    {
        base.Start();

        wait = new WaitForSeconds(timeBetweenLetterPrint);
        playerName = PlayerStats.PlayerName;
    }

    public void ShowDialogue(string dialogue)
    {
        StopAllCoroutines();
        // Sets canvas group alpha to 1 and changes dialogue text component to display string passed
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;


        for(int i = 0; i < dialogue.Length; i++)
        {
            if(i < dialogue.Length - 2)
            {
                if(dialogue[i] == '&' && dialogue[i+1] == 'n')
                {
                    // player name insert

                    dialogue = dialogue.Substring(0, i) + playerName + dialogue.Substring(i + 2, dialogue.Length - 1 - (i + 2));
                }
            }

        }
        
        displayedDialogue = dialogue;

        StartCoroutine(PrintDialogue(dialogue));
        //dialogueText.text = dialogue;

        // Allows player to see & interact w/ dialogue box
        cg.alpha = 1;
        cg.blocksRaycasts = true;
        cg.interactable = true;

        showing = true;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space) && showing == true)
        {
            StopAllCoroutines();
            dialogueText.text = displayedDialogue;
        }
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

        showing = false;
    }

    private IEnumerator PrintDialogue(string dialogue)
    {
        string display = "";

        string closingTag = "";

        bool isSyntax = false;

        for(int i = 0; i < dialogue.Length; i++)
        {
            if(dialogue[i] == '<' && dialogue[i + 2] == '>')
            {
                // got the opening tag
                closingTag = "</" + dialogue[i+1] + ">";

                display = display + dialogue.Substring(i, 3) + closingTag;
                dialogueText.text = display;

                i += 2;

                yield return wait;

                isSyntax = true;
            }
            else if(dialogue[i] == '<' && dialogue[i + 1] == '/')
            {
                // got closing tag
                display = display + closingTag;

                i += 3;
                isSyntax = false;

                closingTag = "";

                dialogueText.text = display;


                yield return wait;
            }
            else
            {
                if(isSyntax)
                {
                    display = display.Substring(0, display.Length - 4); 
                }

                display = display + dialogue[i] + closingTag;
                dialogueText.text = display;

                yield return wait;
            }
        }   
    }
}
