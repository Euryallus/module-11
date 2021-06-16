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
    private EnemyBase hit;


    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
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
        spawnedIceLance.transform.position = GameObject.FindGameObjectWithTag("PlayerHand").transform.position;

    }

    protected override void ChargeEnd()
    {
        base.ChargeEnd();

        if(charging)
        {
            Destroy(spawnedIceLance);
        }
    }

    protected override void FindUIIndicator()
    {
        uiIndicator = GameObject.FindGameObjectWithTag("FreezeIndicator").GetComponent<AbilityIndicator>();
    }

    protected override void AbilityStart()
    {
        base.AbilityStart();

        if(spawnedIceLance!= null)
        {
            GameObject playerCam = gameObject.GetComponent<PlayerMovement>().playerCamera;

            RaycastHit hit;
            Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out hit, range);

            Vector3 direction = spawnedIceLance.transform.forward;

            if (hit.transform != null)
            {
                direction = spawnedIceLance.transform.position - hit.point;
                Debug.Log(direction);
            }

            spawnedIceLance.GetComponent<IceLanceObject>().Launch(direction.normalized, gameObject.GetComponent<FreezeAbility>());
        }
    }

    public void FreezeEnemy(EnemyBase enemyObj)
    {
        hit = enemyObj;
        enemyObj.StopAgentMovement();

        StartCoroutine(UnFreezeEnemy());
    }

    IEnumerator UnFreezeEnemy()
    {
        yield return new WaitForSeconds(FreezeDuration);
        hit.StartAgentMovement();
    }

}
