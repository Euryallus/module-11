using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordSwingAnimEvent : MonoBehaviour
{
    [SerializeField] private MeleeWeapon swordRef;

    public void SwingStart()
    {
        swordRef.Swing();
    }
 }
