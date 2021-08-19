using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Adapted version of EnemyHealth - incorperates animations & damage indicator
// Development window:  Production phase
// Inherits from:       EnemyHealth

public class EnemyHealthAnim : EnemyHealth
{
    [SerializeField] private Animator anim;                 // Ref. to enemies animator component
    [SerializeField] private Material hurt;                 // Material enemy flashes when hurt (e.g. flashes white when damaged)
    [SerializeField] private SkinnedMeshRenderer render;    // Ref. to renderer component (used to switch materials)
    [SerializeField] private Material defaultMat;           // Default enemy material (switches to this after flashes "hurt" material)

    public override void DoDamage(float damageAmount, bool destroyOnDeath = false)
    {
        base.DoDamage(damageAmount, destroyOnDeath);
        // Additional check for if enemy is hurt but not dying - plays damaged anim & flashes to indicate damage
        if(health > 0.0f)
        {
            anim.SetTrigger("Take Damage");
            StartCoroutine(FlashHurt());
        }
    }

    protected override void Die()
    {
        // Stops all coroutines & destroys self (playing Death anim caused issues with tracking when a fight was over)
        gameObject.GetComponent<EnemyBase>().StopAllCoroutines();

        if(ParticleManager.Instance != null)
        {
            ParticleManager.Instance.SpawnParticle(transform.position + Vector3.up, "EnemyDeath", transform.forward);
        }


        Destroy(gameObject);
    }

    IEnumerator FlashHurt()
    {
        // Temporeraly changes material used by renderer
        render.material = hurt;
        
        // Wait [x] seconds then switch back to normal material
        yield return new WaitForSeconds(0.2f);
        render.material = defaultMat;
    }

}
