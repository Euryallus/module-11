using System.Collections.Generic;
using UnityEngine;

// ||=======================================================================||
// || CraftingRecipe: Defines the items required to craft another item      ||
// ||   using a crafting table.                                             ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

[CreateAssetMenu(fileName = "Crafting Recipe", menuName = "Crafting Recipe")]
public class CraftingRecipe : ScriptableObject
{
    #region Properties
    // See tooltips below for info

    public List<ItemGroup> RecipeItems { get { return m_recipeItems; } }
    public ItemGroup       ResultItem  { get { return m_resultItem; } }

    #endregion

    [Space]
    [Header("Recipe Setup")]

    [Space]
    [Header("Hover over variable names for tooltips with more info.")]

    [SerializeField] [Tooltip("Items required to craft the result item")]
    private List<ItemGroup> m_recipeItems;

    [SerializeField] [Tooltip("Items required to craft the result item")]
    private ItemGroup       m_resultItem;
}