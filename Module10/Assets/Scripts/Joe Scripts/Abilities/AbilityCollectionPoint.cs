using UnityEngine;

// ||=======================================================================||
// || AbilityCollectionPoint: An object that the player can interact with   ||
// ||   to acquire an ability.                                              ||
// ||=======================================================================||
// || Used on prefab: Joe/Environment/AbilityCollectionPoint                ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the production phase (Module 11).                                 ||
// ||=======================================================================||

public class AbilityCollectionPoint : InteractableWithOutline, IPersistentSceneObject
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [Header("Ability Collection Point")]

    [SerializeField] private Item               abilityItem;    // The ability item the player will be given when they interact with this object
    [SerializeField] private PlayerAbilityType  abilityType;    // The type of ability that is being given (determines the look of the object)

    [Header("Do Not Edit")]

    [SerializeField] [ColorUsage(true, true)]
    private Color[]         glowColours;                // Array of colours to be used for the glow effect, one for each ability type

    [SerializeField]
    private Material        abilityGlowMaterial;        // The material used to give the glow effect

    [SerializeField]
    private MeshRenderer    abilityGlowMeshRenderer;    // The renderer that will use the glow material

    [SerializeField]
    private Sprite[]        abilityIconSprites;         // Array of sprites used to display the ability icon, one for each ability type

    [SerializeField]
    private SpriteRenderer  iconSpriteRenderer;         // The renderer that will show one of the abilityIconSprites depending on the set abilityType

    #endregion

    private bool abilityCollected;  // Whether or not an ability has been collected from this object

    protected override void Start()
    {
        base.Start();

        // Subscribe to save/load events so the container's data will be saved/loaded with the game
        SaveLoadManager.Instance.SubscribeSceneSaveLoadEvents(OnSceneSave, OnSceneLoadSetup, OnSceneLoadConfigure);

        // Setup sprites/materials based on abilityType
        SetupVisuals();

        // By default the ability is not collected
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
            // If the ability was not yet collected, add the item to the player's inventory and mark it as collected

            GameSceneUI.Instance.PlayerInventory.TryAddItem(abilityItem);
            
            SetAbilityCollected(true);
        }
    }

    public void OnSceneSave(SaveData saveData)
    {
        // Save whether the ability has been collected

        saveData.AddData(GetUniquePositionId() + "_collected", abilityCollected);
    }

    public void OnSceneLoadSetup(SaveData saveData)
    {
        // Load whether the ability has been collected

        SetAbilityCollected(saveData.GetData<bool>(GetUniquePositionId() + "_collected"));
    }

    public void OnSceneLoadConfigure(SaveData saveData) { } // Nothing to configure

    private void SetAbilityCollected(bool collected)
    {
        abilityCollected = collected;

        // Show/hide the icon depending on if the ability was collected
        iconSpriteRenderer.gameObject.SetActive(!collected);

        // Disallow interactions if the ability was collected
        canInteract = !collected;
    }

    private void SetupVisuals()
    {
        // Create a new instance of the glow material to be used on this object
        Material glowMaterial = new Material(abilityGlowMaterial);

        // Set the icon being displayed based on the ability type that can be collected form this point
        iconSpriteRenderer.sprite = abilityIconSprites[(int)abilityType];

        // Set the glow material's bottom colour based on the ability type
        glowMaterial.SetColor("_BottomColour", glowColours[(int)abilityType]);

        // Apply the material to the renderer
        abilityGlowMeshRenderer.material = glowMaterial;

        // Set the tooltip name text so the ability's UI name will be shown on hover
        tooltipNameText = abilityItem.UIName;

        // Try getting the ability item's upgrade level
        CustomFloatProperty itemUpgradeProperty = abilityItem.GetCustomFloatPropertyWithName("upgradeLevel", true);

        if (itemUpgradeProperty != null && itemUpgradeProperty.Value > 1)
        {
            // The ability item is upgraded (i.e. has an upgrade level greater than 1),
            //   append 'Upgrade' to the tooltip text so the player will know this
            tooltipNameText += " Upgrade";
        }
    }

    private string GetUniquePositionId()
    {
        // Returns a unique id for this object based on its position
        return "abilityCollection_" + transform.position.x + "_" + transform.position.y + "_" + transform.position.z;
    }
}
