using UnityEngine;

public class AbilityUnlockCutscene : CutsceneTriggerer
{
    [Header("Ability Unlock Cutscene")]

    [SerializeField] private GameObject abilityUnlockPanelPrefab;

    private CursorLockMode returnToLockState;

    private Item abilityItem;
    private PlayerAbilityType abilityType;
    private int upgradeLevel;

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

        AbilityUnlockPanel unlockPanel = Instantiate(abilityUnlockPanelPrefab, cinematicsCanvas.transform).GetComponent<AbilityUnlockPanel>();
        unlockPanel.transform.SetSiblingIndex(2);

        unlockPanel.Setup(abilityItem, abilityType, upgradeLevel);

        unlockPanel.ContinueButtonPressEvent += EndCutscene;

        returnToLockState = Cursor.lockState;

        // Show/unlock the cursor so the player can interact with the unlock panel
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    protected override void EndCutscene()
    {
        base.EndCutscene();

        // If the cursor was locked before the cutscene, re-lock and hide it on cutscene end
        if(returnToLockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}