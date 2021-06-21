using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyHealthAnim : EnemyHealth
{
    [SerializeField] private Animator anim;

    [SerializeField] private Material hurt;
    [SerializeField] private SkinnedMeshRenderer render;
    [SerializeField] private Material defaultMat;
    public override void DoDamage(float damageAmount)
    {
        base.DoDamage(damageAmount);

        if(health > 0.0f)
        {
            anim.SetTrigger("Take Damage");
            StartCoroutine(FlashHurt());
        }

    }

    protected override void Die()
    {
        //StopAllCoroutines();
        //render.material = defaultMat;

        alive = false;

        gameObject.GetComponent<EnemyBase>().StopAllCoroutines();
        gameObject.GetComponent<EnemyBase>().enabled = false;
        gameObject.GetComponent<EnemyHealthAnim>().enabled = false;
        gameObject.GetComponent<Collider>().enabled = false; 
        gameObject.GetComponent<NavMeshAgent>().enabled = false;

        anim.StopPlayback();
        anim.SetBool("Run Forward", false);
        anim.SetTrigger("Die");
        //anim.enabled = false;
    }

    IEnumerator FlashHurt()
    {
        render.material = hurt;
        yield return new WaitForSeconds(0.2f);
        render.material = defaultMat;
    }

}
