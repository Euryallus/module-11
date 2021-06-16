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

    public void Launch(Vector3 direction, FreezeAbility spawner)
    {
        transform.parent = null;

        transform.forward = -direction;
        move = true;

        parent = spawner;
        gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        gameObject.GetComponent<Rigidbody>().velocity = transform.forward * speed;
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
            if(collision.transform.gameObject.GetComponent<EnemyBase>())
            {
                parent.FreezeEnemy(collision.transform.gameObject.GetComponent<EnemyBase>());

                Debug.Log("DESTROY");
            }
            else
            {
                Destroy(gameObject);
            }
        
        
    }
}
