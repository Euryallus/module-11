using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class TeleportPoint : MonoBehaviour
{
    private BoxCollider triggerVol;
    private Animator animator;

    [SerializeField] private Transform teleportPoint;

    private GameObject player;

    private void Awake()
    {
        triggerVol = gameObject.GetComponent<BoxCollider>();
        triggerVol.isTrigger = true;
        animator = gameObject.GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.transform.CompareTag("Player"))
        {
            player.GetComponent<PlayerMovement>().StopMoving();
            animator.SetTrigger("FadeOut");

        }
    }

    public void TeleportToPoint()
    {
        player.GetComponent<CharacterController>().enabled = false;
        player.transform.position = teleportPoint.transform.position;
        player.GetComponent<CharacterController>().enabled = true;
    }

    public void FinishFade()
    {
        player.GetComponent<PlayerMovement>().StartMoving();
    }
}
