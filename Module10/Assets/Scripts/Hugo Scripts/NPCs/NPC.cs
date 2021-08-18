using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Stores data retaining to the NPC component is attached to. Manages dialogue & flags if NPC is also a quest giver
// Development window:  Prototype phase & production phase
// Inherits from:       MonoBehaviour

public class NPC : InteractableWithOutline, IPersistentSceneObject
{
    [Header("NPC data")]

                        public string npcName;                      // Stores Name of NPC
                        public bool isQuestGiver = false;           // Flags if NPC has quests associated withh them
                        public string displayName = "A Stranger";   // Name displayed during dialogue

    [Header("Does dialogue influence NPC movement")]
    public bool walkAfterConversation = false;                      // Flags if player interactions could cause NPC to move (only works if NPC also has walkingNonInteractable component)

    [Header("Dialogue data")]

    [TextArea(1, 5)]
    [SerializeField]    private string[] dialogueLines;             // List of dialogue lines NPC will say if interacted with

    [SerializeField]    private string[] endGameDialogueLines;      // Flags if NPC has alternative dialogue lines for after player has completed game
                        private int endDialogueProgression = 0;     // Stores player progression through end dialogue lines

    [HideInInspector]   public bool haveTalkedToBefore = false;     // Flags if player has talked to the NPC before 
                        private int dialogueProgression = 0;        // Stores player progression through dialogue lines

    [Header("Focus camera point")]

                        public Transform cameraFocusPoint;          // Stores transform point associated with NPC (point camera focuses on while talking)

    // Added by Joe:
    private SpriteRenderer      npcIndicator;       // The indicator showing if this NPC has something to say, or a quest to give/recieve
    private IndicatorShowMode   indicatorShowMode;  // Determines which sprite (if any) is being shown above the NPC
    private QuestManager        questManager;       // Reference to the QuestManager script
    private Vector3             basePosition;       // The NPC's default position in the world

    // All ways the npcIndicator can be displayed:
    public enum IndicatorShowMode
    {
        None,       // No indicator is shown
        Talk,       // The talk indicator (a blue 'i') is shown
        GiveQuest,  // The give quest indicator (a green '!') is shown
        AcceptQuest // The accept quest indicator (a green tick) is shown
    }

    protected override void Start()
    {
        // Code in this function was added by Joe

        base.Start();

        // Allows save / load functionality
        SaveLoadManager.Instance.SubscribeSceneSaveLoadEvents(OnSceneSave, OnSceneLoadSetup, OnSceneLoadConfigure);

        // Sets up InteractableWithOutline properties
        interactionRange = 4.0f;
        interactionSound = "";

        // Adds the NPC indicator and positions it above the player based on the height of the camera focus point
        GameObject npcIndicatorParent = Instantiate(GameSceneUI.Instance.NPCIndicatorPrefab, transform);
        npcIndicatorParent.transform.position = new Vector3(npcIndicatorParent.transform.position.x, cameraFocusPoint.position.y + 0.75f, npcIndicatorParent.transform.position.z);

        npcIndicator = npcIndicatorParent.transform.GetChild(0).GetComponent<SpriteRenderer>();

        // Store the NPC's default position, used for saving
        basePosition = transform.position;

        if (isQuestGiver)
        {
            // Quest givers show the 'give quest' sprite by default
            SetIndicatorShowMode(IndicatorShowMode.GiveQuest);
        }
        else if(dialogueLines.Length > 0)
        {
            // Standard NPCs with dialogue show the 'talk' indicator by default
            SetIndicatorShowMode(IndicatorShowMode.Talk);
        }
        else
        {
            // If there are no quests to give or dialogue lines, show no indicator
            SetIndicatorShowMode(IndicatorShowMode.None);
        }
    }

    protected override void OnDestroy()
    {
        // Added by Joe - Unsubscribes from save events to prevent null ref errors if this NPC is destroyed
        SaveLoadManager.Instance.UnsubscribeSceneSaveLoadEvents(OnSceneSave, OnSceneLoadSetup, OnSceneLoadConfigure);
    }

