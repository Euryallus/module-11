using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         A quest objective that flags when a player has visited the co-ordinates listed
// Development window:  Prototype phase
// Inherits from:       QuestObjective

[CreateAssetMenu(fileName = "Quest data", menuName = "Quests/Objectives/Go To objective", order = 1)]
[System.Serializable]
public class GoToQuestObjective : QuestObjective
{
    [Tooltip("Position player has to be in / near to complete")]
    public Vector3 positionToGoTo;                                  // Stores position player must go to to complete objective

    [Tooltip("Distance from player to positionToGoTo to complete")]
    [SerializeField]    private float distanceFlagged = 2.5f;       // Min. distance player must be from positionToGoTo to complete objective

    public override bool CheckCcompleted()
    {
        // Saves "type" for easier reference when using lists of different objectives
        objectiveType = Type.GoTo;
        // Returns if player is within [distanceFlagged] meters of [positionToGoTo]
        return (Vector3.Distance(GameObject.FindGameObjectWithTag("Player").transform.position, positionToGoTo) < distanceFlagged);
    }
}
