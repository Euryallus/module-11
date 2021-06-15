using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeAbility : PlayerAbility
{
    [Header("Freeze details")]
    public float freezeRadius = 5f;
    [SerializeField]    private GameObject triggerVolPrefab;
                        private GameObject triggerVol;

    [SerializeField]    private GameObject freezeIndicatorPrefab;
                        private GameObject freezeIndicator;

    [SerializeField] private float bubbleUpTime;
    private float activeTime = 0f;



    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if(charging)
        {
            if(freezeIndicator != null)
            {
                freezeIndicator.transform.localScale -= new Vector3(Time.deltaTime / chargeTime, 0.0f,
                                                                    Time.deltaTime / chargeTime);
            }
        }

        if(triggerVol != null)
        {
            activeTime += Time.deltaTime;
            if(activeTime >= bubbleUpTime)
            {
                Destroy(triggerVol);
            }
        }
    }

    protected override void ChargeStart()
    {
        base.ChargeStart();
        freezeIndicator = Instantiate(freezeIndicatorPrefab, transform);
    }

    protected override void ChargeEnd()
    {
        base.ChargeEnd();

        if(freezeIndicator != null)
        {
            Destroy(freezeIndicator);
        }

    }

    protected override void FindUIIndicator()
    {
        uiIndicator = GameObject.FindGameObjectWithTag("FreezeIndicator").GetComponent<AbilityIndicator>();
    }

    protected override void AbilityStart()
    {
        base.AbilityStart();
        activeTime = 0f;

        if (freezeIndicator != null)
        {
            Destroy(freezeIndicator);
        }

        triggerVol = Instantiate(triggerVolPrefab, transform.position + new Vector3(0, -1, 0), Quaternion.identity);
        triggerVol.transform.localScale = new Vector3(freezeRadius, freezeRadius, freezeRadius);

    }

}
