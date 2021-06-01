using System.Collections.Generic;
using UnityEngine;

// ||=======================================================================||
// || LootTable: Defines how a collection of items of various types will    ||
// ||    be semi-randomly spawned in a loot chest                           ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

[CreateAssetMenu(fileName = "Loot Table", menuName = "Loot Table")]
public class LootTable : ScriptableObject
{
    // See tooltips below for comments describing member variables
    #region Properties

    public int                  MinItems    { get { return m_minItems; } }
    public int                  MaxItems    { get { return m_maxItems; } }
    public List<WeightedItem>   ItemPool    { get { return m_itemPool; } }

    #endregion

    #region InspectorVariables
    // Variables in this region are set in the inspector

    [Space]
    [Header("Loot Table")]
    [Space]
    [Header("Hover over variable names for tooltips with more info.")]

    [SerializeField] [Tooltip("The minumum number of items that can be spawned from this loot table")]
    private int                  m_minItems;

    [SerializeField] [Tooltip("The maximum number of items that can be spawned from this loot table")]
    private int                  m_maxItems;

    [Space]
    [SerializeField] [Tooltip("The items that can be spawned from this loot table, with weights that determine the likelihood of each item spawning")]
    private List<WeightedItem>   m_itemPool;

    #endregion

    public List<Item> GetWeightedItemPool()
    {
        // Returns a list of items, where items with a greater weight
        //   have more entries list

        List<Item> weightedItemPool = new List<Item>();

        // Loop through all items in the item pool
        for (int i = 0; i < m_itemPool.Count; i++)
        {
            // Add each item type with a count equal to its weight
            for (int j = 0; j < m_itemPool[i].Weight; j++)
            {
                weightedItemPool.Add(m_itemPool[i].Item);
            }
        }

        return weightedItemPool;
    }
}

// WeightedItem: An item with a weight determining how likely it is to be picked from a LootTable, as well as
//   a minimum quantity that forces it to be spawned in a loot chest at least a certain number of times
//===========================================================================================================

[System.Serializable]
public struct WeightedItem
{
    // See tooltips for comments

    [Tooltip("The item that can be added to the loot chest")]
    public Item Item;

    [Tooltip("The likelihood that this item will be added, e.g. if there are 2 items, one with weight 1, and one with weight 3," +
                "the first item will have a 1/4 chance of being added each time, and the second will have a 3/4 chance each time")]
    public int  Weight;

    [Tooltip("At least this many of the item type will be guaranteed to be spawned in the loot chest")]
    public int  MinimumQuantity;
}
