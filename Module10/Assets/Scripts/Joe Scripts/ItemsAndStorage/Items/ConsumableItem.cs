using UnityEngine;

// ||=======================================================================||
// || ConsumableItem: An item that can be eaten by the player.              ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
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

    [SerializeField] [Tooltip("How much the player's food level will increase by when this item is eaten")]
    private float m_hungerIncrease;

    private void OnValidate()
    {
        if(CustomStringProperties.Length == 0)
        {
            CustomStringProperties = new CustomStringProperty[1];
            CustomStringProperties[0] = new CustomStringProperty();
        }

        CustomStringProperties[0].Name = "foodLevelIncrease";
        CustomStringProperties[0].Value = "Food Level +" + m_hungerIncrease;
    }
}