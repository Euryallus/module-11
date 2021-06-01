using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Handles quest UI elements (e.g. HUD quest list, player accept screen etc.)
// Development window:  Prototype phase
// Inherits from:       MonoBehaviour

public class QuestUI : MonoBehaviour
{
    [Header("Accept quest UI")]                                         // References to quest Accept UI elements (quest title, description, objectives etc.)
    [SerializeField]    private TMP_Text questTitle;                
    [SerializeField]    private TMP_Text questDescription;
    [SerializeField]    private TMP_Text questObjectives;

    [Header("Complete quest UI")]                                       // References to quest Complete UI elements (quest title, complete message, rewards etc.)
    [SerializeField]    private TMP_Text questCompleteTitle;
    [SerializeField]    private TMP_Text questCompleteMessage;
    [SerializeField]    private TMP_Text questReward;

    [Header("Quest HUD list")]                                          // References to quest HUD elements
    [SerializeField]    private TMP_Text questTitleHUD;
    [SerializeField]    private GameObject questMarkerBackground;
    [SerializeField]    private float questNameHeight = 35f;

    [Header("Canvas groups")]                                           // References to UI canvas groups
    [SerializeField]    private CanvasGroup questAcceptCanvasGroup;
    [SerializeField]    private CanvasGroup questCompleteCanvasGroup;

    
    private List<TMP_Text> questNamesHUD = new List<TMP_Text>();        // List of quest HUD names (displays @ side of screen as quest backlog list)

    [Header("HUD list element blueprint")]
    [SerializeField]    private TMP_Text defaultName;                   // HUD quest name blueprint 

    private void Start()
    {
        // Hides all UI apart from quest HUD element to begin with
        HideQuestAccept();
        HideQuestComplete();
    }

    // Displays data associated with [quest] for player to accept or decline quest
    public void DisplayQuestAccept(QuestData quest)
    {
        // Allows cursor to move around screen & assigns correct strings to text elements (e.g. quest description)
        Cursor.lockState = CursorLockMode.None;
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

        // Allows player to see & interact with quest UI
        questAcceptCanvasGroup.alpha = 1;
        questAcceptCanvasGroup.blocksRaycasts = true;
        questAcceptCanvasGroup.interactable = true;
    }

    public void HideQuestAccept()
    {
        // Resets cursor state & hides quest UI
        Cursor.lockState = CursorLockMode.Locked;
        questAcceptCanvasGroup.alpha = 0;
        questAcceptCanvasGroup.blocksRaycasts = false;
        questAcceptCanvasGroup.interactable = false;
    }

    public void DisplayQuestComplete(QuestData quest)
    {
        // Allows cursor to move around screen
        Cursor.lockState = CursorLockMode.None;
        // Fills in text components
        questCompleteTitle.text = quest.questName;
        questCompleteMessage.text = quest.questCompleteDialogue;
        // Rewards system still being worked out - will be added in production phase
        string rewardText = "";

        foreach(ItemGroup reward in quest.rewards)
        {
            rewardText += reward.Item.UIName + " x" + reward.Quantity.ToString() + "\n";
        }

        questReward.text = rewardText;

        // Displays UI 
        questCompleteCanvasGroup.alpha = 1;
        questCompleteCanvasGroup.blocksRaycasts = true;
        questCompleteCanvasGroup.interactable = true;
    }

    public void HideQuestComplete()
    {
        // Hides UI and resets cursor state
        Cursor.lockState = CursorLockMode.Locked;
        questCompleteCanvasGroup.alpha = 0;
        questCompleteCanvasGroup.blocksRaycasts = false;
        questCompleteCanvasGroup.interactable = false;
    }

    // Adds quest name to HUD side bar
    public void AddHUDQuestName(string name)
    {
        // Creates new HUD quest name object, enables it & sets transform parent
        TMP_Text newName = Instantiate(defaultName);
        newName.transform.gameObject.SetActive(true);
        newName.transform.SetParent(questMarkerBackground.transform);

        // If there are already quest names in list, adjust position to be at bottom of list
        if(questNamesHUD.Count != 0)
        {
            newName.rectTransform.anchoredPosition = new Vector2(defaultName.rectTransform.anchoredPosition.x, defaultName.rectTransform.anchoredPosition.y - ((questNameHeight + 5) * questNamesHUD.Count));
        }
        else
        {
            // If no other names are in the list, set to top of list (*default* position)
            newName.rectTransform.anchoredPosition = defaultName.rectTransform.anchoredPosition;
        }

        // Set display name to corrispond with quest
        newName.text = name;
        // Add new naame to list
        questNamesHUD.Add(newName);
    }

    // Changes quest HUD display to reflect when quest is complete but not handed in
    public void SetHUDQuestNameCompleted(string name)
    {
        // Tries to locate name in list of HUD names
        foreach(TMP_Text questName in questNamesHUD)
        {
            if(questName.text == name)
            {
                // If found, change its colour to green and drop out of func.
                questName.color = Color.green;
                return;
            }
        }
    }

    // Removes name of HUD quest list (used when quest is completed)
    public void RemoveHUDQuestName(string name)
    {
        for(int i = 0; i < questNamesHUD.Count; i++)
        {
            // Cycles each name in list to find match with name passed as param
            if(questNamesHUD[i].text == name)
            {
                // Destroys quest name HUD element
                Destroy(questNamesHUD[i]);
                questNamesHUD.RemoveAt(i);

                // Shifts all other quest names around so no gaps appear
                if(questNamesHUD.Count != 0)
                {
                    for(int j = 0; j < questNamesHUD.Count; j ++)
                    {
                        questNamesHUD[j].rectTransform.anchoredPosition = new Vector2(defaultName.rectTransform.anchoredPosition.x, defaultName.rectTransform.anchoredPosition.y - (60 * j));
                    }
                }
                // Drops out of func. once name has been found & adjusted
                return;
            }
        }
    }

}
