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
    [SerializeField] private PlayerDeathCause deathCause = PlayerDeathCause.FellOutOfWorld; // Cause of death shown when the trigger is entered

    [SerializeField] private SoundClass secondaryDeathSound; // Sound effect (if any) to be played on top of the default death sound when the trigger is entered

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            // Player entered the trigger

            // Get a reference to the PlayerDeath script on the player
            PlayerDeath playerDeath = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerDeath>();

            if(secondaryDeathSound != null)
            {
                AudioManager.Instance.PlaySoundEffect2D(secondaryDeathSound, true);
            }

            // Kill the player, with a message saying they fell out of the world
            playerDeath.KillPlayer(deathCause);
        }
        else if(other.CompareTag("MovableObj"))
        {
            // A movable object entered the trigger, move it back to its starting position

            if (secondaryDeathSound != null)
            {
                AudioManager.Instance.PlaySoundEffect3D(secondaryDeathSound, other.gameObject.transform.position);
            }

            other.GetComponent<MovableObject>().MoveToStartPosition();
        }
    }
}