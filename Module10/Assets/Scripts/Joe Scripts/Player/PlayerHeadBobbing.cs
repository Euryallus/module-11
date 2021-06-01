using UnityEngine;

// ||=======================================================================||
// || PlayerHeadBobbing: Applies a subtle up/down bobbing effect to the     ||
// ||   player's camera that is adjusted depending on their movement state. ||
// ||=======================================================================||
// || Used on prefab: Player                                                ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

[RequireComponent(typeof(PlayerMovement))]
public class PlayerHeadBobbing : MonoBehaviour
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private float          bobSpeedMultiplier = 2.5f;    // Multiplier used when calculating bob speed, which is based on player movement speed
    [SerializeField] private float          bobBaseIntensity   = 0.05f;   // The default intensity (up/down movement amount) of the effect
    [SerializeField] private float          waterBobSpeed      = 3.25f;   // The speed of the bob effect when idle in water
    [SerializeField] private float          waterBobIntensity  = 0.025f;  // The intensity (up/down movement amount) of the effect when idle in water
                                                                           
    [SerializeField] private Transform      cameraParentTransform;        // The parent object of the main player camera
    [SerializeField] private PlayerMovement playerMovement;               // The script attached to the player that handles movement

    #endregion

    private float time; // Keeps track of time (seconds), used for time-dependent calculations

    void Update()
    {
        float targetYPos; //The camera parent's y position will be set to this value

        if(DoBobbingEffect(out float yBobbingPos))
        {
            //Bobbing effect should be applied, set target to the calculated y bobbing position (see DoBobbingEffect)
            targetYPos = yBobbingPos;
        }
        else
        {
            // Return the target y position back to 0 (default)
            targetYPos = 0.0f;
        }

        // Lerp the camera parent transform's position towards the calculated y value
        cameraParentTransform.localPosition = Vector3.Lerp(cameraParentTransform.localPosition, new Vector3(0.0f, targetYPos, 0.0f), Time.deltaTime * 20.0f);
    }

    private bool DoBobbingEffect(out float yBobbingPos)
    {
        yBobbingPos = 0.0f; // Default y position for if no effect is applied

        if (SaveLoadManager.Instance.GetBoolFromPlayerPrefs("viewBobbing") == false)
        {
            // The player has disabled view bobbing in the options, don't apply any effect
            return false;
        }

        float playerSpeed = Vector3.Magnitude(playerMovement.GetVelocity());                        //Speed of player movement used to calculate bob effect speed
        bool diving = (playerMovement.currentMovementState == PlayerMovement.MovementStates.dive);  // Check if the player is diving 

        float bobSpeed = playerSpeed * bobSpeedMultiplier; // Multiply player speed by the set multiplier to get bob speed
        float bobIntensity  = bobBaseIntensity;            // Use the base bob intensity by default

        bool doBobbing      = true;                        // Apply the bobbing effect by default

        if (playerMovement.PlayerIsMoving())
        {
            // The player is moving, i.e. not stood still, so a head bobbing effect will be applied by default

            if (diving || !playerMovement.PlayerIsGrounded())
            {
                // Player is diving or not stood on the ground, disable bobbing
                doBobbing = false;
            }
        }
        else
        {
            // Player is stood still

            if (diving)
            {
                // Still in water, slowly bob the camera with a lower speed/intensity
                bobSpeed     = waterBobSpeed;
                bobIntensity = waterBobIntensity;
            }
            else
            {
                // Still on land, disable bobbing
                doBobbing = false;
            }
        }

        if(doBobbing)
        {
            // Calculate the y position of the camera to apply the bobbing effect (used if doBobbing = true)
            yBobbingPos = GetTargetYBobbingPos(bobSpeed, bobIntensity);
        }
        else
        {
            // Reset the timer ready for the next time the bobbing effect is applied
            time = 0.0f;
        }

        return doBobbing;
    }

    private float GetTargetYBobbingPos(float bobSpeed, float intensity)
    {
        // Set the target y position using a sin function based on the bob intensity to give a smooth up/down movement over time
        float yPos = (Mathf.Sin(time) * intensity);

        // Increment the timer used for sin calculation
        time += (Time.deltaTime * bobSpeed);

        return yPos;
    }
}