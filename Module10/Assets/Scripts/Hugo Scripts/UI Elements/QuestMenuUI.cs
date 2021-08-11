using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuestMenuUI : UIPanel
{
    [SerializeField] private GameObject buttonPrefab;

    [SerializeField] private GameObject questButtonContainer;

    [SerializeField] private float vertOffset = 30f;

    [Header("Container refs")]
    public TMP_Text title;
    public TMP_Text description;
    public TMP_Text objectives;
    public TMP_Text rewards;

    private CanvasGroup group;

    // NEEDED: Quest title, quest description, objectives rewards etc. containers

    //Each button has a quest ref. on it - when pressed, replace title description etc. with data from ref. 

    [SerializeField] private List<GameObject> buttons = new List<GameObject>();

    protected override void Start()
    {
        base.Start();

        group = gameObject.GetComponent<CanvasGroup>();

        group.interactable = false;
        group.blocksRaycasts = false;
        group.alpha = 0f;
    }


    private void Update()
    {
        // Adjusted by Joe to work with UIPanel base class/fix bugs related to showing/hiding panels

        if(Input.GetKeyDown(KeyCode.O))
        {
            if(!showing && CanShowUIPanel())
            {
                // Show panel
                showing = true;
                group.alpha = 1.0f;
                group.interactable = true;
                group.blocksRaycasts = true;

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().StopMoving();

                AudioManager.Instance.PlaySoundEffect2D("buttonClickMain1");
            }
            else if(showing)
            {
                // Hide panel
                showing = false;
                group.alpha = 0.0f;
                group.blocksRaycasts = false;

                Cursor.lockState = CursorLockMode.Locked;

                GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().StartMoving();

                AudioManager.Instance.PlaySoundEffect2D("buttonClickMain2");
            }
        }
    }

    public void AddQuestButton(QuestData questToAdd)
    {
        GameObject newButton = Instantiate(buttonPrefab, questButtonContainer.transform);

        buttons.Add(newButton);

        

        Vector2 buttonDimensions = buttonPrefab.GetComponent<RectTransform>().sizeDelta;
        questButtonContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(buttonDimensions.x, (buttonDimensions.y + 20f) * buttons.Count);
        

        SortButtonPosition();

        newButton.GetComponent<QuestMenuButton>().questDisplay = questToAdd;
        newButton.GetComponent<QuestMenuButton>().buttonTitle.text = "Quest name";
        newButton.SetActive(true);
    }

    public void RemoveButton(QuestData questToRemove)
    {
        for(int i = 0; i < buttons.Count; i++)
        {
            GameObject button;

            button = buttons[i];

            if(button.GetComponent<QuestMenuButton>().questDisplay.name == questToRemove.name)
            {
                buttons.RemoveAt(i);

                SortButtonPosition();

                break;
            }
        }
    }

    private void SortButtonPosition()
    {
        Vector2 offset = new Vector2(0, vertOffset);

        for (int i = 0; i < buttons.Count; i++)
        {
            if (i == 0)
            {
                buttons[0].GetComponent<RectTransform>().anchoredPosition = buttonPrefab.GetComponent<RectTransform>().anchoredPosition;
            }
            else
            {
                buttons[i].GetComponent<RectTransform>().anchoredPosition = buttons[i - 1].GetComponent<RectTransform>().anchoredPosition + offset;
            }
        }
    }
}
