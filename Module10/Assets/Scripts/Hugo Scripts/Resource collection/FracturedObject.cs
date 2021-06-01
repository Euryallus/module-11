using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Controls resource object that breaks into smaller pieces (e.g. rocks that fracture)
// Development window:  Prototype phase
// Inherits from:       MonoBehaviour
public class FracturedObject : MonoBehaviour
{
    public List<Rigidbody> bodies = new List<Rigidbody>();  // List of rigidbodies in object
    public float force = 20f;                               // Force applied to rigidbodies when object "explodes"

   
    public void Explode()
    { 
        // Adds an explosion force on all fractured parts of the object
        for(int i = 0; i <bodies.Count; i++)
        {
            bodies[i].AddExplosionForce(20f, transform.position, force);
        }
        // Begins despawn "countdown"
        StartCoroutine("Despawn");
    }

    IEnumerator Despawn()
    {
        yield return new WaitForSeconds(3);
        Destroy(gameObject);
    }
}
