using UnityEngine;

// Module 11

public class FaceCamera : MonoBehaviour
{
    private void LateUpdate()
    {
        Camera playerCamera = Camera.main;

        if (playerCamera != null)
        {
            transform.forward = playerCamera.transform.forward;
        }
    }
}