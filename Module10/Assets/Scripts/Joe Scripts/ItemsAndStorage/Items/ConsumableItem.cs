using UnityEngine;

// ||=======================================================================||
// || ConsumableItem: An item that can be eaten by the player.              ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || originally for the prototype phase.                                   ||
// ||                                                                       ||
// || Changes made during the production phase (Module 11):                 ||
// ||                                                                       ||
// || - Added code in OnValidate to generate a foodLevelIncrease property   ||
// ||=======================================================================||

[CreateAssetMenu(fileName = "Consumable Item", menuName = "Item/Consumable Item")]
public class ConsumableItem : Item
{
    // See tooltips on each member variable for comments describing their purpose

    #region Properties

    public float HungerIncrease { get { return m_hungerIncrease; } }

    #endregion

    [Space]
    [Header("Consumable Item Options")]

    [SerializeField] [Tooltip("How much the player's food level will increase by when this item is eaten")] // <--
    private float m_hungerIncrease;

    private void OnValidate()
    {
        // Automatically creates a 'foodLevelIncrease' property, which is used to ensure the item's
        //   hunger increase value is displayed in any UI that shows item properties (e.g. item info popup)

        // Add an empty property if no properties currently exist for the item
        if (CustomStringProperties.Length == 0)
        {
            CustomStringProperties    = new CustomStringProperty[1];
            CustomStringProperties[0] = new CustomStringProperty();
        }

        // Give the property the name 'foodLevelIncrease' since it will display the value
        //  for how much food level increases when the consumable item is eaten
        CustomStringProperties[0].Name = "foodLevelIncrease";

        // The text that will be displayed for this property, for example if m_hungerIncrease is 0.4, would be: 'Food Level: +0.4'
        CustomStringProperties[0].Value = "Food Level +" + m_hungerIncrease;
    }
}