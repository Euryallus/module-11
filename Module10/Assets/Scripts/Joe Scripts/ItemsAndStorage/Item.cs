using UnityEngine;

// ||=======================================================================||
// || Item: Something that can be stored in the player's inventory, hotbar, ||
// ||   a chest, or any item container. Can attach a HeldItemGameObject     ||
// ||   to add behaviour when the player is holding the item.               ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

[CreateAssetMenu(fileName = "Item", menuName = "Item/Item")]
public class Item : ScriptableObject
{
    // See tooltips below for comments describing each member variable
    #region Properties

    public string                       Id { get { return m_id; } set { m_id = value; } }
    public string                       UIName { get { return m_uiName; } set { m_uiName = value; } }
    public string                       UIDescription { get { return m_uiDescription; } }
    public int                          StackSize { get { return m_stackSize; } }
    public float                        Weight { get { return m_weight; } }
    public Sprite                       Sprite { get { return m_sprite; } }
    public GameObject                   HeldItemGameObject { get { return m_heldItemGameObject; } }
    public bool                         Customisable { get { return m_customisable; } }
    public string                       CurrencyItemId { get { return m_currencyItemId; } }
    public int                          CurrencyItemQuantity { get { return m_currencyItemQuantity; } }
    public bool                         CustomItem { get { return m_customItem; } set { m_customItem = value; } }
    public string                       BaseItemId { get { return m_baseItemId; } set { m_baseItemId = value; } }
    public CustomFloatProperty[]        CustomFloatProperties { get { return m_customFloatProperties; } set { m_customFloatProperties = value; } }
    public CustomStringProperty[]       CustomStringProperties { get { return m_customStringProperties; } set { m_customStringProperties = value; } }

    #endregion

    #region InspectorVariables
    // Variables in this region are set in the inspector

    [Space]
    [Header("Info")]
    [Space]
    [Header("Hover over variable names for tooltips with more info.")]

    [SerializeField] [Tooltip("Unique identifier for this item")]
    private string m_id;

    [SerializeField] [Tooltip("Name to be displayed in the user interface")]
    private string m_uiName;

    [SerializeField] [Tooltip("Short description to be displayed in the user interface. Can be left blank")]
    private string m_uiDescription;

    [SerializeField] [Tooltip("Maximum number of this item that can be stored in a single stack")]
    private int m_stackSize = 1;

    [SerializeField] [Tooltip("The weight of this item - more desirable items should have a greater weight")]
    private float m_weight;

    [SerializeField] [Tooltip("Sprite to be displayed in the UI for this item")]
    private Sprite m_sprite;

    [SerializeField] [Tooltip("The GameObject to be instantiated when the player holds this item, leave blank if no held item should be shown")]
    private GameObject m_heldItemGameObject;

    [Space]
    [Header("Player Customisation")]

    [SerializeField] [Tooltip("Whether or not the player can customise this item type's name/properties")]
    private bool m_customisable;

    [SerializeField] [Tooltip("The id of the item needed to customise this item type. Leave blank if this item is not customisable, or does not require another item to be customised.")]
    private string m_currencyItemId;

    [SerializeField] [Tooltip("The number of the above items required to customise this item type. Leave at 0 if this item is not customisable, or does not require another item to be customised.")]
    private int m_currencyItemQuantity;

    [Space]
    [SerializeField] [Tooltip("Properties with float values that can be customised by the player.")]
    private CustomFloatProperty[] m_customFloatProperties;

    [SerializeField] [Tooltip("Properties with string values that can be customised by the player.")]
    private CustomStringProperty[] m_customStringProperties;

    #endregion

    // Fields that are not editable in the inspector:

    private bool    m_customItem;   // Whether the item is a custom item made by the player
    private string  m_baseItemId;   // If the item is custom, the id of the original item it is based on

    public CustomFloatProperty GetCustomFloatPropertyWithName(string propertyName)
    {
        // Returns a custom float property with a matching propertyName

        // Loop through all custom float properties
        for (int i = 0; i < m_customFloatProperties.Length; i++)
        {
            // Find one with a matching name
            if(m_customFloatProperties[i].Name == propertyName)
            {
                // Return the matching property
                return m_customFloatProperties[i];
            }
        }

        // No matching properties found
        Debug.LogError("Trying to get invalid custom float property: " + propertyName);
        return default;
    }

    public void SetCustomFloatProperty(string propertyName, float value)
    {
        // Sets the value of a custom float property with a matching propertyName

        // Loop through all custom float properties
        for (int i = 0; i < m_customFloatProperties.Length; i++)
        {
            // Find one with a matching name
            if (m_customFloatProperties[i].Name == propertyName)
            {
                // Set its value to the given value
                m_customFloatProperties[i].Value = value;
                return;
            }
        }

        // No matching properties found
        Debug.LogError("Trying to set invalid custom float property: " + propertyName);
    }

    public CustomStringProperty GetCustomStringPropertyWithName(string propertyName)
    {
        // Returns a custom string property with a matching propertyName

        // Loop through all custom string properties
        for (int i = 0; i < m_customStringProperties.Length; i++)
        {
            // Find one with a matching name
            if (m_customStringProperties[i].Name == propertyName)
            {
                // Return the matching property
                return m_customStringProperties[i];
            }
        }

        // No matching properties found
        Debug.LogError("Trying to get invalid custom string property: " + propertyName);
        return default;
    }

    public void SetCustomStringProperty(string propertyName, string value)
    {
        // Sets the value of a custom string property with a matching propertyName

        // Loop through all custom string properties
        for (int i = 0; i < m_customStringProperties.Length; i++)
        {
            // Find one with a matching name
            if (m_customStringProperties[i].Name == propertyName)
            {
                // Set its value to the given value
                m_customStringProperties[i].Value = value;
                return;
            }
        }

        // No matching properties found
        Debug.LogError("Trying to set invalid custom string property: " + propertyName);
    }
}

// CustomFloatProperty: A property with a float value that can be adjusted by the player in a customisation table
//===============================================================================================================

[System.Serializable]
public class CustomFloatProperty
{
    // See tooltips for comments

    [Tooltip("The name used to get/set this property in code")]
    public string   Name;

    [Tooltip("The name that will be displayed for this property in the user interface")]
    public string   UIName;

    [Tooltip("The default value of this property")]
    public float    Value;

    [Tooltip("How much the value changed by each time the +/- button is pressed when customising")]
    public float    UpgradeIncrease;

    [Tooltip("The Value cannot go below this no matter how many times the item is customised")]
    public float    MinValue;

    [Tooltip("The Value cannot surpass this no matter how many times the item is customised")]
    public float    MaxValue;
}

// CustomStringProperty: A property with a string value that can be adjusted by the player in a customisation table
//=================================================================================================================

[System.Serializable]
public class CustomStringProperty
{
    // See tooltips for comments

    [Tooltip("The name used to get/set this property in code")]
    public string Name;

    [Tooltip("The name that will be displayed for this property in the user interface")]
    public string UIName;

    [Tooltip("The default value of this property")]
    public string Value;
}