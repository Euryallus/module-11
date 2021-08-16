using UnityEngine;

// ||=======================================================================||
// || RainEffect: Forces the rain particle effect used in the Flooded City  ||
// ||   area to move with the player, and plays a rain sound.               ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the production phase (Module 11).                                 ||
// ||=======================================================================||

public class RainEffect : MonoBehaviour
{
    private void Start()
    {
        // Play a looping rain storm sound effect
        AudioManager.Instance.PlayLoopingSoundEffect("rainStormLoop", "rainEffect");
    }

    private void LateUpdate()
    {
        if(PlayerInstance.ActivePlayer != null)
        {
            // Get the player GameObject
            GameObject playerGameObj = PlayerInstance.ActivePlayer.gameObject;

            if (playerGameObj != null)
            {
                // Move the rain effect to always be 10 units above the player, so they are always covered in rain
                //   Called in LateUpdate so the player's movement, which is set in Update, is matched without a 1 frame delay

                transform.position = new Vector3(playerGameObj.transform.position.x, playerGameObj.transform.position.y + 10.0f, playerGameObj.transform.position.z);
            }
        }
    }
}