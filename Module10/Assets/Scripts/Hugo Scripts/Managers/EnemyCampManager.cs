using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

// Main author:         Hugo Bailey
// Additional author:   Joe Allen
// Description:         Enemy camp entity - spawns & manages enemies that patrol an area surrounding it
// Development window:  Prototype phase & production phase
// Inherits from:       MonoBehaviour

public class EnemyCampManager : MonoBehaviour
{
    [SerializeField]    private float spawnDistanceMax          = 10f;  // Maximum distance units will spawn from own position
    [SerializeField]    private int difficultyLevel             = 10;   // Difficulty level of camp (lower number = less / easier enemies spawn)

    [SerializeField]    private bool spawnOnStart = true;

    [SerializeField]    private List<EnemyBase> possibleUnits   = new List<EnemyBase>();    // List of enemies that could be spawned from the camp
    [SerializeField]    private List<EnemyBase> spawnedEnemies  = new List<EnemyBase>();    // List of refs to enemies that were spawned
                        private List<EnemyBase> unitsDifficulty = new List<EnemyBase>();    // List of enemies that could possibly spawn, that make up difficulty ~ that defined by diffifultyLevel

    public bool hasBeenDefeated = false;        // Flags if all spawned enemies have been defeated
    public bool spawnRandomPosition = true;     // Flags whether enemies spawn in random pos or only at camp position
    public int totalSpawned;                    // Stores total # of enemies that have been spawned


    public int remainingUnits // returns # of remaining units (not always just spawnedEnemies.Count so some more calculations needed)
    { get {
            int left = 0;

            if (spawnedEnemies.Count != 0)
            {
                // Cycles each spawned ref., if ref. is null OR enemy is flagged as !alive enemy is dead (don't incriment "left" int)
                for (int i = 0; i < spawnedEnemies.Count; i++)
                {
                    EnemyBase enemy = spawnedEnemies[i];
                    if(enemy == null)
                    {
                        spawnedEnemies.RemoveAt(i);
                        i--;
                    }
                    else if (enemy.gameObject.GetComponent<EnemyHealth>().alive)
                    {
                        left++;
                    }
                }
            }

            // Returns remaining enemy count
            return left; } 
    }

    // Added by Joe
    [SerializeField] private CombatDynamicAudioArea dynamicAudio;   // The dynamic audio area associated with this enemy camp
    [SerializeField] private UnityEvent noActiveEnemiesEvent = new UnityEvent();    // Functions that play when all enemies have been defeated    

    void Start()
    {
        totalSpawned = 0;
        if (spawnOnStart)
        { 
            // If spawnOnStart is flagged, immediately spawn units
            SpawnUnits(difficultyLevel, Vector3.zero);
        }
    }

    private void Update()
    {
        // Checks whether encounter has been defeated
        if(spawnedEnemies.Count != 0)
        {
            hasBeenDefeated = false;

            // Cycles each enemy, if any are still alive hasBeenDefeated = false
            for(int i = 0; i < spawnedEnemies.Count; i++)
            {
                if(spawnedEnemies[i] != null)
                {
                    if (!spawnedEnemies[i].gameObject.GetComponent<EnemyHealth>().alive)
                    {
                        // If any enemies are not alive OR the ref. is null, remove from list
                        spawnedEnemies.RemoveAt(i);
                        if (spawnedEnemies.Count == 0)
                        {
                            break;
                        }
                        else
                        {
                            // if enemy was removed & more remain decrement i by 1 to account for altered list
                            i--;
                        }
                    }
                }
            }
        }
        else if(!hasBeenDefeated)
        {
            // If no spawned enemies remain but hasBeenDefeated hasnt been flagged, flag & call "end" func.
            hasBeenDefeated = true;

            NoActiveEnemies();
        }
    }

    private void NoActiveEnemies()
    {
        // Added by Joe: Disable combat audio when all enemies are defeated
        if (dynamicAudio != null)
        {
            dynamicAudio.SetAreaEnabled(false);
        }

        noActiveEnemiesEvent.Invoke();
    }

    // Spawns units with a total difficulty equal to the param passed
    public void SpawnUnits(int difficultyLevel, Vector3 initialSearchPos)
    {        
        hasBeenDefeated = false;
        // Resets list of potential enemies
        unitsDifficulty = new List<EnemyBase>();

        // First unit spawned is a random unit from the list of possible units
        int i = Random.Range(0, possibleUnits.Count);
        // Starting difficulty gotten from said random unit
        int startPlace = possibleUnits[i].difficulty;

        // Adds this first unit into the list of those to spawn once process is over
        unitsDifficulty.Add(possibleUnits[i]);
        // Calls RemainingUnits() to calculate rest of the units to spawn
        RemainingUnits(startPlace);

        // Once all units are calculated, spawn each in
        foreach (EnemyBase prefab in unitsDifficulty)
        {
            Vector3 randomPosition = transform.position;
            if (spawnRandomPosition)
            {
                // Generate random position within [spawnDistanceMax] meters of position
                randomPosition = Random.insideUnitSphere * spawnDistanceMax;

                randomPosition += transform.position;
                randomPosition.y = transform.position.y;
            }

            // Instantiate Enemy from list, set position to random pos generated
            spawnedEnemies.Add(Instantiate(prefab, randomPosition, Quaternion.identity));
            // Create "created" ref to enemy just spawned
            EnemyBase created = spawnedEnemies[spawnedEnemies.Count - 1];
            // Assign manager & centralHubPos variables to new enemy
            created.centralHubPos = transform.position;
            created.manager = gameObject.GetComponent<EnemyCampManager>();

            if(initialSearchPos != Vector3.zero)
            {
                created.StartSearching(initialSearchPos);
            }
            else
            {
                created.StartPatrolling();
            }
        }

        totalSpawned = spawnedEnemies.Count;

        // Added by Joe: Enable combat audio enemy units are spawned
        if (dynamicAudio != null)
        {
            dynamicAudio.SetAreaEnabled(true);
        }
    }

    // Calculates remaining units to spawn based on total difficulty of units already selected
    private void RemainingUnits(int total)
    {
        // Exit condition - if total difficulty of encounter goes over "difficultyLevel" value
        if (total >= difficultyLevel)
        {
            return;
        }

        // Ints used to generate random unit & difficulty
        int n = 1;
        int i = 0;

        // If remaining difficulty is over 1, find random unit & add difficulty to total
        if (difficultyLevel - total > 1)
        {
            i = Random.Range(0, possibleUnits.Count);
            n = possibleUnits[i].difficulty;
        }

        // If remaining difficulty is 1, just chose first unit in possibleUnits list
        unitsDifficulty.Add(possibleUnits[i]);
        // Recursive call of RemainingUnits (once exit condition is hit, difficulty of units selected ~ difficultyLevel)
        RemainingUnits(total + n);
    }

    // Used to allow units to alert others of player
    public void AlertUnits(Vector3 position)
    {
        // Cycles each enemy in spawnedEnemies and tells them to go look at position passed
        foreach (EnemyBase enemy in spawnedEnemies)
        {
            enemy.AlertOfPosition(position);
        }
    }

    // Adds externally spawned enemy to list of spawned enemies (e.g. if Enemy#1 spawns 3 minions, adds minions to list)
    public void AddUnitToList(EnemyBase enemy)
    {
        spawnedEnemies.Add(enemy);
        totalSpawned += 1;
    }
}
