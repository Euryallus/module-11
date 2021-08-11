using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class QuestMenuButton : MonoBehaviour
{
    public QuestData questDisplay;
    public TMP_Text buttonTitle;

    public QuestMenuUI ui;

    private void Start()
    {
        gameObject.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = questDisplay.questName;
    }

    public void DisplayQuestData()
    {
        QuestMenuButton button = gameObject.GetComponent<QuestMenuButton>();

        

        //buttonTitle.text = questDisplay.questName;

        ui.title.text = button.questDisplay.questName;

        ui.description.text = button.questDisplay.questDescription;

        string objectivesText = "";
        string rewardsText = "";
        foreach(QuestObjective objective in button.questDisplay.objectives)
        {
            objectivesText = objectivesText + objective.taskName + "\n";
        }

        ui.objectives.text = objectivesText;

        if (button.questDisplay.rewards.Count != 0)
        {
            foreach (ItemGroup reward in button.questDisplay.rewards)
            {
                rewardsText = rewardsText + reward.Item.UIName + "\n";
            }
        }

        ui.rewards.text = rewardsText;
    }
}
