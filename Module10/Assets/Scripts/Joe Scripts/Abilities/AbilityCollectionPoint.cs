using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityCollectionPoint : InteractableWithOutline, IPersistentSceneObject
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [Header("Ability Collection Point")]

    [SerializeField] private Item               abilityItem;
    [SerializeField] private PlayerAbilityType  abilityType;

    [Header("Do Not Edit")]
    [SerializeField] [ColorUsage(true, true)]
    private Color[]         glowColours;

    [SerializeField]
    private Material        abilityGlowMaterial;

    [SerializeField]
    private MeshRenderer    abilityGlowMeshRenderer;

    [SerializeField]
    private Sprite[]        abilityIconSprites;

    [SerializeField]
    private SpriteRenderer  iconSpriteRenderer;

    #endregion

    private bool abilityCollected;

    protected override void Start()
    {
        base.Start();

        // Subscribe to save/load events so the container's data will be saved/loaded with the game
        SaveLoadManager.Instance.SubscribeSceneSaveLoadEvents(OnSceneSave, OnSceneLoadSetup, OnSceneLoadConfigure);

        SetupVisuals();

        SetAbilityCollected(false);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        // Unsubscribe from save/load events if the GameObject is destroyed to prevent null reference errors
        SaveLoadManager.Instance.UnsubscribeSceneSaveLoadEvents(OnSceneSave, OnSceneLoadSetup, OnSceneLoadConfigure);
    }

    public override void Interact()
    {
        base.Interact();

        if(!abilityCollected)
        {
            GameSceneUI.Instance.PlayerInventory.TryAddItem(abilityItem);
            
            SetAbilityCollected(true);
        }
    }

    public void OnSceneSave(SaveData saveData)
    {
        saveData.AddData(GetUniquePositionId() + "_collected", abilityCollected);
    }

    public void OnSceneLoadSetup(SaveData saveData)
    {
        SetAbilityCollected(saveData.GetData<bool>(GetUniquePositionId() + "_collected"));
    }

    public void OnSceneLoadConfigure(SaveData saveData) { } // Nothing to configure

    private void SetAbilityCollected(bool collected)
    {
        abilityCollected = collected;

        iconSpriteRenderer.gameObject.SetActive(!collected);

        canInteract = !collected;
    }

    private void SetupVisuals()
    {
        Material glowMaterial = new Material(abilityGlowMaterial);

        // Set the icon being displayed based on the ability type that can be collected form this point
        iconSpriteRenderer.sprite = abilityIconSprites[(int)abilityType];

        // Set the glow material's bottom colour based on the ability type
        glowMaterial.SetColor("_BottomColour", glowColours[(int)abilityType]);

        abilityGlowMeshRenderer.material = glowMaterial;

        tooltipNameText = abilityItem.UIName;

        CustomFloatProperty itemUpgradeProperty = abilityItem.GetCustomFloatPropertyWithName("upgradeLevel", true);

        if (itemUpgradeProperty != null && itemUpgradeProperty.Value > 1)
        {
            tooltipNameText += " Upgrade";
        }
    }

    private string GetUniquePositionId()
    {
        return "abilityCollection_" + transform.position.x + "_" + transform.position.y + "_" + transform.position.z;
    }
}
