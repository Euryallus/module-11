using System.Collections;
using UnityEngine;

// ||=======================================================================||
// || Hazard: An obstacle with a triggerable animation that can kill        ||
// ||   the player.                                                         ||
// ||=======================================================================||
// || Used on all prefabs in: Joe/Environment/Hazards                       ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || originally for the prototype phase.                                   ||
// ||                                                                       ||
// || Changes made during the production phase (Module 11):                 ||
// ||                                                                       ||
// || - Allowed animations to be reversed for use in QTE hazards.           ||
// || - Added an optional start delay for continuous animations.            ||
// ||=======================================================================||

public enum HazardMode
{
    PlayerTrigger,  // The hazard will animate when the player enters a certain trigger area
    Continuous      // The hazard will animate continuously at a set interval
}

public class Hazard : MonoBehaviour, IExternalTriggerListener
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    [Header("Hazard")]
    [SerializeField] private Animator           animator;                                           // The animator used to trigger animations
    [SerializeField] private ParticleGroup      impactParticles;                                    // Particles to spawn when ImpactEvents is called
    [SerializeField] private SoundClass         impactSound;                                        // Sound to play when ImpactEvents is called
    [SerializeField] private PlayerDeathCause   deathCause              = PlayerDeathCause.Crushed; // Cause of death to show when the player is killed by the obstacle
    [SerializeField] private float              cameraShakeMultiplier   = 1.0f;                     // How intense the camera shake will be on impact (1.0 = default)
    [SerializeField] private HazardMode         mode                    = HazardMode.PlayerTrigger; // How the hazard animation is triggered

    [SerializeField] [Range(2.0f, 120.0f)]
    private float                               continuousInverval      = 2.5f;  // When using HazardMode.Continuous, how frequently the hazard animation will play
    
    [SerializeField] [Range(0.0f, 10.0f)]
    private float                               continuousStartDelay    = 0.0f;  // When using HazardMode.Continuous, the delay before the initial animation trigger

    [Header("Triggers")]
    [SerializeField] private ExternalTrigger[]  hitTriggers;    // All triggers that will kill the player if entered
    [SerializeField] private ExternalTrigger    areaTrigger;    // Trigger that activates the hazard when using HazardMode.PlayerTrigger

    #endregion

    private bool playingAnimation   = false;    // Whether the hazard animation is being played
    private bool reversingAnimation = false;    // Whether the animation is being reversed

    private void Awake()
    {
        // The hazard is a listener for all external triggers so certain
        //   funcitonality can be triggered when they are entered/exited
        for (int i = 0; i < hitTriggers.Length; i++)
        {
            hitTriggers[i].AddListener(this);
        }
        areaTrigger.AddListener(this);
    }

    private void Start()
    {
        if (mode == HazardMode.Continuous)
        {
            // Start triggering the hazard animation at a continuous interval
            StartCoroutine(ContinuousTriggerCoroutine());
        }
    }

    private void Update()
    {
        if(reversingAnimation && animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 0.0f)
        {
            // If the ainmation is being reversed and it reaches the start, stop playing the animation

            reversingAnimation  = false;
            playingAnimation    = false;

            // Tell the animator that animations should no longer be played in reverse
            animator.SetTrigger("DoneReversing");
        }
    }

    public void OnExternalTriggerEnter(string triggerId, Collider other)
    {
        if(other.CompareTag("Player"))
        {
            if (triggerId == "triggerArea")
            {
                if (mode == HazardMode.PlayerTrigger && !playingAnimation)
                {
                    // The player entered the trigger area wile using HazardMode.PlayerTrigger, start the hazard animation
                    StartAnimation();
                }
            }
            else if (triggerId == "hit")
            {
                // The hazard hit the player
                HazardHitPlayer();
            }
        }
    }

    private void HazardHitPlayer()
    {
        // The player was hit by the obstacle, kill them and give the death cause that was set in the inspector

        PlayerStats playerStats = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>();
        playerStats.DecreaseHealth(10000.0f, deathCause);

        // Play an impact sound if one was set in the inspector
        if (impactSound != null)
        {
            AudioManager.Instance.PlaySoundEffect3D(impactSound, transform.position);
        }
    }

    public void OnExternalTriggerStay(string triggerId, Collider other) { }

    public void OnExternalTriggerExit(string triggerId, Collider other) { }

    private IEnumerator ContinuousTriggerCoroutine()
    {
        // Wait for the start delay before triggering the continuous animation for the first time
        yield return new WaitForSeconds(continuousStartDelay);

        while(mode == HazardMode.Continuous)
        {
            // Continuously trigger the hazard animation, then wait for the interval, and repeat

            StartAnimation();

            yield return new WaitForSeconds(continuousInverval);
        }
    }

    public void ImpactEvents()
    {
        // ImpactEvents is called by an animation event

        // Spawn impact particles if any were set in the inspector
        if(impactParticles != null)
        {
            impactParticles.PlayEffect();
        }

        // Find the player's camera and shake it
        CameraShake playerCameraShake = GameObject.FindGameObjectWithTag("Player").GetComponent<CameraShake>();

        if(playerCameraShake != null)
        {
            float distanceFromPlayer = Vector3.Distance(transform.position, playerCameraShake.gameObject.transform.position);

            // The further from the hazard, the less intense screen shake will be
            float shakeIntensity = (0.3f - (distanceFromPlayer * 0.025f)) * cameraShakeMultiplier;

            if(shakeIntensity > 0.0f)
            {
                playerCameraShake.ShakeCameraForTime(0.3f, CameraShakeType.ReduceOverTime, shakeIntensity);
            }
        }

        // Play an impact sound if one was set in the inspector
        if(impactSound != null)
        {
            AudioManager.Instance.PlaySoundEffect3D(impactSound, transform.position);
        }
    }

    private void StartAnimation()
    {
        // Play the hazard animation at normal speed, not reversed

        animator.SetFloat("Speed", 1.0f);

        animator.SetTrigger("StartHazard");

        playingAnimation = true;
    }

    public void ReverseAnimationAfterDelay(float delay)
    {
        // Pause the animation and then reverse it after the given delay (seconds)

        StartCoroutine(ReverseAnimationCoroutine(delay));
    }

    private IEnumerator ReverseAnimationCoroutine(float delay)
    {
        // Pause animation and wait for delay

        animator.SetFloat("Speed", 0.0f);

        yield return new WaitForSeconds(delay);

        // Reverse animation after delay

        animator.SetFloat("Speed", -1.0f);

        reversingAnimation = true;
    }

    // Called by an animation event when the hazard animation is complete
    public void AnimationDone()
    {
        playingAnimation = false;
    }
}
