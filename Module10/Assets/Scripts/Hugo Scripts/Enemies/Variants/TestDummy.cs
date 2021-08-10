using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main author:         Hugo Bailey
// Additional author:   N/A
// Description:         Allows target dummies to flash when hit
// Development window:  Production phase
// Inherits from:       MonoBehaviour

public class TestDummy : MonoBehaviour
{
    [SerializeField] private List<MeshRenderer> renderers = new List<MeshRenderer>();   // List of renderers to flash with hitMat
    [SerializeField] private Material hitMat;                                           // Material to flash when hit
    [SerializeField] private Material normalMaterial;                                   // Default material

    public void TakeHit()
    {
        // Cycles each renderer in the renderers list & switches material to hitMat
        foreach(MeshRenderer mesh in renderers)
        {
            mesh.material = hitMat;
        }
        
        // Starts coroutine to return all materials to default
        StartCoroutine(ReturnToNormal());
    }

    IEnumerator ReturnToNormal()
    {
        // After 0.2 seconds all materials are switches back to normal material
        yield return new WaitForSeconds(0.2f);

        foreach (MeshRenderer mesh in renderers)
        {
            mesh.material = normalMaterial;
        }
    }
}
