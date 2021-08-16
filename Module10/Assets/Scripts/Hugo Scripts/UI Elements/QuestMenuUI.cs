using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Main author:         Hugo Bailey
// Additional author:   Joe Allen
// Description:         Handles quest menu UI components
// Development window:  Production phase
// Inherits from:       UIPanel

public class QuestMenuUI : UIPanel
{
    [SerializeField] private GameObject buttonPrefab;           // Ref. to QuestMenuButton object (used as template)
    [SerializeField] private GameObject questButtonContainer;   // Ref. to button container (UI, allows scrolling)
    [SerializeField] private float vertOffset = 30f;            // Offset added to new quest buttons

    [Header("Container refs")]
    public TMP_Text title;          // Ref. to title text box
    public TMP_Text description;    // Ref. to description text box
    public TMP_Text objectives;     // Ref. to objective text box
    public TMP_Text rewards;        // Ref. to rewards text box

    private CanvasGroup group; 
    //Each button has a quest ref. on it - when pressed, replace title description etc. with data from ref. 

    [SerializeField] private List<GameObject> buttons = new List<GameObject>(); // List of refs. to quest buttons created

    protected override void Start()
    {
        base.Start();

        // Creates ref. to canvas group component
        group = gameObject.GetComponent<CanvasGroup>();

        // Prevents menu being interacted with when not in use
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
                // Displays panel
                showing = true;
                group.alpha = 1.0f;
                group.interactable = true;
                group.blocksRaycasts = true;

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                // Prevents player moving then UI is open
                GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().StopMoving();

                AudioManager.Instance.PlaySoundEffect2D("buttonClickMain1");
            }
            else if(showing)
            {
                // Hides panel
                showing = false;
                group.alpha = 0.0f;
                group.blocksRaycasts = false;

                Cursor.lockState = CursorLockMode.Locked;

                // Allows player to move again
                GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().StartMoving();

                AudioManager.Instance.PlaySoundEffect2D("buttonClickMain2");
            }
        }
    }

    // Creates new instance of the "quest select" button for new quest collected by player
    public void AddQuestButton(QuestData questToAdd)
    {
        // Creates new instance of quest button
        GameObject newButton = Instantiate(buttonPrefab, questButtonContainer.transform);
        
        // Adds new button to list of buttons
        buttons.Add(newButton);

        // Sets button size
        Vector2 buttonDimensions = buttonPrefab.GetComponent<RectTransform>().sizeDelta;
        questButtonContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(buttonDimensions.x, (buttonDimensions.y + 20f) * buttons.Count);
        
        // Sets all button positions to be one under the other
        SortButtonPosition();

        newButton.GetComponent<QuestMenuButton>().questDisplay = questToAdd;
        newButton.GetComponent<QuestMenuButton>().buttonTitle.text = "Quest name";

        // Enables button
        newButton.SetActive(true);
    }

    // Removes button associated with quest passed in params
    public void RemoveButton(QuestData questToRemove)
    {
        // Cycles each button, if button is associated with quest passed remove button from list & re-order the rest
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

    // Cycles each button, positions each below the last by [vertOffset]
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
