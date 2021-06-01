using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Adapted version of EnemyBase for Enemy type #1 - uses a melee attack & summons smaller clones when fighting
// Development window:  Prototype phase
// Inherits from:       EnemyBase

public class Enemy1 : EnemyBase
{
    [Header("Enemy 1 elements")]
    [SerializeField]    private int numberOfDuplicates;                                 // Number of clones enemy will attempt to spawn on first attack at player
    [SerializeField]    private GameObject duplicatePrefab;                             // Prefab enemies are spawned using
    [SerializeField]    private List<GameObject> children = new List<GameObject>();     // Array of children enemy has spawned (initialized to prevent access errors)
    [SerializeField]    private float duplicateSpawnDistance;                           // Distance clones can spawn from enemy
                        protected  bool HasSplit = false;                               // References whether or not enemy has spawned clones yet

    // Adapted Attack() function - Adds the "clone" ability
    public override void Attack()
    {
        if(!HasSplit)
        {
            // If the enemy tries attacking and hasn't yet cloned itself, TRY spawning the nunmber defined as "numberOfDuplicates"
            for (int i = 0; i < numberOfDuplicates; i++)
            {
                // Select a random position within [duplicateSpawnDistance] of the enemy as the spawn point of the new clone & adjust for current position
                Vector3 pos = Random.insideUnitSphere * duplicateSpawnDistance;
                pos += transform.position;

                // Set Y co-ord of this position to the same as enemies own (much more likely to align with NavMesh & avoid issues)
                pos.y = transform.position.y;

                // Adds new instance of the clone prefab to "children" list, setting position as point defined above
                children.Add(Instantiate(duplicatePrefab, pos, Quaternion.identity)); 

                // Assigns new child references to the EnemyCamp (the "manager") & the enemy's camp position
                children[children.Count - 1].GetComponent<EnemyBase>().manager = manager;
                children[children.Count - 1].GetComponent<EnemyBase>().centralHubPos = centralHubPos;

                // Updates bool to reflect that the enemy has cloned itself
                HasSplit = true;
                
            }
        }
        else
        {
            // If enemy has already split & tries attacking, run base melee attack
            base.Attack();
        }

    }
}
