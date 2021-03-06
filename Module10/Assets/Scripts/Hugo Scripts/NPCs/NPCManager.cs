using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   Joe Allen (see "Added by Joe" comments)
// Description:         Manages NPC interactions & passes data to QuestManager when NPC is also a quest giver. Communicates with DialogueUI to display dialogue on screen
// Development window:  Prototype phase & production phase
// Inherits from:       MonoBehaviour

public class NPCManager : MonoBehaviour
{ 
    private enum focusCameraState
    { 
        normal,
        moving,
        focused
    }

    [Tooltip("Quest manager ref")]
    [SerializeField]    private QuestManager qmanager;                                                  // Ref to QuestManager
                        private DialogueUI UI;                                                          // Ref to DialogueUI needed to dispay dialogue
                        public NPC interactingWith                         = null;                     // Ref to NPC data currently being used (NPC player is talking to)
                        private PlayerMovement playerMove;                                              // Ref. to player movement script (allows movement to be disabled when in conversation)
                        private focusCameraState focusCameraCurrentState    = focusCameraState.normal;  // Current state of the NPC focus camera
                        private Transform targetCameraTransform;                                        // Target transform for the NPC focus camera
   

    [Header("Camera focus componens")]
    
    [SerializeField]    private float cameraLerpSpeed;      // Speed camera lerps from current pos to focused pos

    private GameObject playerCamera;    // Ref to default player camera
    private GameObject focusCamera;     // Ref to 2nd camera used to focus on NPC

    private bool hasFinishedGame = false;

    private void Start()
    {
        // Assigns references to components needed & hides NPC dialogue UI by default
        UI = gameObject.GetComponent<DialogueUI>();        
        UI.HideDialogue();
        playerMove = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        focusCamera = FindFocusCamera();

        // Deactivates focus camera
        focusCamera.SetActive(false);
    }

    private void Update()
    {
        // Checks if camera should be moving
        if(focusCameraCurrentState == focusCameraState.moving)
        {
            // LERP from current pos to target pos over [1 / cameraLerpSpeed] seconds
            focusCamera.transform.position = Vector3.Lerp(focusCamera.transform.position, targetCameraTransform.position, cameraLerpSpeed * Time.deltaTime);
            focusCamera.transform.rotation = Quaternion.Lerp(focusCamera.transform.rotation, targetCameraTransform.rotation, cameraLerpSpeed * Time.deltaTime);

            // Checks if camera is close enough to target, if so stop moving & snap to target pos
            if(Vector3.Distance( focusCamera.transform.position, targetCameraTransform.position) < 0.01f)
            {
                focusCamera.transform.position = targetCameraTransform.position;
                focusCamera.transform.rotation = targetCameraTransform.rotation;

                focusCameraCurrentState = focusCameraState.focused;
            }
        }
    }

    public void InteractWithNPC(NPC npc)
    {
        interactingWith = npc;
        string dialogueLine;

        // Checks if NPC should switch dialogue to "end game state" dialogue
        if (!hasFinishedGame)
        {
            dialogueLine = npc.ReturnDialoguePoint();
        }
        else
        {
            dialogueLine = npc.ReturnEndDialogue();
        }


        // Checks if NPC has any dialogue to say - if so, focus on target & show dialogue UI
        if(dialogueLine != null)
        {

            StartFocusCameraMove(npc.cameraFocusPoint);

            playerMove.StopMoving();
            UI.ShowDialogue(dialogueLine, npc.displayName);
            
            if(npc.gameObject.GetComponent<WalkingNonInteractable>() != null)
            {

                npc.gameObject.GetComponent<WalkingNonInteractable>().InteruptWait();

            }
        }
        else
        // Checks if NPC has any quests to give instead
        {
            UI.HideDialogue();
            StartFocusCameraMove(npc.cameraFocusPoint);

            // Prevents player from using movement input while talking to someone
            playerMove.StopMoving();

            if (interactingWith.isQuestGiver)
            {
                if (qmanager.InteractWith(interactingWith))
                {
                    // If the NPC is a quest giver and has something to say to the player, do that instead of this

                    if (npc.gameObject.GetComponent<WalkingNonInteractable>() != null)
                    {
                        npc.gameObject.GetComponent<WalkingNonInteractable>().InteruptWait();
                    }

                    return;
                }
            }
            // If npc has nothing to say at all, end the convo and stop focusing camera
            EndConversation();
            StopFocusCamera();
        }
    }

