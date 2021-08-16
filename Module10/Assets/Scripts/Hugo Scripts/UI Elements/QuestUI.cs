using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Handles quest UI elements (e.g. HUD quest list, player accept screen etc.)
// Development window:  Prototype phase & production phase
// Inherits from:       UIPanel

public class QuestUI : UIPanel
{
    [Header("Accept quest UI")]                                         // References to quest Accept UI elements (quest title, description, objectives etc.)
    [SerializeField]    private TMP_Text questTitle;                
    [SerializeField]    private TMP_Text questDescription;
    [SerializeField]    private TMP_Text questObjectives;
    [SerializeField]    private Button rejectButton;

    [Header("Complete quest UI")]                                       // References to quest Complete UI elements (quest title, complete message, rewards etc.)
    [SerializeField]    private TMP_Text questCompleteTitle;
    [SerializeField]    private TMP_Text questCompleteMessage;
    [SerializeField]    private TMP_Text questReward;

    [Header("Quest HUD list")]                                          // References to quest HUD elements
    [SerializeField]    private TMP_Text questTitleHUD;
    [SerializeField]    private GameObject questMarkerBackground;
    [SerializeField]    private float questNameHeight = 35f;

    [SerializeField] private TMP_Text latestQuestNameText;
    [SerializeField] private List<TMP_Text> latestQuestObjectivesText;

    [Header("Canvas groups")]                                           // References to UI canvas groups
    [SerializeField]    private CanvasGroup questAcceptCanvasGroup;
    [SerializeField]    private CanvasGroup questCompleteCanvasGroup;

    
    private List<TMP_Text> questNamesHUD = new List<TMP_Text>();        // List of quest HUD names (displays @ side of screen as quest backlog list)

    [Header("HUD list element blueprint")]
    [SerializeField]    private TMP_Text defaultName;                   // HUD quest name blueprint 

    protected override void Awake()
    {
        RemoveHUDQuestName();
        base.Awake();

    }

    protected override void Start()
    {
        base.Start();

        // Hides all UI apart from quest HUD element to begin with
        HideQuestAccept();
        HideQuestComplete();
    }

    // Displays data associated with [quest] for player to accept or decline quest
    public void DisplayQuestAccept(QuestData quest, bool forceAccept = false)
    {
        // Allows cursor to move around screen & assigns correct strings to text elements (e.g. quest description)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        questTitle.text = quest.questName;
        questDescription.text = quest.questDescription;

        // Temp vairable used to calculate text for "questObjectives" display
        string objectivesText = "";


        foreach(QuestObjective objective in quest.objectives)
        {
            // Objectives are listed one after the other
            objectivesText = objectivesText + "\n" + objective.taskName;
        }

        // Once calculated, displays list of objectives
        questObjectives.text = objectivesText;

        // Checks if quest HAS to be accepted - if so, grey out "decline" button
        if(forceAccept)
        {
            rejectButton.interactable = false;
        }
        else
        {
            rejectButton.interactable = true;
        }

        // Allows player to see & interact with quest UI
        questAcceptCanvasGroup.alpha = 1;
        questAcceptCanvasGroup.blocksRaycasts = true;
        questAcceptCanvasGroup.interactable = true;

        showing = true;
    }

    public void HideQuestAccept()
    {
        // Resets cursor state & hides quest UI
        Cursor.lockState = CursorLockMode.Locked;
        questAcceptCanvasGroup.alpha = 0;
        questAcceptCanvasGroup.blocksRaycasts = false;
        questAcceptCanvasGroup.interactable = false;

        showing = false;
    }

    public void DisplayQuestComplete(QuestData quest)
    {
        // Allows cursor to move around screen
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Fills in text components
        questCompleteTitle.text = quest.questName;
        questCompleteMessage.text = quest.questCompleteDialogue;
        // Rewards system still being worked out - will be added in production phase
        string rewardText = "";

        // Cycles each reward & creates readable list to display
        foreach(ItemGroup reward in quest.rewards)
        {
            rewardText += reward.Item.UIName + " x" + reward.Quantity.ToString() + "\n";
        }

        questReward.text = rewardText;

        // Displays UI 
        questCompleteCanvasGroup.alpha = 1;
        questCompleteCanvasGroup.blocksRaycasts = true;
        questCompleteCanvasGroup.interactable = true;

        showing = true;
    }

    public void HideQuestComplete()
    {
        // Hides UI and resets cursor state
        Cursor.lockState = CursorLockMode.Locked;
        questCompleteCanvasGroup.alpha = 0;
        questCompleteCanvasGroup.blocksRaycasts = false;
        questCompleteCanvasGroup.interactable = false;

        showing = false;
    }

    // Adds quest name to HUD side bar
    public void AddHUDQuestName(QuestData quest)
    {
        // Assigns quest name to the HUD element
        latestQuestNameText.text = quest.questName;

        // Cycles each objective display & if the quest has that many tasks it displays them (if not spare objective displays set to "")
        if(quest.objectives.Count > 0)
        {
            for(int i = 0; i < latestQuestObjectivesText.Count; i++)
            {
                if (quest.objectives.Count > i)
                {
                    latestQuestObjectivesText[i].text = quest.objectives[i].taskName;
                }
                else
                {
                    latestQuestObjectivesText[i].text = "";
                }
            }
        }
    }

    // Changes quest HUD display to reflect when quest is complete but not handed in
    public void SetHUDQuestNameCompleted(QuestData quest)
    {
        // Cycles each objective in the quest being displayed on the HUD - if any objectives are complete, mark them as done by striking through
        for(int i = 0; i < quest.objectives.Count; i++)
        {
            if(quest.objectives[i].taskComplete)
            {
                if(i < latestQuestObjectivesText.Count)
                {
                    latestQuestObjectivesText[i].fontStyle = FontStyles.Strikethrough;
                }
            }
        }
    }

    // Removes name of HUD quest list (used when quest is completed)
    public void RemoveHUDQuestName()
    {
        // Sets display of all quest HUD elements to "" (empty)
        latestQuestNameText.text = "";
        foreach(TMP_Text textDis in latestQuestObjectivesText)
        {
            textDis.text = "";
            textDis.fontStyle = FontStyles.Normal;
        }
    }

}
