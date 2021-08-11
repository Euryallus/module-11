using UnityEngine;

// ||=======================================================================||
// || AbilityUnlockCutscene: The cutscene triggered when the player         ||
// ||   acquires a new ability.                                             ||
// ||=======================================================================||
// || Used on prefab: Joe/UI/AbilityUnlock/AbilityUnlockPanel               ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the production phase (Module 11).                                 ||
// ||=======================================================================||

public class AbilityUnlockCutscene : CutsceneTriggerer
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [Header("Ability Unlock Cutscene")]

    [SerializeField] private GameObject abilityUnlockPanelPrefab;   // The UI panel to be instantiated during the cutscene

    #endregion

    private CursorLockMode      returnToLockState;  // The state to return the cursor to when the cutscene is done

    private Item                abilityItem;        // The ability item that was aquired to trigger this cutscene
    private PlayerAbilityType   abilityType;        // The ability type that was unlocked/upgraded
    private int                 upgradeLevel;       // The upgrade level of the ability

    public void Setup(Item item, PlayerAbilityType ability, int level)
    {
        abilityItem     = item;
        abilityType     = ability;
        upgradeLevel    = level;
    }

    public override void StartCutscene()
    {
        base.StartCutscene();

        CinematicsCanvas cinematicsCanvas = GameSceneUI.Instance.GetActiveCinematicsCanvas();

        // Instantiate the unlock panel as a child of the cinematics canvas, setting its sibling index so it renders behind the fade cover
        AbilityUnlockPanel unlockPanel = Instantiate(abilityUnlockPanelPrefab, cinematicsCanvas.transform).GetComponent<AbilityUnlockPanel>();
        unlockPanel.transform.SetSiblingIndex(2);

        // Setup the unlock panel so it can display the correct info
        unlockPanel.Setup(abilityItem, abilityType, upgradeLevel);

        // End the cutscene when the continue button is pressed
        unlockPanel.ContinueButtonPressEvent += EndCutscene;

        // Fade music back in when the panel animation is done
        unlockPanel.PanelAnimationDoneEvent += FadeInMusic;

        // Store the current lock state of the cursor so it can revert back to this once the cutscene is done
        returnToLockState = Cursor.lockState;

        // Show/unlock the cursor so the player can interact with the unlock panel
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Fade background music out and play a fanfare sound
        AudioManager.Instance.FadeGlobalVolumeMultiplier(0.0f, 0.2f);
        AudioManager.Instance.PlayMusicalSoundEffect("unlockFanfare");
    }

    private void FadeInMusic()
    {
        // Fade background music back in
        AudioManager.Instance.FadeGlobalVolumeMultiplier(1.0f, 1.5f);
    }

    protected override void EndCutscene()
    {
        base.EndCutscene();

        // If the cursor was locked before the cutscene, re-lock and hide it when the cutscene ends
        if(returnToLockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}