    public void ProgressDialogue()
    {
        // Called by "Next" button on dialogue UI
        string dialogueLine;

        if (!hasFinishedGame)
        {
            dialogueLine = interactingWith.ReturnDialoguePoint();
        }
        else
        {
            dialogueLine = interactingWith.ReturnEndDialogue();
        }

        if (dialogueLine != null)
        {
            // If the NPC has something else to say, show it
            UI.ShowDialogue(dialogueLine, interactingWith.displayName);

            // Plays a click sound
            AudioManager.Instance.PlaySoundEffect2D("buttonClickSmall", true);
        }
        else
        {
            // If NPC has no dialogue left, check for quests to give / complete
            UI.HideDialogue();
            interactingWith.ResetDialogue();

            if (interactingWith.isQuestGiver)
            {
                if (qmanager.InteractWith(interactingWith))
                {
                    // If npc has quests to accept / give, do that instead of this
                    return;
                }
            }

            // If nothing left to say, end the convo & defocus camera
            EndConversation();
            StopFocusCamera();
        }
    }

    // Called when player hits "leave" button on dialogue UI
    public void EndConversation()
    {
        // Allows player to move again, hides dialogue UI, resets dialogue and returns cam control to the player
        playerMove.StartMoving();
        UI.HideDialogue();
        
        interactingWith.ResetDialogue();

        if (interactingWith.gameObject.GetComponent<WalkingNonInteractable>() != null)
        {
            // Checks if NPC should begin walking after interaction - if true, select a new point; if false, flag to NPC walk script that interaction has happened
            if (interactingWith.walkAfterConversation)
            {
                interactingWith.gameObject.GetComponent<WalkingNonInteractable>().NewPoint();
            }
            else
            {
                interactingWith.gameObject.GetComponent<WalkingNonInteractable>().StartMovingAgain();
            }
            
        }

        // Switches from NPC focus camera to 
        StopFocusCamera();
    }

    public void StartFocusCameraMove(Transform target)
    {
        if(Camera.main != null)
        {
            playerCamera = Camera.main.gameObject;

            // De-activates player camera
            playerCamera.SetActive(false);

            if(focusCamera == null)
            {
                // Added by Joe: Re-finds focus camera if it became null (can happen when switching scenes)
                focusCamera = FindFocusCamera();
            }

            // Sssigns initial position of focus cam to match players current position
            focusCamera.transform.position = playerCamera.transform.position;
            focusCamera.transform.rotation = playerCamera.transform.rotation;

            // Activates focus cam 
            focusCamera.SetActive(true);

            // Sets target transform for focus cam
            targetCameraTransform = target;
            // Sets movement state to "moving"
            focusCameraCurrentState = focusCameraState.moving;

            //Added by Joe - Hide hotbar/player stats UI
            GameObject.FindGameObjectWithTag("Hotbar").GetComponent<HotbarPanel>().HideHotbarAndStatPanels();
        }
    }
    public void StopFocusCamera()
    {
        // Re-activates player camera & de-activates focus cam
        playerCamera.SetActive(true);
        focusCamera.SetActive(false);

        // Sets target transform to null, sets camera mode to normal
        targetCameraTransform = null;
        focusCameraCurrentState = focusCameraState.normal;

        //Added by Joe - Re-show hotbar/player stats UI
        GameObject.FindGameObjectWithTag("Hotbar").GetComponent<HotbarPanel>().ShowHotbarAndStatPanels();
    }

    // Returns the "focus" camera associated with the player
    private GameObject FindFocusCamera()
    {
        return GameObject.FindGameObjectWithTag("Player").transform.Find("FocusCamera").gameObject;
    }
}
