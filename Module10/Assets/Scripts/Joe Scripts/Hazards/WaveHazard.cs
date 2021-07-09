using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveHazard : CutsceneTriggerer, IExternalTriggerListener
{
    #region InspectorVariables
    // Variables in this region are set in the inspector

    //See tooltips for comments

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

    [SerializeField] [Tooltip("Base material to use for the wave")]
    private Material waveMaterial;

    [SerializeField] [Tooltip("Particles spawned when the player enters a wave")]
    private GameObject waveEnterParticlesPrefab;

    [SerializeField] private MeshRenderer    waveMesh;
    [SerializeField] private GameObject      waveParticles;
    [SerializeField] private ExternalTrigger deathTrigger;
    [SerializeField] private ExternalTrigger waterTrigger;

    #endregion

    private Vector3 basePosition;
    private float moveIntervalTimer;
    private bool moving;
    private CanvasGroup warningUICanvasGroup;
    private Transform playerTransform;
    private AudioSource waveLoopSoundSource;
    private Vector3 startScale;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        basePosition = transform.position;

        warningUICanvasGroup = GameObject.FindGameObjectWithTag("WaveWarning").GetComponent<CanvasGroup>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        deathTrigger.AddListener(this);
        waterTrigger.AddListener(this);

        startScale = transform.localScale;

        waveMesh.material = new Material(waveMaterial);

        StopMoving();
    }

    // Update is called once per frame
    void Update()
    {
        moveIntervalTimer += Time.deltaTime;

        if(moveIntervalTimer > movementInterval && !moving)
        {
            StartMoving();
        }

        if(moving)
        {
            MoveWaveUpdate();
        }
    }

    public void OnExternalTriggerEnter(string triggerId, Collider other)
    {
        if (moving && other.gameObject.CompareTag("Player"))
        {
            if (triggerId == "death")
            {
                // Player collided with the inside of the wave/is submerged

                StartCutscene();
            }
            else if (triggerId == "water")
            {
                // Player collided with the edge of the wave

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
        moveIntervalTimer = 0.0f;
        moving = true;
        transform.position = basePosition + startPosition;

        waveMesh.enabled = true;
        waveParticles.SetActive(true);

        transform.localScale = new Vector3(startScale.x, 0.0f, startScale.z);
        waveMesh.material.SetFloat("_Opacity", 0.0f);

        string loopSoundId = "waveHazardLoop_" + basePosition.x + "_" + basePosition.z;

        waveLoopSoundSource = AudioManager.Instance.PlayLoopingSoundEffect("waveLoop", "waveHazardLoop_" + basePosition.x + "_" + basePosition.z, true, transform.position, 30.0f, 100.0f);
    }

    private void MoveWaveUpdate()
    {
        Vector3 endWorldPos = basePosition + endPosition;

        transform.position = Vector3.MoveTowards(transform.position, endWorldPos, Time.deltaTime * moveSpeed);

        float waveOpacity = waveMesh.material.GetFloat("_Opacity");
        if (waveOpacity < 0.9f)
        {
            waveMesh.material.SetFloat("_Opacity", waveOpacity + Time.deltaTime * growSpeed);
        }

        if(transform.localScale.y < startScale.y)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, startScale, Time.deltaTime * growSpeed);
        }

        Vector3 closestPointToPlayer = waterTrigger.TriggerCollider.ClosestPointOnBounds(playerTransform.position);

        if (Vector3.Distance(playerTransform.position, closestPointToPlayer) <= warningDistance)
        {
            warningUICanvasGroup.alpha = 1.0f;
        }
        else
        {
            warningUICanvasGroup.alpha = 0.0f;
        }

        if(waveLoopSoundSource != null)
        {
            waveLoopSoundSource.gameObject.transform.position = closestPointToPlayer;
        }

        if (transform.position == endWorldPos)
        {
            StopMoving();
        }

    }

    private void StopMoving()
    {
        moving = false;

        waveMesh.enabled = false;
        waveParticles.SetActive(false);

        warningUICanvasGroup.alpha = 0.0f;

        AudioManager.Instance.StopLoopingSoundEffect("waveHazardLoop_" + basePosition.x + "_" + basePosition.z);

        waveLoopSoundSource = null;
    }

    protected override void StartCutscene()
    {
        base.StartCutscene();

        cutsceneCameraParent.position = playerTransform.position;

        StartCoroutine(KillPlayerAfterTime(1.0f));
    }

    private IEnumerator KillPlayerAfterTime(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        // Decrease the player's health by a large amount to kill them
        playerTransform.GetComponent<PlayerStats>().DecreaseHealth(100.0f, PlayerDeathCause.WaveHit);

        AudioManager.Instance.PlaySoundEffect2D("waterExit");
    }

    private void OnDrawGizmos()
    {
#if (UNITY_EDITOR)

        // Gizmos for visualisation of the wave's path in the editor

        Gizmos.color = Color.yellow;

        Gizmos.DrawLine(transform.position + startPosition, transform.position + endPosition);

        Gizmos.color = Color.red;

        Gizmos.DrawSphere(transform.position + startPosition, 0.5f);
        Gizmos.DrawSphere(transform.position + endPosition, 0.5f);

#endif
    }
}
