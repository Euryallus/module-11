using System.Collections.Generic;
using UnityEngine;

// ||=======================================================================||
// || PuzzleButtonSequence: A group of buttons that have to be pressed in   ||
// ||   a certain order to open a door.                                     ||
// ||=======================================================================||
// || Used on prefab: Joe/PuzzleElements/PuzzleButtonSequence               ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public class PuzzleButtonSequence : MonoBehaviour, IPersistentObject
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [Header("Important: Set unique id")]
    [Header("Puzzle Button Sequence")]

    [SerializeField] private string              id;                    // Unique id for the sequence to save/load its state
    [SerializeField] private List<PuzzleButton>  buttonsInSequence;     // All buttons that make up the sequence
    [SerializeField] private DoorPuzzleData[]    connectedDoors;        // Doors that will be opened/closed when the sequence is complete

    #endregion

    private int     currentSequenceIndex = 0;   // Number of successful button presses
    private bool    sequenceCompleted;          // Whether the sequence was completed

    private void Awake()
    {
        for (int i = 0; i < buttonsInSequence.Count; i++)
        {
            // Register all connected buttons as being in the sequence
            buttonsInSequence[i].RegisterWithSequence(this);
        }
    }

    void Start()
    {
        // Subscribe to save/load events so the sequence state is saved/loaded with the game
        SaveLoadManager.Instance.SubscribeSaveLoadEvents(OnSave, OnLoadSetup, OnLoadConfigure);

        if (string.IsNullOrEmpty(id))
        {
            // Warning if an id was not set
            Debug.LogWarning("IMPORTANT: PuzzleButtonSequence exists without id. All sequences require a *unique* id for saving/loading data. Click this message to view the problematic GameObject.", gameObject);
        }

        // Trigger fail to restore the default state of connected doors
        SequenceFailedEvents();
    }

    private void OnDestroy()
    {
        // Unsubscribe from save/load events if the sequence GameObject is destroyed to prevent null reference errors
        SaveLoadManager.Instance.UnsubscribeSaveLoadEvents(OnSave, OnLoadSetup, OnLoadConfigure);
    }

    public void OnSave(SaveData saveData)
    {
        // Save whether the sequence was completed successfully
        saveData.AddData("buttonSequenceCompleted_" + id, sequenceCompleted);
    }

    public void OnLoadSetup(SaveData saveData)
    {
        // Load whether the sequence was completed successfully
        sequenceCompleted = saveData.GetData<bool>("buttonSequenceCompleted_" + id);

        if (sequenceCompleted)
        {
            // The sequence was completed, trigger complete events
            SequenceCompleteEvents();
        }
        else
        {
            // The sequence was not completed, trigger fail events
            SequenceFailedEvents();
        }
    }

    public void OnLoadConfigure(SaveData saveData) { } // Nothing to configure

    public void ButtonInSequencePressed(PuzzleButton button)
    {
        if(buttonsInSequence.IndexOf(button) == currentSequenceIndex)
        {
            // The index of the button in the array matches the sequence index,
            //   meaning the correct button was pressed
            currentSequenceIndex++;

            if(currentSequenceIndex == buttonsInSequence.Count)
            {
                // All buttons were pressed in order, sequence complete
                sequenceCompleted = true;

                // Completion sound
                AudioManager.Instance.PlaySoundEffect2D("notification1");

                // Trigger sequence complete events
                SequenceCompleteEvents();
            }
            else
            {
                // Correct button sound
                AudioManager.Instance.PlaySoundEffect2D("coins");
            }
        }
        else
        {
            // The wrong button was pressed, reset sequence
            sequenceCompleted = false;
            currentSequenceIndex = 0;

            // Incorrect button sound
            AudioManager.Instance.PlaySoundEffect2D("sealExplosion");

            // Trigger sequence failed events
            SequenceFailedEvents();
        }
    }

    private void SequenceCompleteEvents()
    {
        for (int i = 0; i < connectedDoors.Length; i++)
        {
            // Open/close all doors depending on their default states
            DoorPuzzleData doorData = connectedDoors[i];
            if (doorData.OpenByDefault)
            {
                doorData.Door.SetAsClosed();
            }
            else
            {
                doorData.Door.SetAsOpen(doorData.OpenInwards);
            }
        }
    }

    private void SequenceFailedEvents()
    {
        for (int i = 0; i < connectedDoors.Length; i++)
        {
            // Close/open all doors depending on their default states
            DoorPuzzleData doorData = connectedDoors[i];
            if (doorData.OpenByDefault)
            {
                doorData.Door.SetAsOpen(doorData.OpenInwards);
            }
            else
            {
                doorData.Door.SetAsClosed();
            }
        }
    }
}
