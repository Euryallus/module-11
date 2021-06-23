using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveHazard : MonoBehaviour
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

    [SerializeField] private MeshRenderer   waveMesh;
    [SerializeField] private GameObject     waveParticles;

    #endregion

    private Vector3 basePosition;
    private float moveIntervalTimer;
    private bool moving;
    private CanvasGroup warningUICanvasGroup;
    private Transform playerTransform;
    private AudioSource waveLoopAudioSource;

    // Start is called before the first frame update
    void Start()
    {
        basePosition = transform.position;

        warningUICanvasGroup = GameObject.FindGameObjectWithTag("WaveWarning").GetComponent<CanvasGroup>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

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

    private void StartMoving()
    {
        moveIntervalTimer = 0.0f;
        moving = true;
        transform.position = basePosition + startPosition;

        waveMesh.enabled = true;
        waveParticles.SetActive(true);

        transform.localScale = new Vector3(1.0f, 0.0f, 1.0f);
        waveMesh.material.SetFloat("_Opacity", 0.0f);

        waveLoopAudioSource = AudioManager.Instance.PlayLoopingSoundEffect("waveLoop", "waveHazardLoop_" + basePosition.x + "_" + basePosition.z, true, transform.position, 30.0f, 100.0f);
    }

    private void MoveWaveUpdate()
    {
        Vector3 endWorldPos = basePosition + endPosition;

        transform.position = Vector3.MoveTowards(transform.position, endWorldPos, Time.deltaTime * moveSpeed);

        float waveOpacity = waveMesh.material.GetFloat("_Opacity");
        if (waveOpacity < 1.0f)
        {
            waveMesh.material.SetFloat("_Opacity", waveOpacity + Time.deltaTime * growSpeed);
        }

        if(transform.localScale.y < 1.0f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, Time.deltaTime * growSpeed);
        }

        if(Vector3.Distance(playerTransform.position, transform.position) <= warningDistance)
        {
            warningUICanvasGroup.alpha = 1.0f;
        }
        else
        {
            warningUICanvasGroup.alpha = 0.0f;
        }

        if(waveLoopAudioSource != null)
        {
            waveLoopAudioSource.gameObject.transform.position = transform.position;
        }

        if(transform.position == endWorldPos)
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
        waveLoopAudioSource = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerStats>().DecreaseHealth(100.0f, PlayerDeathCause.WaveHit);
        }
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
