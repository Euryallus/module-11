using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceLanceObject : MonoBehaviour
{
    public bool move = false;
    [SerializeField] private float speed = 10f;
    private FreezeAbility parent;

    [SerializeField] private float maxLifetime = 5f;
    private float currentLifetime;

    [SerializeField] private GameObject impactParticles;

    private void Awake()
    {
        gameObject.GetComponent<BoxCollider>().enabled = false;
    }

    public void Launch(Vector3 direction, FreezeAbility spawner)
    {
        transform.parent = null;

        transform.forward = direction;
        move = true;

        parent = spawner;
        gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        gameObject.GetComponent<Rigidbody>().velocity = transform.forward * speed;
        
        gameObject.GetComponent<BoxCollider>().enabled = true;

    }

    private void Update()
    {
        if(move)
        {
            currentLifetime += Time.deltaTime;
            if(currentLifetime >= maxLifetime)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject particles = Instantiate(impactParticles, collision.GetContact(0).point, Quaternion.identity);
        particles.transform.forward = -transform.forward;
        particles.GetComponent<ParticleGroup>().PlayEffect();
        
        if (collision.transform.gameObject.GetComponent<EnemyBase>())
        {
            parent.FreezeEnemy(collision.transform.gameObject.GetComponent<EnemyBase>());
        }

        int mask = 1 << 6;
        if (parent.chainEnemyCount > 0)
        {
            RaycastHit[] surrounding = Physics.SphereCastAll(transform.position, parent.chainDistance, collision.transform.forward, parent.chainDistance, mask, QueryTriggerInteraction.Ignore);

            if(surrounding.Length != 0)
            {
                for (int i = 0; i < parent.chainEnemyCount; i++)
                {
                    parent.FreezeEnemy(surrounding[i].transform.gameObject.GetComponent<EnemyBase>());
                }
            }
        }

        Destroy(gameObject);
    }
}
