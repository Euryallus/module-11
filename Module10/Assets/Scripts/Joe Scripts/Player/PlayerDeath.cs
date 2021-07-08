using System.Collections.Generic;
using UnityEngine;

// PlayerDeathCause: All of the possible death reasons that can be shown
public enum PlayerDeathCause
{
    Starved,
    Drowned,
    Enemy,
    EnemyProjectile,
    FellOutOfWorld,
    Crushed,       
    Skewered,
    SwingHit,
    WaveHit,
    Fire
}

// ||=======================================================================||
// || PlayerDeath: Handles 'killing' the player; showing a death message    ||
// ||   and reloading the game ready to restore the last save point.        ||
// ||=======================================================================||
// || Used on prefab: Player                                                ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public class PlayerDeath : MonoBehaviour
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private GameObject deathPanelPrefab;

    #endregion

    // Dictionary containing all possible messages that can be shown for each PlayerDeathCause
    //   Each death cause has an array of messages that can be chosen from, each with a different weight/likelihood of being displayed

    private readonly Dictionary<PlayerDeathCause, WeightedString[]> deathCauseTextDict = new Dictionary<PlayerDeathCause, WeightedString[]>()
    {
        { PlayerDeathCause.Starved,         new WeightedString[]    {
                                                                        new WeightedString("You starved.", 100),
                                                                        new WeightedString("Maybe eat next time.", 1)
                                                                    } },

        { PlayerDeathCause.Drowned,         new WeightedString[]    {
                                                                        new WeightedString("You drowned.", 100),
                                                                        new WeightedString("HOLDING MY BREATH CHALLENGE (GONE WRONG) (NOT CLICKBAIT)", 1)
                                                                    } },

         { PlayerDeathCause.Enemy,          new WeightedString[]    {
                                                                        new WeightedString("You were killed by an enemy.", 100),
                                                                        new WeightedString("That was embarrassing.", 1)
                                                                    } },

        { PlayerDeathCause.EnemyProjectile, new WeightedString[]    {
                                                                        new WeightedString("You were killed by an enemy projectile.", 100),
                                                                        new WeightedString("You should try moving out of the way next time.", 1)
                                                                    } },

        { PlayerDeathCause.FellOutOfWorld,  new WeightedString[]    {
                                                                        new WeightedString("You fell out of the world.", 100),
                                                                        new WeightedString("You fell into the great void.", 1)
                                                                    } },

        { PlayerDeathCause.Crushed,         new WeightedString[]    {
                                                                        new WeightedString("You were crushed.", 100),
                                                                        new WeightedString("You were flattened.", 20),
                                                                        new WeightedString("You were squished.", 20),
                                                                        new WeightedString("You were turned into a pancake.", 5),
                                                                        new WeightedString("LOL SQUASHING DEATH", 1),
                                                                    } },

        { PlayerDeathCause.Skewered,        new WeightedString[]    {
                                                                        new WeightedString("You were skewered by spikes.", 100),
                                                                        new WeightedString("Protip: Don't stand on sharp pointy objects.", 1)
                                                                    } },

        { PlayerDeathCause.SwingHit,        new WeightedString[]    {
                                                                        new WeightedString("You were hit by a swinging object.", 100),
                                                                        new WeightedString("Swoosh swoosh", 1)
                                                                    } },

        { PlayerDeathCause.WaveHit,         new WeightedString[]    {
                                                                        new WeightedString("You were hit by a wave.", 100),
                                                                        new WeightedString("You were defeated by a liquid.", 1)
                                                                    } },

        { PlayerDeathCause.Fire,            new WeightedString[]    {
                                                                        new WeightedString("You were killed by fire.", 100),
                                                                        new WeightedString("You were roasted on an open flame.", 1),
                                                                        new WeightedString("Why would you just step into fire?", 1)
                                                                    } }
    };

    public void KillPlayer(PlayerDeathCause causeOfDeath)
    {
        Debug.Log("Player died! Cause of death: " + causeOfDeath);

        GameSceneUI gameUI = GameSceneUI.Instance;

        if(gameUI.ShowingCinematicsCanvas)
        {
            gameUI.HideCinematicsCanvas();
            gameUI.SetUIShowing(true);
        }

        // Stop player movement to prevent them moving while dead
        GetComponent<PlayerMovement>().StopMoving();

        // Show the cursor and unlock its movement so the player can interact with the death UI panel
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Pause time to prevent further animation/movement of other entities
        Time.timeScale = 0.0f;

        string deathCauseText = "";

        if (deathCauseTextDict.ContainsKey(causeOfDeath))
        {
            // Pick a random weighted death message depending on the cause of death
            deathCauseText = PickRandomWeightedString(deathCauseTextDict[causeOfDeath]);
        }
        else
        {
            Debug.LogError("No dictionary entry for death cause: " + causeOfDeath);
        }

        // Instantiate/show the death panel, making it a child of a canvas
        Transform canvasTransform = GameObject.FindGameObjectWithTag("JoeCanvas").transform;
        DeathPanel deathPanel = Instantiate(deathPanelPrefab, canvasTransform).GetComponent<DeathPanel>();

        // Set the death showing cause of death on the panel to the chosen string
        deathPanel.SetDeathCauseText(deathCauseText);

        // Play a sound when the death panel is first shown
        AudioManager.Instance.PlaySoundEffect2D("playerDeath");

        // Stop any looping sounds so they don't continue to play after respawning from death
        AudioManager.Instance.StopAllLoopingSoundEffects();
    }

    public string PickRandomWeightedString(WeightedString[] weightedStrings)
    {
        List<string> stringsPool = new List<string>();

        // Loop through all possible weighted strings to choose from
        for (int i = 0; i < weightedStrings.Length; i++)
        {
            // Add the text from each weighted string to the strings pool, messages with
            //  a higher weight will be added more times to increase their likelihood of being chosen
            for (int j = 0; j < weightedStrings[i].Weight; j++)
            {
                stringsPool.Add(weightedStrings[i].Text);
            }
        }

        // Pick a random string from the pool
        return stringsPool[Random.Range(0, stringsPool.Count)];
    }
}

// WeightedString: An string with a weight that can be used to determine its likelihood of being picked from a list
//=================================================================================================================
public struct WeightedString
{
    public WeightedString(string text, int weight)
    {
        Text    = text;
        Weight  = weight;
    }

    public string   Text;
    public int      Weight;
}