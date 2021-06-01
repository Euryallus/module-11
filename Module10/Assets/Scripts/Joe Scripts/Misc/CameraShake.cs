using UnityEngine;

public enum CameraShakeType
{
    StopAfterTime,  // The camera will shake with a constant intensity for a certain amount of time and then stop
    ReduceOverTime  // The camera will shake for a certain amount of time, but the shake intensity will linearly decrease to 0 over the duration of the effect
}

// ||=======================================================================||
// || CameraShake: Allows a shake effect to be applied to a camera.         ||
// ||=======================================================================||
// || Used on prefab: Player                                                ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the prototype phase.                                              ||
// ||=======================================================================||

public class CameraShake : MonoBehaviour
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [SerializeField] private Transform targetCameraTransform;   //The transform for the GameObject containing the camera to shake

    #endregion

    private bool            shaking;                // Whether or not the shake effect is currently being applied
    private float           shakeIntensity;         // How much the camera is moved during the shake effect
    private float           startShakeIntensity;    // The initial shake intensity when the effect is triggered
    private float           shakeFrequency;         // How frequently a random shake position is chosen (seconds)
    private float           shakeSpeed;             // How quickly the camera moves between random shake positions
    private CameraShakeType shakeType;              // The shake type for the effect - see enum above
    private float           totalShakeTime;         // How long the shake effect will last for (seconds)
                                                       
    private float           shakeTimer;             // Keeps track of how long the shake effect has been going for (seconds)
    private float           frequencyTimer;         // Keeps track of how many seconds have passed since a random camera position was chosen
                                                       
    private Vector3         basePosition;           // The local position of the target camera before any effects are applied
    private Vector3         targetOffset;           // The offset added to the camera's base position to create the effect


    private const float DefaultShakeIntensity   = 0.05f;    // Intensity of the shake effect if no override parameter is given
    private const float DefaultShakeFrequency   = 0.03f;    // Frequency of the shake effect if no override parameter is given
    private const float DefaultShakeSpeed       = 0.3f;     // Speed of the shake effect if no override parameter is given

    private void Start()
    {
        basePosition = targetCameraTransform.localPosition;
    }

    void Update()
    {
        if(shaking)
        {
            // Shake effect is being applied

            // Increment timers
            shakeTimer      += Time.deltaTime;
            frequencyTimer  += Time.deltaTime;

            // Move the target camera's position towards the offset 'shake' position
            targetCameraTransform.localPosition = Vector3.Lerp(targetCameraTransform.localPosition, basePosition + targetOffset, shakeSpeed);

            // Check if the intensity should be reduced over time
            if(shakeType == CameraShakeType.ReduceOverTime)
            {
                // Move the intensity from its starting value to 0 over the duration of the shake effect
                shakeIntensity = Mathf.Lerp(startShakeIntensity, 0.0f, shakeTimer / totalShakeTime);
            }

            if(shakeTimer > totalShakeTime)
            {
                // The effect is over - restore the camera to its original position
                targetCameraTransform.localPosition = basePosition;
                shaking = false;
            }
            else
            {
                if (frequencyTimer > shakeFrequency)
                {
                    // The frequency interval was reached - chose a new random position for the camera to move towards
                    targetOffset = new Vector3(Random.Range(-shakeIntensity, shakeIntensity), Random.Range(-shakeIntensity, shakeIntensity), Random.Range(-shakeIntensity, shakeIntensity));

                    frequencyTimer = 0.0f;
                }
            }
        }
    }

    public void ShakeCameraForTime(float time, CameraShakeType type,
                                    float intensity = DefaultShakeIntensity, float frequency = DefaultShakeFrequency, float speed = DefaultShakeSpeed)
    {
        if(SaveLoadManager.Instance.GetBoolFromPlayerPrefs("screenShake") == false)
        {
            // Screen/camera shake was disabled in the optiops menu, don't apply the effect
            return;
        }

        // Setup variables
        totalShakeTime      = time;
        shakeIntensity      = intensity;
        startShakeIntensity = intensity;
        shakeFrequency      = frequency;
        shakeSpeed          = speed;

        shakeType           = type;

        shakeTimer          = 0.0f;
        frequencyTimer      = 0.0f;

        // Start the shake effect
        shaking = true;
    }
}