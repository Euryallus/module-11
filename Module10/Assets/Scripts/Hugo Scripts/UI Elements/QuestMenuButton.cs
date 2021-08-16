using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Button used to display quest info in quest menu
// Development window:  Production phase
// Inherits from:       MonoBehaviour


public class QuestMenuButton : MonoBehaviour
{
    public QuestData questDisplay;  // Stores quest the button will expand
    public TMP_Text buttonTitle;    // Ref. to text component of button

    public QuestMenuUI ui;          // Ref. to quest UI component of parent

    private void Start()
    {
        // Sets text of button (ngl i spent about 2 hours trying to work out why using buttonTitle.text wouldn't work, this is the only alternative i found)
        gameObject.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = questDisplay.questName;
    }

    public void DisplayQuestData()
    {
        // Grabs ref. to self (called from the "button" component so reference to self is needed)
        QuestMenuButton button = gameObject.GetComponent<QuestMenuButton>();

        // Assigns quest description & title to correct display within quest menu
        ui.title.text = button.questDisplay.questName;
        ui.description.text = button.questDisplay.questDescription;

        // Formats rewards & objectives to be displayed
        string objectivesText = "";
        string rewardsText = "";

        // Cycles each objective & takes name to put into readable format
        foreach(QuestObjective objective in button.questDisplay.objectives)
        {
            objectivesText = objectivesText + objective.taskName + "\n";
        }
        // Displays objective data on screen
        ui.objectives.text = objectivesText;

        // Cycles each reward & takes item name to put into readable format
        if (button.questDisplay.rewards.Count != 0)
        {
            foreach (ItemGroup reward in button.questDisplay.rewards)
            {
                rewardsText = rewardsText + reward.Item.UIName + "\n";
            }
        }
        // Displays rewards data on screen
        ui.rewards.text = rewardsText;

        // Plays a click sound
        AudioManager.Instance.PlaySoundEffect2D("buttonClickSmall", true);
    }
}
