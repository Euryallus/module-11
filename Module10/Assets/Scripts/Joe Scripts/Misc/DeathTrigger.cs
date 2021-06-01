using UnityEngine;

// ||=======================================================================||
// || DeathTrigger: A trigger to be placed below the world terrain that     ||
// ||   kills the player when entered.                                      ||
// ||=======================================================================||
// || Used on prefab: Joe/Environment/DeathTrigger                          ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public class DeathTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            // Player entered the trigger

            // Get a reference to the PlayerDeath script on the player
            PlayerDeath playerDeath = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerDeath>();

            // Kill the player, with a message saying they fell out of the world
            playerDeath.KillPlayer(PlayerDeathCause.FellOutOfWorld);
        }
    }
}