    protected override void Update()
    {
        // Update code added by Joe
        base.Update();

        if(questManager == null)
        {
            // Finds the QuestManager if it wasn't already found
            questManager = GameObject.FindGameObjectWithTag("QuestManager").GetComponent<QuestManager>();
        }

        if(isQuestGiver && indicatorShowMode == IndicatorShowMode.None)
        {
            // This NPC is a quest giver and may be waiting for the player to hand in a quest
            foreach (var quest in questManager.PlayerQuestData.questBacklog)
            {
                // Loop through all quests in the backlog and find one that is completed, not handed in, and should be handed in to this NPC
                if (quest.questCompleted && !quest.questHandedIn && quest.handInToGiver && quest.handInNPCName == npcName)
                {
                    // There is a quest to be handed in, show the 'accept quest' indicator sprite
                    SetIndicatorShowMode(IndicatorShowMode.AcceptQuest);
                }
            }
        }
    }

    public void SetIndicatorShowMode(IndicatorShowMode showMode)
    {
        // Changes indicatorShowMode and hides/shows the indicator with the appropriate sprite

        if (indicatorShowMode != showMode)
        {
            // Set the new show mode
            indicatorShowMode = showMode;

            // Show the indicator by default
            npcIndicator.transform.parent.gameObject.SetActive(true);

            switch (showMode)
            {
                case IndicatorShowMode.None:
                    // Hide the indicator if the 'None' ShowMode was set
                    npcIndicator.transform.parent.gameObject.SetActive(false);
                    break;

                case IndicatorShowMode.Talk:
                    // Show the talk sprite
                    npcIndicator.sprite = GameSceneUI.Instance.NPCIndicatorSprites[0];
                    break;

                case IndicatorShowMode.GiveQuest:
                    // Show the give quest sprite
                    npcIndicator.sprite = GameSceneUI.Instance.NPCIndicatorSprites[1];
                    break;

                case IndicatorShowMode.AcceptQuest:
                    // Show the accept quest sprite
                    npcIndicator.sprite = GameSceneUI.Instance.NPCIndicatorSprites[2];
                    break;
            }
        }
    }

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
            else
            {
                // Added by Joe: Last dialogue line was reached, hide the 'talk' indicator
                SetIndicatorShowMode(IndicatorShowMode.None);
            }
        }

        // If no dialogue is found, return NULL
        return null;
    }

    public string ReturnEndDialogue()
    {
        // Checks if NPC has more to say using endDialogueProgression pointer
        if (endDialogueProgression < endGameDialogueLines.Length)
        {
            // Returns next line from list & incriments pointer
            string returnString = endGameDialogueLines[endDialogueProgression];

            ++endDialogueProgression;

            return returnString;
        }
        else
        {
            // If end of dialogue is met and NPC is a quest giver, dont repeat the lines
            if (isQuestGiver)
            {
                haveTalkedToBefore = true;
            }
        }

        return null;
    }

    //Resets dialogue after conversation if NPC is not a quest giver
    public void ResetDialogue()
    {
        // Called when player exits conversation, checks if "point in conversation" needs returning to 0
        if(!haveTalkedToBefore)
        {
            dialogueProgression = 0;
            endDialogueProgression = 0;
        }
    }

    public void OnSceneSave(SaveData saveData)
    {
        // Added by Joe - Saves the current indicatorShowMode
        saveData.AddData(GetUniquePositionId() + "_indicator", indicatorShowMode);
    }

    public void OnSceneLoadSetup(SaveData saveData)
    {
        // Added by Joe - Loads the indicatorShowMode that was being used when the game was saved
        SetIndicatorShowMode(saveData.GetData<IndicatorShowMode>(GetUniquePositionId() + "_indicator"));
    }

    public void OnSceneLoadConfigure(SaveData saveData) { }

    private string GetUniquePositionId()
    {
        // Returns a unique id used for saving based on the NPC's base position
        return "npc_" + basePosition.x + "_" + basePosition.y + "_" + basePosition.z;
    }
}
