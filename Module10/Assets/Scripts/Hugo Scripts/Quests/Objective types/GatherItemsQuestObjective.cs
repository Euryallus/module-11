using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         A quest objective that flags when a player has collected [x] items in their inventory
// Development window:  Prototype phase
// Inherits from:       QuestObjective

[CreateAssetMenu(fileName = "Quest data", menuName = "Quests/Objectives/Gather Items objective", order = 2)]
[System.Serializable]
public class GatherItemsQuestObjective : QuestObjective
{
    public ItemGroup toCollect; // The item & quanitity of said item the player has to collect to complete the objective

    public override bool CheckCcompleted()
    {
        // Saves "type" for easier reference when using lists of different objectives
        objectiveType = Type.Collect;

        // References player's inventory
        InventoryPanel inventory = GameObject.FindGameObjectWithTag("Inventory").GetComponent<InventoryPanel>();

        // Returns if inventory contains item(s) listed in toCollect
        return (inventory.ItemContainer.CheckForQuantityOfItem(toCollect.Item) >= toCollect.Quantity);
    }
}
