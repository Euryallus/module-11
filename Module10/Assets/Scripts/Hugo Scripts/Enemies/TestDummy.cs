using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDummy : MonoBehaviour
{
    [SerializeField] private List<MeshRenderer> renderers = new List<MeshRenderer>();
    [SerializeField] private Material hitMat;
    [SerializeField] private Material normalMaterial;

    public void TakeHit()
    {
        foreach(MeshRenderer mesh in renderers)
        {
            mesh.material = hitMat;
            StartCoroutine(ReturnToNormal());
        }
    }

    IEnumerator ReturnToNormal()
    {
        yield return new WaitForSeconds(0.2f);

        foreach (MeshRenderer mesh in renderers)
        {
            mesh.material = normalMaterial;
        }
    }
}
