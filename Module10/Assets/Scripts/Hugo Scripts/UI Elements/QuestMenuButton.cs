using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class QuestMenuButton : MonoBehaviour
{
    public QuestData questDisplay;
    public TMP_Text buttonTitle;

    public QuestMenuUI ui;

    public void DisplayQuestData()
    {
        ui.title.text = gameObject.GetComponent<QuestMenuButton>().questDisplay.questName;

        ui.description.text = gameObject.GetComponent<QuestMenuButton>().questDisplay.questDescription;
    }
}
