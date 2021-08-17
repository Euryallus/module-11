using System.Collections;
using UnityEngine;

// ||=======================================================================||
// || WaveHazard: A large wave that moves linearly from a start point to an ||
// ||   end point and kills the player on impact.                           ||
// ||=======================================================================||
// || Used on prefab: Joe/Environment/Wave                                  ||
// ||=======================================================================||
// || Written by Joseph Allen                                               ||
// || for the production phase (Module 11).                                 ||
// ||=======================================================================||

public class WaveHazard : CutsceneTriggerer, IExternalTriggerListener
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    // See tooltips for comments

    [Space]
    [Header("(See tooltips for info)")]
    [Header("Wave Hazard")]

    [SerializeField] [Tooltip("The starting position of the wave (relative to the position of the GameObject)")]
    private Vector3 startPosition;

    [SerializeField] [Tooltip("The position the wave will move towards (relative to the position of the GameObject)")]
    private Vector3 endPosition;

    [SerializeField] [Tooltip("How frequently the wave movement will be triggered (seconds)")]
    private float   movementInterval;

    [SerializeField] [Tooltip("How quickly the wave will move from startPosition to endPosition")]
    private float   moveSpeed = 10.0f;

    [SerializeField] [Tooltip("How quickly the wave grow as it first emerges from the water")] 
    private float   growSpeed = 1.0f;

    [SerializeField] [Tooltip("How close the player has to be to the wave before warning UI is shown")]
    private float   warningDistance = 120.0f;

    [SerializeField] [Tooltip("How far the wave has to be from its starting point before it has 'passed' and is no longer a threat")]
    private float wavePassedDistance = 450.0f;

    [SerializeField] [Tooltip("The minimum Y position that the player can be at to be considered 'safe' from getting hit by the wave")]
    private float safeYPos = 95.5f;

    [SerializeField] [Tooltip("Base material to use for the wave")]
    private Material waveMaterial;

    [SerializeField] [Tooltip("Particles spawned when the player enters a wave")]
    private GameObject waveEnterParticlesPrefab;

    [SerializeField] private MeshRenderer    waveMeshRenderer;  // Renderer that the wave material is applied to
    [SerializeField] private GameObject      waveParticles;     // Particles spawned in front of the wave for a foam effect
    [SerializeField] private ExternalTrigger deathTrigger;      // Trigger that detects when the player is submerged in the wave and should be killed
    [SerializeField] private ExternalTrigger waterTrigger;      // Trigger that detects when the player enters the wave's water

    #endregion

    private Vector3     basePosition;           // The starting position of the wave
    private Vector3     startScale;             // The starting scale of the wave mesh

    private float       moveIntervalTimer;      // The minimum amount of time that can pass between the wave moving
    private bool        moving;                 // Whether the wave is currently moving

    private CanvasGroup warningUICanvasGroup;   // Canvas group used to show/hide the warning UI
    private AudioSource waveLoopSoundSource;    // The looping sound effect played as the wave moves

    protected override void Start()
    {
        base.Start();

        // Add this class as a listener for the external triggers so OnExternalTriggerEnter will be called when either is entered
        deathTrigger.AddListener(this);
        waterTrigger.AddListener(this);

        basePosition = transform.position;
        startScale = transform.localScale;

        // Set this wave's material to be an instance of waveMaterial so properties
        //   can be adjusted without affecting the material of other waves
        waveMeshRenderer.material = new Material(waveMaterial);

        warningUICanvasGroup = GameObject.FindGameObjectWithTag("WaveWarning").GetComponent<CanvasGroup>();

        // Stop the wave from moving until [moveIntervalTimer] seconds have passed
        StopMoving();
    }

    // Update is called once per frame
    void Update()
    {
        // Increment moveIntervalTimer each frame
        moveIntervalTimer += Time.deltaTime;

        if(moveIntervalTimer > movementInterval && !moving)
        {
            // The move interval has been reached and the wave is not already moving, start moving
            StartMoving();
        }

        if(moving)
        {
            // Update the wave while moving
            MoveWaveUpdate();
        }
    }

    public void OnExternalTriggerEnter(string triggerId, Collider other)
    {
        if (moving && other.gameObject.CompareTag("Player"))
        {
            if (triggerId == "death")
            {
                // Player collided with the inside of the wave/is submerged, start the death cutscene
                StartCutscene();
            }
            else if (triggerId == "water")
            {
                // Player collided with the edge of the wave, play a splash sound and
                //   spawn some splash particles to cover the player's screen

                AudioManager.Instance.PlaySoundEffect2D("splash");
                if (Camera.main != null)
                {
                    Instantiate(waveEnterParticlesPrefab, Camera.main.transform);
                }
            }
        }
    }

    public void OnExternalTriggerStay(string triggerId, Collider other) { }

    public void OnExternalTriggerExit(string triggerId, Collider other) { }

    private void StartMoving()
    {
        // Reset the movement interval
        moveIntervalTimer = 0.0f;
        moving = true;

        // Move the wave to its initial position - its base position in the world plus the start offset set in the inspector
        transform.position = basePosition + startPosition;

        // Show the wave mesh and particles
        waveMeshRenderer.enabled = true;
        waveParticles.SetActive(true);

        // Shrink the wave down on the y-axis so it can emerge from the water
        transform.localScale = new Vector3(startScale.x, 0.0f, startScale.z);

        // Set the material's opacity to 0.0 so the wave can 'fade in'
        waveMeshRenderer.material.SetFloat("_Opacity", 0.0f);

        // Start the looping wave sound
        waveLoopSoundSource = AudioManager.Instance.PlayLoopingSoundEffect("waveLoop", "waveHazardLoop_" + basePosition.x + "_" + basePosition.z, true, false, transform.position, 30.0f, 100.0f);
    }

    private void MoveWaveUpdate()
    {
        // Move the wave towards the target position (its base position in the world plus the end offset set in the inspector)
        transform.position = Vector3.MoveTowards(transform.position, basePosition + endPosition, Time.deltaTime * moveSpeed);

        // While the wave has not reached max opacity, increase its material's opacity value each frame
        float waveOpacity = waveMeshRenderer.material.GetFloat("_Opacity");
        if (waveOpacity < 0.9f)
        {
            waveMeshRenderer.material.SetFloat("_Opacity", waveOpacity + Time.deltaTime * growSpeed);
        }

        // Also 'grow' the wave each frame by increasing its scale towards the default/start scale
        if(transform.localScale.y < startScale.y)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, startScale, Time.deltaTime * growSpeed);
        }

        Transform playerTransform = PlayerInstance.ActivePlayer.transform;

        // Calculate the closest point to the player that is on the boundary of the wave mesh
        Vector3 closestPointToPlayer = waterTrigger.TriggerCollider.ClosestPointOnBounds(playerTransform.position);
        
        // Calculate the distance between the player and the closest point on the wave
        float waveDistanceFromPlayer = Vector3.Distance(playerTransform.position, closestPointToPlayer);

        // Calculate the distance between the wave's current position and its starting position
        float waveDistanceFromStart  = Vector3.Distance(transform.position, basePosition + startPosition);

        if (waveDistanceFromPlayer <= warningDistance &&
            waveDistanceFromStart < wavePassedDistance &&
            playerTransform.position.y < safeYPos)
        {
            // The wave is 1. within warning distance, 2. has not moved past wavePassedDistance, and
            //   3. the player is low enough to be at risk of getting hit. Show the warning UI
            warningUICanvasGroup.alpha = 1.0f;
        }
        else
        {
            // The player is in a safe position, do not show the warning UI
            warningUICanvasGroup.alpha = 0.0f;
        }

        // Move the wave sound source to be at the closest point on the wave mesh to the player while it's active
        if(waveLoopSoundSource != null)
        {
            waveLoopSoundSource.gameObject.transform.position = closestPointToPlayer;
        }

        // Stop the wave moving once it reached the target end position
        if (transform.position == (basePosition + endPosition))
        {
            StopMoving();
        }

    }

    private void StopMoving()
    {
        moving = false;

        // Hide the wave mesh and particles
        waveMeshRenderer.enabled = false;
        waveParticles.SetActive(false);

        // Hide the warning UI in case it's still showing
        warningUICanvasGroup.alpha = 0.0f;

        // Stop the looping wave sound
        AudioManager.Instance.StopLoopingSoundEffect("waveHazardLoop_" + basePosition.x + "_" + basePosition.z);
        waveLoopSoundSource = null;
    }

    public override void StartCutscene()
    {
        base.StartCutscene();

        // Focus the cutscene camera on the player
        cutsceneCameraParent.position = PlayerInstance.ActivePlayer.transform.position;

        // Kill the player after showing the cutscene for a second
        StartCoroutine(KillPlayerAfterTime(1.0f));
    }

    private IEnumerator KillPlayerAfterTime(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        // Decrease the player's health by a large amount to kill them
        PlayerInstance.ActivePlayer.PlayerStats.DecreaseHealth(10000.0f, PlayerDeathCause.WaveHit);

        // Play a splash sound to emphasise death by water
        AudioManager.Instance.PlaySoundEffect2D("waterExit");
    }

    private void OnDrawGizmos()
    {
#if (UNITY_EDITOR)

        // Gizmos for visualisation of the wave's path in the editor - draws a line from the start to end position with spheres at each end

        Gizmos.color = Color.yellow;

        Gizmos.DrawLine(transform.position + startPosition, transform.position + endPosition);

        Gizmos.color = Color.red;

        Gizmos.DrawSphere(transform.position + startPosition, 0.5f);
        Gizmos.DrawSphere(transform.position + endPosition, 0.5f);

#endif
    }
}
