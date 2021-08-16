using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Handles NPC dialogue UI
// Development window:  Prototype phase & production phase
// Inherits from:       MonoBehaviour

public class DialogueUI : UIPanel
{
    [SerializeField]    private CanvasGroup cg;         // Ref. to NPC Dialogue UI canvas group
    [SerializeField]    private TMP_Text dialogueText;  // Ref. to dialogue text box
    [SerializeField]    private TMP_Text nameText;      // Ref. to NPC name in dialogue

    [SerializeField] private float timeBetweenLetterPrint = 0.005f;
    private WaitForSecondsRealtime wait;

    private string displayedDialogue;                   // Ref. to dialogue being displayed
    private string playerName;                          // Ref. to player's name entered from main menu


    protected override void Start()
    {
        base.Start();

        // Creates new wait (used when printing letters)
        wait = new WaitForSecondsRealtime(timeBetweenLetterPrint);
        // Saves player's name from player stats
        playerName = PlayerStats.PlayerName;
    }

    public void ShowDialogue(string dialogue, string NPCName)
    {
        StopAllCoroutines();
        // Sets canvas group alpha to 1 and changes dialogue text component to display string passed
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Cycles dialogue, if "&n" is found replace with player's name
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
        nameText.text = NPCName;

        // Prints dialogue like typewriter (one letter at a time)
        StartCoroutine(PrintDialogue(dialogue));

        // Allows player to see & interact w/ dialogue box
        cg.alpha = 1;
        cg.blocksRaycasts = true;
        cg.interactable = true;

        showing = true;
    }

    private void Update()
    {
        // Checks to see if dialogue printing is skipped using Space
        if(Input.GetKeyDown(KeyCode.Space) && showing == true)
        {
            StopAllCoroutines();
            dialogueText.text = displayedDialogue;
        }
    }

    //Hides dialogue
    public void HideDialogue()
    {
        // Added by Joe: Plays a click sound if not loading and the dialogue is not already hidden
        if(cg.alpha > 0.0f && !SaveLoadManager.Instance.LoadingSceneData)
        {
            AudioManager.Instance.PlaySoundEffect2D("buttonClickSmall", true);
        }

        // Stops player from interacting w/ dialogue box & sets cursor back to "locked" state
        Cursor.lockState = CursorLockMode.Locked;
        cg.alpha = 0;
        cg.blocksRaycasts = false;
        cg.interactable = false;

        // Resets dialogue in text box
        dialogueText.text = "";

        showing = false;
    }

    // Prints dialogue one letter at a time
    private IEnumerator PrintDialogue(string dialogue)
    {
        
        string display = "";

        string closingTag = "";

        bool isSyntax = false;

        // Cycles each character in the dialogue passed
        for(int i = 0; i < dialogue.Length; i++)
        {
            // Checks if character is part of syntax tags
            if(dialogue[i] == '<' && dialogue[i + 2] == '>')
            {
                // Adds the opening tag to the dialogue to print, saves closing tag
                closingTag = "</" + dialogue[i+1] + ">";

                // Sets display to include opening & closing tags w/ next character in between
                display = display + dialogue.Substring(i, 3) + closingTag;

                // Displays string on screen
                dialogueText.text = display;

                // skips i to the next letter in the dialogue (ignores </>)
                i += 2;

                
                yield return wait;

                isSyntax = true;
            }
            else if(dialogue[i] == '<' && dialogue[i + 1] == '/')
            {
                // If dialogue includes </> the closing tag is found
                display = display + closingTag;

                // Increase i to the next letter in dialogue
                i += 3;
                isSyntax = false;

                // Resets closing tag
                closingTag = "";

                // Displays dialogue on screen
                dialogueText.text = display;


                yield return wait;
            }
            else
            {
                // If syntax tags are still being applied, split string to remove the closing tag from the end, add the next character, then re-add closing tags
                if(isSyntax)
                {
                    display = display.Substring(0, display.Length - 4); 
                }


                // Display dialogue on screen
                display = display + dialogue[i] + closingTag;
                dialogueText.text = display;

                yield return wait;
            }
        }   
    }
}
