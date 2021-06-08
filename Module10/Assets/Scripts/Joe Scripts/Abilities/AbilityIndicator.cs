using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityIndicator : MonoBehaviour
{
    [SerializeField] private Image chargeImage;
    [SerializeField] private Image cooldownImage;

    //private float charge;
    //private float cooldown;

    public void SetChargeAmount(float charge)
    {
        chargeImage.fillAmount = charge;
    }

    public void SetCooldownAmount(float cooldown)
    {
        cooldownImage.fillAmount = cooldown;
    }
}
