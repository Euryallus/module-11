using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeAbility : PlayerAbility
{
    [Header("Freeze details")]

    [SerializeField]    private GameObject iceLancePrefab;
                        private GameObject spawnedIceLance;

    [SerializeField] private float range = 50f;
    [SerializeField] private float FreezeDuration = 5f;
    [SerializeField] private Material freezeMaterial;
    private EnemyBase hit;

    private GameObject playerCam;


    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        playerCam = gameObject.GetComponent<PlayerMovement>().playerCamera;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

    }

    protected override void ChargeStart()
    {
        base.ChargeStart();
        spawnedIceLance = Instantiate(iceLancePrefab, transform);
        spawnedIceLance.transform.forward = transform.forward;

        spawnedIceLance.transform.localPosition = new Vector3(1, 1, 1);
        //spawnedIceLance.transform.position = GameObject.FindGameObjectWithTag("PlayerHand").transform.position;

    }

    protected override void ChargeEnd()
    {
        base.ChargeEnd();
        
        Destroy(spawnedIceLance);

    }

    protected override void FindUIIndicator()
    {
        uiIndicator = GameObject.FindGameObjectWithTag("FreezeIndicator").GetComponent<AbilityIndicator>();
    }

    protected override void AbilityStart()
    {
        base.AbilityStart();

        spawnedIceLance = Instantiate(iceLancePrefab, transform);
        spawnedIceLance.transform.forward = transform.forward;

        spawnedIceLance.transform.localPosition = new Vector3(1, 1, 1);

        RaycastHit hit;
        Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out hit, range);

        Vector3 direction = spawnedIceLance.transform.forward;

        if (hit.transform != null)
        {
            direction = spawnedIceLance.transform.position - hit.point;
        }

        spawnedIceLance.GetComponent<IceLanceObject>().Launch(direction.normalized, gameObject.GetComponent<FreezeAbility>());
        
    }

    public void FreezeEnemy(EnemyBase enemyObj)

    {
        if(enemyObj.Suspended)
        {
            // The targeted enemy is currently suspended in the air - apply an ice material, thendrop and kill it
            enemyObj.MeshRenderer.material = freezeMaterial;

            GetComponent<SlamAbility>().DropAndKillSuspendedEnemy(enemyObj, true);

            AudioManager.Instance.PlaySoundEffect3D("iceSmash", enemyObj.gameObject.transform.position);
        }
        else if (enemyObj.AgentEnabled)
        {
            // Only freeze enemies that don't already have their agents disabled
            //   Prevents multiple effects that disable enemy movement from being applied at once

            hit = enemyObj;
            enemyObj.Freeze();

            StartCoroutine(UnFreezeEnemy());
        }
    }

    IEnumerator UnFreezeEnemy()
    {
        yield return new WaitForSeconds(FreezeDuration);
        if(hit != null)
        {
            hit.UnFreeze();
            hit = null;
        }

    }

}
